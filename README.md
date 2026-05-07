# SarasBlogg – Monorepo Workspace

SarasBlogg är ett API-first monorepo där backend är systemets kärna och frontend är utbytbar.

Projektet är byggt för att stödja flera klienter mot samma API-kontrakt:

- Razor Pages (nuvarande stabil frontend)
- SvelteKit (under utveckling)
- framtida klienter och integrationer

API:t är frikopplat från frontend och ansvarar för auth, dataintegritet och affärslogik.

---

# 🧱 Struktur

```text
sarasblogg-workspace/
├── Frontend/                 # Razor Pages frontend
├── Client/                   # SvelteKit-klient
├── API/                      # ASP.NET API
├── APITests/                 # Integrationstester
├── SarasBlogg-Workspace.sln  # Gemensam solution
├── sync-media.ps1            # DEV-script för mediasynk
└── README.md
```

---

# 🎯 Arkitektur

## API-first

API:t är source of truth.

Frontend fungerar som konsument av API-kontraktet.

Systemet är byggt för att möjliggöra:

- flera klienter
- stabila DTO-kontrakt
- centraliserad auth
- tydlig ansvarsfördelning
- frikopplad frontend-utveckling

---

# 🧠 Ansvarsfördelning

## Backend (API)

API:t ansvarar för:

- Databas
- Identity & auth
- Roller & claims
- Affärslogik
- Dataintegritet
- Moderering
- Bildhantering
- ViewCount
- Externa integrationer

Teknik:

- ASP.NET Core API
- EF Core
- PostgreSQL
- Identity
- JWT/auth-flöden
- Typed HttpClients
- Polly retry policies

---

## Frontend

Frontend ansvarar för:

- Presentation
- UX
- Routing
- State
- UI-anpassning av data

Frontend får inte anta databasstruktur eller duplicera backend-logik.

---

# 🎨 Frontends

## Frontend/ (Razor Pages)

Nuvarande stabil frontend.

- API-driven
- Ingen direkt databasåtkomst
- All data hämtas via API

---

## Client/ (SvelteKit)

Nästa generations frontend.

Teknik:

- SvelteKit
- TypeScript
- Vite

Principer:

```text
routes/
components/
services/
stores/
lib/
```

Regler:

- API-anrop sker via services
- inga direkta fetch-anrop i komponenter
- DTOs typas
- komponenter hålls presentationsnära

---

# 🔐 Auth

Auth hanteras av API:t.

Systemet använder backend-driven auth med tokenbaserade API-anrop och centraliserad sessionshantering.

Målet är att auth-flödet ska fungera oberoende av frontend-teknik.

---

# 🗄️ Databas & Infrastruktur

## Databas

- PostgreSQL
- Produktion körs på Render
- Lokala development/test-miljöer stöds

## Hosting

- API körs på Render
- Razor frontend körs på Render
- Frontends kan deployas separat

---

# 🖼️ Mediahantering

## Produktion

Media ligger i separat repository:

```text
sarasblogg-media
```

## Lokal utveckling

Lokal mediafolder används i utvecklingsmiljö.

## DEV-sync

```powershell
sync-media.ps1
```

kan användas för att synka media lokalt.

---

# 🧩 API-kontrakt

DTOs används som klientkontrakt.

Backend ansvarar för:

- ownership
- timestamps
- moderation
- fallback-värden
- dataintegritet

Frontend skickar endast det som behövs för användarens handling.

---

# 📝 Blogg & kommentarer

## Blogg

Blog create/update använder DTO-baserade requests.

API:t ansvarar för:

- UserId
- ViewCount
- fallback title
- timestamps
- dataintegritet

---

## Kommentarer

Kommentarssystemet är API-drivet.

API:t ansvarar för:

- moderation
- ownership
- authenticated username
- timestamps
- AI-baserad innehållsanalys och filtrering

Frontend skickar endast nödvändig data för skapande av kommentarer.

---

# 👁️ ViewCount

ViewCount hanteras helt av backend.

- Detaljhämtning ökar ViewCount i API:t
- Blogglistor hämtar uppdaterad data från API:t
- Frontend ska inte incrementera views lokalt

---

# 🧪 Tester

## Integrationstester

Projekt:

```text
APITests/
```

Fokus:

- API-kontrakt
- auth
- kommentarer
- moderation
- view count
- DTO-validering

CI kör:

```bash
dotnet test SarasBlogg-Workspace.sln
```

Frontend testas för närvarande huvudsakligen manuellt.

---

# 🧹 Refaktorering

Projektet har successivt refaktorerats mot tydligare API-first-principer.

Fokusområden:

- borttagning av duplicerad frontend-logik
- tydligare ansvarsfördelning
- stabilare API-kontrakt
- cleanup av legacy-flöden
- bättre separation mellan UI och affärslogik

---

# ⚠️ Viktigt vid vidareutveckling

## Razor

Var försiktig med:

- hidden fields
- model binding
- modal-state
- form lifecycle
- date formatting

---

## API-kontrakt

Tänk alltid:

- är detta en breaking change?
- påverkar detta flera klienter?
- är detta backend- eller frontend-ansvar?

---

# 🧠 Monorepo

Var uppmärksam på:

- Node-version
- .NET-version
- CORS
- cookies/auth
- environment variables
- build/deploy-påverkan

---

# ☁️ Deployment

Separata Render-services används för frontend och API.

Konfiguration sker via environment variables.

---

# 📦 Backup

PostgreSQL-backups hanteras via scripts och utvecklingsverktyg för lokal återställning och testning.

---

# 🚀 Status

## Klart

- ✅ API-first arkitektur etablerad
- ✅ Frontend frikopplad från databasen
- ✅ Integrationstester
- ✅ Backend-driven ViewCount
- ✅ Cleanup av legacy payloads
- ✅ Typed HttpClients + Polly
- ✅ Monorepo etablerat
- ✅ AI-baserad kommentarsmoderering

---

# 🔮 Planerat

- Vidareutveckling av SvelteKit-klienten
- Fler API-klienter/integrationer
- Utökad AI-funktionalitet
- Förbättrad frontend-upplevelse

---

# 🔗 Snabblänkar

## Produktion

- 🌐 Frontend: https://sarasblogg.onrender.com
- 🔗 API: https://sarasbloggapi.onrender.com

## Övrigt

- 🌍 Hilden Media: https://hildenmedia.se
- 💻 GitHub: https://github.com/Hilden202
- 🖼 Media repo: https://github.com/Hilden202/sarasblogg-media