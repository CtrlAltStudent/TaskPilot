using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using TaskPilot.Models;

namespace TaskPilot.Services;

/// <summary>
/// Zapis listy zadań w jednej tabeli SQLite (prosty zapis całej listy przy każdej zmianie).
/// </summary>
public sealed class SqliteTaskPersistence : ITaskPersistence
{
    private readonly string _databasePath;

    public SqliteTaskPersistence(string databasePath)
    {
        _databasePath = databasePath;
    }

    public string StorageDescription => $"Baza SQLite: {_databasePath}";

    public IReadOnlyList<TaskItem> Load()
    {
        try
        {
            EnsureDatabaseFileDirectoryExists();
            using var connection = OpenConnection();
            EnsureSchema(connection);
            MigrateLegacyColumns(connection);

            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                """
                SELECT Id, Title, Description, Category, DueDate, Priority, IsCompleted,
                       AssignedTo, ClientProject, CreatedUtc, UpdatedUtc
                FROM Tasks
                ORDER BY Id;
                """;

            var list = new List<TaskItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TaskItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Category = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    DueDate = ReadDate(reader, 4),
                    Priority = (TaskPriority)reader.GetInt32(5),
                    IsCompleted = reader.GetInt32(6) != 0,
                    AssignedTo = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    ClientProject = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    CreatedUtc = ReadUtc(reader, 9),
                    UpdatedUtc = ReadUtc(reader, 10)
                });
            }

            return list;
        }
        catch (SqliteException ex)
        {
            System.Windows.MessageBox.Show(
                $"Błąd odczytu bazy SQLite.\n{ex.Message}\n\nZostanie użyta pusta lista.",
                "TaskPilot — błąd odczytu",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return Array.Empty<TaskItem>();
        }
    }

    public void Save(IEnumerable<TaskItem> tasks)
    {
        EnsureDatabaseFileDirectoryExists();
        using var connection = OpenConnection();
        EnsureSchema(connection);
        MigrateLegacyColumns(connection);

        using var tx = connection.BeginTransaction();
        using (var delete = connection.CreateCommand())
        {
            delete.Transaction = tx;
            delete.CommandText = "DELETE FROM Tasks;";
            delete.ExecuteNonQuery();
        }

        foreach (var t in tasks)
        {
            using var insert = connection.CreateCommand();
            insert.Transaction = tx;
            insert.CommandText =
                """
                INSERT INTO Tasks (Id, Title, Description, Category, DueDate, Priority, IsCompleted,
                    AssignedTo, ClientProject, CreatedUtc, UpdatedUtc)
                VALUES ($id, $title, $desc, $cat, $due, $prio, $done, $assign, $client, $created, $updated);
                """;
            insert.Parameters.AddWithValue("$id", t.Id);
            insert.Parameters.AddWithValue("$title", t.Title ?? string.Empty);
            insert.Parameters.AddWithValue("$desc", t.Description ?? string.Empty);
            insert.Parameters.AddWithValue("$cat", t.Category ?? string.Empty);
            insert.Parameters.AddWithValue("$due", t.DueDate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            insert.Parameters.AddWithValue("$prio", (int)t.Priority);
            insert.Parameters.AddWithValue("$done", t.IsCompleted ? 1 : 0);
            insert.Parameters.AddWithValue("$assign", t.AssignedTo ?? string.Empty);
            insert.Parameters.AddWithValue("$client", t.ClientProject ?? string.Empty);
            insert.Parameters.AddWithValue("$created", FormatUtc(t.CreatedUtc));
            insert.Parameters.AddWithValue("$updated", FormatUtc(t.UpdatedUtc));
            insert.ExecuteNonQuery();
        }

        tx.Commit();
    }

    private static string FormatUtc(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return utc.ToString("o", CultureInfo.InvariantCulture);
    }

    private SqliteConnection OpenConnection()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        };

        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();
        }

        return connection;
    }

    private static void EnsureSchema(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER NOT NULL PRIMARY KEY,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                Category TEXT NOT NULL,
                DueDate TEXT NOT NULL,
                Priority INTEGER NOT NULL,
                IsCompleted INTEGER NOT NULL,
                AssignedTo TEXT NOT NULL DEFAULT '',
                ClientProject TEXT NOT NULL DEFAULT '',
                CreatedUtc TEXT NOT NULL DEFAULT '',
                UpdatedUtc TEXT NOT NULL DEFAULT ''
            );
            """;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Istniejące bazy z 7 kolumnami — dopisanie pól firmowych + uzupełnienie znaczników czasu.
    /// </summary>
    private static void MigrateLegacyColumns(SqliteConnection connection)
    {
        var columns = GetColumnNames(connection, "Tasks");

        void AddColumnIfMissing(string name, string ddl)
        {
            if (columns.Contains(name, StringComparer.OrdinalIgnoreCase))
                return;

            using var alter = connection.CreateCommand();
            alter.CommandText = ddl;
            alter.ExecuteNonQuery();
            columns.Add(name);
        }

        AddColumnIfMissing("AssignedTo", "ALTER TABLE Tasks ADD COLUMN AssignedTo TEXT NOT NULL DEFAULT '';");
        AddColumnIfMissing("ClientProject", "ALTER TABLE Tasks ADD COLUMN ClientProject TEXT NOT NULL DEFAULT '';");
        AddColumnIfMissing("CreatedUtc", "ALTER TABLE Tasks ADD COLUMN CreatedUtc TEXT NOT NULL DEFAULT '';");
        AddColumnIfMissing("UpdatedUtc", "ALTER TABLE Tasks ADD COLUMN UpdatedUtc TEXT NOT NULL DEFAULT '';");

        using (var fillCreated = connection.CreateCommand())
        {
            fillCreated.CommandText =
                """
                UPDATE Tasks
                SET CreatedUtc = strftime('%Y-%m-%dT%H:%M:%SZ','now')
                WHERE CreatedUtc IS NULL OR trim(CreatedUtc) = '';
                """;
            fillCreated.ExecuteNonQuery();
        }

        using (var fillUpdated = connection.CreateCommand())
        {
            fillUpdated.CommandText =
                """
                UPDATE Tasks
                SET UpdatedUtc = strftime('%Y-%m-%dT%H:%M:%SZ','now')
                WHERE UpdatedUtc IS NULL OR trim(UpdatedUtc) = '';
                """;
            fillUpdated.ExecuteNonQuery();
        }
    }

    private static HashSet<string> GetColumnNames(SqliteConnection connection, string table)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info({table});";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            set.Add(r.GetString(1));

        return set;
    }

    private void EnsureDatabaseFileDirectoryExists()
    {
        var dir = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    private static DateTime ReadDate(SqliteDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return DateTime.Today;

        var s = reader.GetString(ordinal);
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
            return dt.Date;

        return DateTime.Today;
    }

    private static DateTime ReadUtc(SqliteDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return DateTime.UtcNow;

        var s = reader.GetString(ordinal);
        if (string.IsNullOrWhiteSpace(s))
            return DateTime.UtcNow;

        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            return dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();

        return DateTime.UtcNow;
    }
}
