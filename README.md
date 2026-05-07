# SarasBlogg – Monorepo Workspace

SarasBlogg är ett API-first monorepo där backend är systemets kärna och frontend är utbytbar.

Projektet är byggt för att stödja flera klienter mot samma API-kontrakt:

- Razor Pages (nuvarande stabil frontend)
- SvelteKit (nästa generations frontend)
- framtida mobilapp eller andra klienter

API:t är redan frikopplat från frontend och äger all auth, dataintegritet och affärslogik.

---

# 🧱 Struktur

```text
sarasblogg-workspace/
├── Frontend/                 # Razor Pages frontend (nuvarande produktion)
├── Client/                   # SvelteKit-klient (under utveckling)
├── API/                      # ASP.NET API (Identity, auth, DB, business logic)
├── APITests/                 # Integrationstester
├── SarasBlogg-Workspace.sln  # Gemensam solution
├── sync-media.ps1            # DEV-script för mediasynk
└── README.md
```

---

# 🎯 Arkitektur

## API-first

API:t är source of truth.

Frontend är endast konsument av API-kontraktet.

Systemet är byggt för att möjliggöra:

- frontend-oberoende utveckling
- flera klienter
- centraliserad auth
- stabila DTO-kontrakt
- backend-driven affärslogik

---

# 🧠 Ansvarsfördelning

## Backend (API)

API äger:

- Databas
- Identity
- Roller
- Auth
- Claims
- Ownership
- Moderering
- ViewCount
- Timestamps
- Dataintegritet
- Externa integrationer
- Bildhantering
- Fallback-värden

Teknik:

- ASP.NET Core API
- EF Core
- PostgreSQL
- Identity
- JWT + refresh flow
- Cookie-baserad refresh/sessionhantering
- Typed HttpClients
- Polly retry policies

---

## Frontend

Frontend äger:

- Presentation
- UX
- Routing
- State
- UI-transformationer
- Rendering

Frontend får aldrig anta databasstruktur.

---

# 🎨 Frontends

## Frontend/ (Razor Pages)

Nuvarande stabil frontend.

- Fungerar som API-klient
- Ingen DbContext
- Ingen backend-logik
- Ingen säkerhetslogik som auktoritet
- All data hämtas via API

Razor används fortfarande aktivt och måste behandlas varsamt vid cleanup/refaktorering.

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

- all API-kommunikation sker via services
- inga direkta fetch-anrop i komponenter
- error mapping sker i services
- DTOs typas
- komponenter hålls presentationsnära

---

# 🔐 Auth-arkitektur

Auth hanteras helt av API:t.

Google OAuth/OIDC är implementerat backend-first.

Flödet:

1. Frontend skickar användare till API
2. API hanterar Google OIDC
3. API skapar JWT/access token
4. Refresh token lagras som HttpOnly-cookie
5. Frontend använder Bearer-token mot API

Designprincip:

```text
Backend = auth authority
Frontend = token consumer
Google = identity provider
```

Detta möjliggör:

- multi-origin auth
- frontend-oberoende auth
- framtida mobilappar
- säkrare tokenhantering

---

# 🗄️ Databas & Infrastruktur

## Databas

- PostgreSQL
- Produktion körs på Render
- Lokalt används Development/Test-databaser

## Hosting

- API körs på Render
- Razor frontend körs på Render
- SvelteKit kommer kunna deployas separat

## Reverse proxy / drift

Forwarded headers används för korrekt proxy-hantering:

- scheme forwarding
- HTTPS-detektering
- korrekt redirect-url i auth-flöden

---

# 🖼️ Mediahantering

## Produktion

Media ligger i separat repository:

```text
sarasblogg-media
```

Bilder hämtas via GitHub raw/GitHub Pages.

## Lokal utveckling

Lokal mediafolder används:

```text
API/SarasBlogg-Media/
```

(gitignorerad)

## DEV-sync

```powershell
sync-media.ps1
```

synkar media från GitHub till lokal miljö.

---

# 🧩 API-kontrakt

## Viktiga principer

- DTOs är klientkontrakt
- Frontend skickar aldrig databasspecifika fält
- Backend sätter ownership och säkerhetskritiska värden
- Legacy payloads är bortstädade

---

# 📝 Bloggflöde

## Backend-owned

API sätter:

- UserId
- ViewCount
- fallback title
- timestamps
- LaunchDate-normalisering

Frontend skickar:

```json
BlogPostWriteRequest
```

