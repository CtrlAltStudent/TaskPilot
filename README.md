# TaskPilot

Desktopowa aplikacja **WPF** (.NET 8) — menedżer zadań zgodny z projektem zespołowym (MVVM, binding, komendy, zapis danych do pliku JSON).

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

## Uruchomienie w Visual Studio

1. **Plik → Otwórz → Folder** i wskaż folder sklonowanego repozytorium `TaskPilot` **lub** podfolder `TaskPilot` z plikiem `TaskPilot.csproj`.
2. Ustaw projekt **TaskPilot** jako projekt startowy (jeśli VS nie wykryje automatycznie).
3. **F5** (debug) lub **Ctrl+F5** (bez debugera).

## Gdzie aplikacja zapisuje dane

Zadania są zapisywane w pliku JSON w profilu użytkownika Windows:

`%LocalAppData%\TaskPilot\tasks.json`

Pełna ścieżka przykładowo:

`C:\Users\<TwojaNazwaUżytkownika>\AppData\Local\TaskPilot\tasks.json`

Na innym komputerze plik powstaje automatycznie przy pierwszym zapisie. Katalog `TaskPilot` w `LocalAppData` tworzy się sam.

## Rozwiązywanie typowych problemów

| Problem | Co zrobić |
|--------|-----------|
| `dotnet` nie jest rozpoznawane | Zainstaluj **.NET 8 SDK** i **uruchom terminal ponownie** (lub zrestartuj IDE). |
| Błąd kompilacji `net8.0-windows` | Użyj **Windows** i SDK 8.x; na starszym SDK zainstaluj aktualizację. |
| Aplikacja się nie uruchamia poza Windows | To normalne — projekt to **WPF** (tylko Windows). |
| Pusty lub uszkodzony `tasks.json` | Przy błędnym JSON aplikacja pokaże ostrzeżenie i uruchomi się z pustą listą; możesz skasować plik, aby „zresetować” dane. |

## Struktura repozytorium (skrót)

```
TaskPilot/                 ← katalog główny Git (README, .gitignore)
└── TaskPilot/             ← projekt WPF
    ├── TaskPilot.csproj
    ├── App.xaml
    ├── MainWindow.xaml
    ├── Models/
    ├── ViewModels/
    ├── Services/          ← m.in. zapis/odczyt JSON, walidacja
    └── Converters/
```

## Skład zespołu (deklaracja projektu)

- Michał Skawski  
- Hubert Łukowski  

Temat: **TaskPilot — menedżer zadań** (aplikacja desktopowa WPF).
