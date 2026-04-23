namespace TaskPilot.Models;

/// <summary>Predefiniowany szablon zadania (np. typowe sprawy w firmie).</summary>
public sealed class TaskTemplateDefinition
{
    public required string DisplayName { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;
    public string AssignedTo { get; init; } = string.Empty;
    public string ClientProject { get; init; } = string.Empty;

    /// <summary>Liczba dni od dzisiaj do terminu (0 = dziś).</summary>
    public int DueInDays { get; init; }
}

public static class TaskTemplateCatalog
{
    public static IReadOnlyList<TaskTemplateDefinition> All { get; } =
        new TaskTemplateDefinition[]
        {
            new()
            {
                DisplayName = "Błąd / incydent (IT)",
                Title = "Błąd: ",
                Description =
                    "Środowisko (np. produkcja / test):\n" +
                    "Objawy:\n" +
                    "Kroki odtworzenia:\n" +
                    "Oczekiwane vs rzeczywiste zachowanie:",
                Category = "IT",
                Priority = TaskPriority.High,
                DueInDays = 2,
                ClientProject = "Incydenty"
            },
            new()
            {
                DisplayName = "Wdrożenie / funkcja",
                Title = "Feature: ",
                Description =
                    "Cel biznesowy:\n" +
                    "Zakres (MVP):\n" +
                    "Ryzyka / zależności:\n" +
                    "Kryteria akceptacji:",
                Category = "Projekt",
                Priority = TaskPriority.Medium,
                DueInDays = 7,
                ClientProject = "Produkt"
            },
            new()
            {
                DisplayName = "Spotkanie z klientem",
                Title = "Spotkanie: ",
                Description =
                    "Cel spotkania:\n" +
                    "Agenda:\n" +
                    "Materiały do przygotowania:\n" +
                    "Następne kroki po spotkaniu:",
                Category = "Sprzedaż",
                Priority = TaskPriority.Medium,
                DueInDays = 3,
                ClientProject = "Klienci"
            },
            new()
            {
                DisplayName = "Code review",
                Title = "Review: ",
                Description =
                    "Link do PR / gałęzi:\n" +
                    "Obszary do sprawdzenia:\n" +
                    "Uwagi wstępne:",
                Category = "Dev",
                Priority = TaskPriority.Medium,
                DueInDays = 1,
                ClientProject = "Jakość kodu"
            },
            new()
            {
                DisplayName = "Onboarding osoby",
                Title = "Onboarding: ",
                Description =
                    "Stanowisko / zespół:\n" +
                    "Lista kont i dostępów:\n" +
                    "Szkolenia do odbycia:\n" +
                    "Buddy / osoba wspierająca:",
                Category = "HR",
                Priority = TaskPriority.Low,
                DueInDays = 14,
                ClientProject = "HR"
            },
            new()
            {
                DisplayName = "Zadanie operacyjne (powtarzalne)",
                Title = "Operacja: ",
                Description =
                    "Procedura / checklista:\n" +
                    "Termin realizacji:\n" +
                    "Potwierdzenie wykonania (komu zgłosić):",
                Category = "Operacje",
                Priority = TaskPriority.Low,
                DueInDays = 0,
                ClientProject = "Backoffice"
            }
        };
}