Frontend får inte skicka:

- id
- userId
- viewCount
- createdAt
- launchDate
- ownership-data

---

# 💬 Kommentarssystem

Kommentarer är API-drivna.

API hanterar:

- moderation
- forbidden words
- ownership
- authenticated username
- anonymous fallback (`"Gäst"`)
- timestamps

Frontend skickar endast:

```json
CommentCreateRequest
```

---

# 👁️ ViewCount

ViewCount är backend-owned.

Princip:

- ViewCount ökas i API när specifik bloggpost hämtas
- Frontend ska aldrig incrementera views själv
- Blogglistor hämtar färsk data från API
- Frontend-cache för blogglistor används inte om den riskerar stale ViewCount

---

# 🧪 Tester

## Integrationstester

Projekt:

```text
APITests/
```

Tester körs mot isolerad PostgreSQL-miljö via Testcontainers.

Fokus:

- API-kontrakt
- auth
- kommentarer
- view count
- public/private blogflöden
- moderation
- DTO-validering

CI kör:

```bash
dotnet test SarasBlogg-Workspace.sln
```

---

# 🧹 Refaktorering & cleanup

Projektet har nyligen genomgått större cleanup där frontend-owned business logic tagits bort.

Borttaget:

- frontend ownership-logik
- frontend moderation-logik
- legacy payloads
- stale cache-logik
- duplicerad auth/business logic
- oanvända helpers och dead code

Målet är:

```text
API = business authority
Frontend = presentation
```

---

# ⚠️ Viktigt vid vidareutveckling

## Razor

Var försiktig med:

- hidden fields
- modal-state
- model binding
- editor lifecycle
- date formatting

Ta inte bort Razor-kod utan att förstå flödet.

---

## API-kontrakt

Tänk alltid:

- är detta breaking change?
- påverkar detta andra klienter?
- är detta backend- eller frontend-ansvar?
- är DTO:t ett kontrakt eller en DB-modell?

---

# 🧠 Monorepo-medvetenhet

Var uppmärksam på:

- Node-version
- .NET-version
- CORS
- SameSite/Secure cookies
- VITE_ env vs server-env
- Render rootDir
- auth redirect URLs
- integrationstester när API redan kör lokalt

---

# ☁️ Deployment

## Render services

Separata services:

- SarasBlogg (frontend)
- SarasBloggAPI (backend)

Samma GitHub-repo används.

Konfiguration sker via environment variables.

---

# 📦 Backup & Restore

Backup-script använder:

```powershell
pg_dump
```

Backup sparas lokalt och loggas.

Återställning sker via pgAdmin med:

```text
Clean before restore
```

för att undvika FK-konflikter.

---

# 🚀 Status

## Klart

- ✅ API-first arkitektur etablerad
- ✅ Frontend frikopplad från databasen
- ✅ Backend-owned auth
- ✅ Google OAuth/OIDC backend-driven
- ✅ Integrationstester
- ✅ ViewCount backend-driven
- ✅ Legacy payload cleanup
- ✅ Frontend business logic cleanup
- ✅ Monorepo etablerat
- ✅ Typed HttpClients + Polly
- ✅ Shared DataProtection keys
- ✅ Refresh token flow

---

# 🔮 Planerat

- SvelteKit som primär frontend
- Mobilklient
- Tarotsystem
- Betalflöden
- Utökad AI-funktionalitet
- GitHub Pages-hostad frontend möjlig
- Fler API-klienter

---

# 🃏 Tarotkortssystem (planerat)

Kommande subsystem i API:t.

Planerat:

- inloggningskrav
- dagliga kortdragningar
- AI-tolkning
- moderation av frågor
- framtida betalflöden
- egen app/site på sikt

---

# 🔗 Snabblänkar

## Produktion

- 🌐 Frontend: https://sarasblogg.onrender.com
- 🔗 API: https://sarasbloggapi.onrender.com

## Övrigt

- 🌍 Hilden Media: https://hildenmedia.se
- 💻 GitHub: https://github.com/Hilden202
- 🖼 Media repo: https://github.com/Hilden202/sarasblogg-media

---

# 📚 Intern dokumentation

Flera interna designbeslut och auth-flöden finns dokumenterade separat, bland annat:

- Google OAuth-flöde
- JWT + refresh-strategi
- multi-origin auth
- monorepo auth setup
- Render proxy/scheme-hantering

Se intern dokumentation för detaljerade auth-sekvenser och implementationstänk.