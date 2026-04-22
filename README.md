# TaskPilot

Desktopowa aplikacja **WPF** (.NET 8) — menedżer zadań zgodny z projektem zespołowym (MVVM, binding, komendy, zapis danych do **JSON** lub opcjonalnie do **SQLite**).

## Wymagania środowiska

| Element | Wersja / uwagi |
|--------|----------------|
| **System operacyjny** | **Windows** (WPF nie uruchomisz na Linuxie/macOS w tej konfiguracji) |
| **.NET SDK** | **8.0** lub nowszy z obsługą `net8.0-windows` ([pobierz SDK](https://dotnet.microsoft.com/download/dotnet/8.0)) |
| **IDE (opcjonalnie)** | Visual Studio 2022 z obciążeniem **Programowanie aplikacji klasycznych w .NET** albo JetBrains Rider |

Sprawdzenie w terminalu:

```powershell
dotnet --version
```

Powinna być widoczna wersja **8.x** (lub nowsza zgodna z projektem).

## Pobranie kodu na obcy komputer (Git)

```powershell
git clone https://github.com/CtrlAltStudent/TaskPilot.git
cd TaskPilot
```

*(Jeśli repozytorium ma inną nazwę lub URL, zamień powyższy adres na właściwy.)*

## Uruchomienie z linii poleceń (zalecane na „obcym” środowisku)

Z **katalogu głównego repozytorium** (tam, gdzie leży ten plik `README.md`):

```powershell
dotnet restore .\TaskPilot\TaskPilot.csproj
dotnet build .\TaskPilot\TaskPilot.csproj -c Release
dotnet run --project .\TaskPilot\TaskPilot.csproj -c Release
```

W trybie debug (szybsza kompilacja przy pracy):

```powershell
dotnet run --project .\TaskPilot\TaskPilot.csproj
```

Pierwsze uruchomienie może pobrać pakiety NuGet i potrwać dłużej.

## Funkcje (skrót)

- Lista zadań w **DataGrid** + formularz szczegółów (MVVM, `RelayCommand`).
- **Filtry**: status, priorytet, kategoria, wyszukiwanie po tytule/opisie/kategorii.
- **Sortowanie** po terminie lub tytule.
- **Motyw**: jasny / ciemny / zgodny z systemem.
- **Import / eksport** listy do pliku JSON (format wspólny z domyślnym zapisem).
- **Walidacja** (m.in. pusty tytuł, termin w przeszłości dla zadań niewykonanych).
- **Opcjonalny zapis w SQLite** (patrz niżej: zmienna środowiskowa `TASKPILOT_STORAGE`).

## Jak przetestować (checklista ręczna)

1. `dotnet build .\TaskPilot\TaskPilot.csproj -c Release` — kompilacja bez błędów.
2. `dotnet run --project .\TaskPilot\TaskPilot.csproj -c Release` — aplikacja startuje.
3. **Dodaj zadanie** — nowy wiersz pojawia się na liście, formularz się włącza, status wskazuje zapis.
4. Edytuj tytuł na pusty i spróbuj **Zapisz teraz** — oczekiwane ostrzeżenie walidacji.
5. **Filtry i sortowanie** — zmiana opcji od razu ogranicza/układa listę.
6. **Usuń** — pojawia się potwierdzenie, po „Tak” wiersz znika i zapis odświeża dane.
7. **Eksportuj… / Importuj…** — eksport do wybranego pliku, import zastępuje listę (po potwierdzeniu).
8. Zamknij aplikację i uruchom ponownie — lista powinna wrócić z tego samego źródła (JSON lub SQLite, zależnie od trybu).

## Uruchomienie w Visual Studio

1. **Plik → Otwórz → Folder** i wskaż folder sklonowanego repozytorium `TaskPilot` **lub** podfolder `TaskPilot` z plikiem `TaskPilot.csproj`.
2. Ustaw projekt **TaskPilot** jako projekt startowy (jeśli VS nie wykryje automatycznie).
3. **F5** (debug) lub **Ctrl+F5** (bez debugera).

## Gdzie aplikacja zapisuje dane

### Domyślnie: JSON

Zadania są zapisywane w pliku JSON w profilu użytkownika Windows:

`%LocalAppData%\TaskPilot\tasks.json`

Pełna ścieżka przykładowo:

`C:\Users\<TwojaNazwaUżytkownika>\AppData\Local\TaskPilot\tasks.json`

Na innym komputerze plik powstaje automatycznie przy pierwszym zapisie. Katalog `TaskPilot` w `LocalAppData` tworzy się sam.

### Opcjonalnie: SQLite

Jeśli przed uruchomieniem ustawisz zmienną środowiskową **`TASKPILOT_STORAGE=sqlite`**, aplikacja użyje bazy:

`%LocalAppData%\TaskPilot\tasks.db`

Przykład w PowerShellu (tylko bieżące okno terminala):

```powershell
$env:TASKPILOT_STORAGE = "sqlite"
dotnet run --project .\TaskPilot\TaskPilot.csproj
```

**Uwaga:** JSON i SQLite to **dwa niezależne magazyny** — przełączenie trybu nie migruje automatycznie danych z jednego do drugiego.

## Rozwiązywanie typowych problemów

| Problem | Co zrobić |
|--------|-----------|
| `dotnet` nie jest rozpoznawane | Zainstaluj **.NET 8 SDK** i **uruchom terminal ponownie** (lub zrestartuj IDE). |
| Błąd kompilacji `net8.0-windows` | Użyj **Windows** i SDK 8.x; na starszym SDK zainstaluj aktualizację. |
| Aplikacja się nie uruchamia poza Windows | To normalne — projekt to **WPF** (tylko Windows). |
| Pusty lub uszkodzony `tasks.json` | Przy błędnym JSON aplikacja pokaże ostrzeżenie i uruchomi się z pustą listą; możesz skasować plik, aby „zresetować” dane. |
| Uszkodzona baza `tasks.db` (tryb SQLite) | Usuń plik `tasks.db` w `%LocalAppData%\TaskPilot\` albo wyłącz tryb SQLite, aby wrócić do JSON. |

## Struktura repozytorium (skrót)

```
TaskPilot/                 ← katalog główny Git (README, .gitignore)
└── TaskPilot/             ← projekt WPF
    ├── TaskPilot.csproj
    ├── App.xaml
    ├── MainWindow.xaml
    ├── Models/
    ├── ViewModels/
    ├── Services/          ← m.in. zapis JSON/SQLite, walidacja
    └── Converters/
```

## Skład zespołu (deklaracja projektu)

- Michał Skawski  
- Hubert Łukowski  

Temat: **TaskPilot — menedżer zadań** (aplikacja desktopowa WPF).
