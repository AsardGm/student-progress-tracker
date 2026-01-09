# Student Progress Tracker

Real-time aplikace pro sledování pokroku studentů učitelem. Blazor Server aplikace s AI asistencí (Google Gemini) pro analýzu práce studentů.

## Funkce

### Pro učitele
- **Google přihlášení** - Autentizace pomocí Google účtu
- **Správa skupin** - Vytváření skupin s vlastními cíli
- **QR kódy** - Automatické generování QR kódů pro snadné připojení studentů
- **Real-time monitoring** - Sledování pokroku všech studentů v reálném čase
- **Notifikace** - Upozornění když student potřebuje pomoc

### Pro studenty
- **Snadné připojení** - Stačí naskenovat QR kód nebo zadat 6-místný kód
- **Chat s AI** - Textový chat s AI asistentem, který navádí k řešení
- **Zobrazení pokroku** - Progress bary a checkmarky pro sledování vlastního pokroku
- **Podpora matematiky** - LaTeX výrazy pomocí KaTeX ($x^2 + 2x + 1$)
- **Hlasové zadávání** - Možnost diktovat text pomocí Web Speech API

### AI funkce (Google Gemini)
- Analýza zpráv studentů
- Detekce pokroku v plnění cílů
- Detekce nečinnosti a off-topic konverzace
- Automatické vedení a nápovědy

## Technologie

- **Backend**: .NET 9, Blazor Server
- **Databáze**: SQL Server LocalDB + Entity Framework Core
- **Real-time**: SignalR
- **Autentizace**: ASP.NET Core Identity + Google OAuth
- **AI**: Google Gemini API
- **UI**: Tailwind CSS + Radzen Blazor
- **QR**: QRCoder
- **Math**: KaTeX
- **Syntax highlighting**: Prism.js

## Požadavky

- .NET 9 SDK
- SQL Server LocalDB (nebo jiná SQL Server instance)
- Google OAuth credentials
- Google Gemini API klíč

## Instalace

1. **Klonování repozitáře**
```bash
cd scio
```

2. **Obnovení NuGet balíčků**
```bash
cd src/StudentProgressTracker
dotnet restore
```

3. **Konfigurace**

Upravte `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentProgressTracker;Trusted_Connection=True;"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-1.5-flash"
  }
}
```

4. **Vytvoření databáze**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. **Spuštění aplikace**
```bash
dotnet run
```

Aplikace bude dostupná na `https://localhost:7001`

## Získání API klíčů

### Google OAuth
1. Přejděte na [Google Cloud Console](https://console.cloud.google.com/)
2. Vytvořte nový projekt nebo vyberte existující
3. Přejděte do APIs & Services > Credentials
4. Vytvořte OAuth 2.0 Client ID
5. Nastavte Authorized redirect URIs: `https://localhost:7001/signin-google`

### Google Gemini API
1. Přejděte na [Google AI Studio](https://aistudio.google.com/)
2. Vytvořte API klíč
3. Zkopírujte klíč do konfigurace

## Struktura projektu

```
src/StudentProgressTracker/
├── Components/
│   ├── Layout/          # Layout komponenty
│   ├── Pages/
│   │   ├── Teacher/     # Učitelský modul
│   │   └── Student/     # Studentský modul
│   └── Shared/          # Sdílené komponenty
├── Data/                # DbContext a migrace
├── Models/              # Databázové entity
├── Services/            # Business logika
├── Hubs/                # SignalR huby
└── wwwroot/             # Statické soubory
```

## Použití

### Učitel
1. Přihlaste se pomocí Google účtu
2. Vytvořte novou skupinu s cíli
3. Sdílejte QR kód nebo kód skupiny se studenty
4. Sledujte pokrok v real-time monitoru

### Student
1. Naskenujte QR kód nebo otevřete odkaz
2. Zadejte svou přezdívku
3. Chatujte s AI asistentem
4. Plňte zadané cíle

## Typy cílů

- **Boolean (Splněno/Nesplněno)** - Zobrazeno jako checkmark
- **Percentage (Procenta)** - Zobrazeno jako progress bar (např. "vyřeš 3 rovnice" = 0%, 33%, 66%, 100%)

## Bonusové funkce

- ✅ Detail studenta s klíčovými zprávami
- ✅ KaTeX pro matematické výrazy
- ✅ Syntax highlighting pro kód (Prism.js)
- ✅ Hlasové diktování (Web Speech API)

## Deployment

### Proč ne Vercel?
Blazor Server vyžaduje **WebSocket** spojení a **persistentní server** (SignalR). Vercel podporuje pouze statické stránky a serverless funkce, proto není kompatibilní s touto aplikací.

### Alternativy pro deployment:
- **Azure App Service** (doporučeno pro .NET)
- **Railway.app**
- **Render.com**
- **Fly.io**

### Lokální spuštění pro demo:
```bash
cd src/StudentProgressTracker
dotnet run --launch-profile https
```
Aplikace poběží na `https://localhost:7241`

## Poznámky k implementaci

**Rozdělení práce (~40% já / ~60% AI):**

**Moje práce (40%):**
- Definice požadavků a zadání
- Výběr technologií
- Gemini API integrace
- Testování a validace
- Finální úpravy a review
- Konfigurace prostředí

**AI asistence (60%):**
- Návrh architektury podle mých požadavků
- Implementace kódu
- Databázový model a EF konfigurace
- UI komponenty a styling
- SignalR integrace
- Dokumentace a komentáře

**Čas strávený tvorbou**: ~5 hodin

**AI systémy použité**:
- Claude Code (Anthropic) via CLI

## Licence

MIT
