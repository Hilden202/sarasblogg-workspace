# SarasBlogg – Monorepo Workspace

Detta repo samlar hela **SarasBlogg-ekosystemet** i ett gemensamt workspace (monorepo), med tydligt separerade projekt för frontend och backend.

**Syftet är att:**
- ha en gemensam solution
- behålla tydlig ansvarsfördelning
- förenkla lokal utveckling, drift och vidareutveckling

---

## 🧱 Struktur

```text
sarasblogg-workspace/
├── Frontend/                # Razor Pages frontend (SarasBlogg)
├── API/                     # Backend API (Identity, DB, media-hantering)
├── SarasBlogg-Workspace.sln # Gemensam solution
├── sync-media.ps1           # Lokalt DEV-verktyg för mediasynk
└── README.md                # Detta dokument
```

## 🎯 Arkitekturprinciper
### Frontend (SarasBlogg Razor Pages)

- Razor Pages

- Pratar endast med API via HTTP

- Ingen DbContext eller direkt databasåtkomst

- Ingen lokal bildlagring i produktion

### Backend (SarasBlogg API)

- Äger all data, Identity och roller

- Ansvarar för bildhantering

- PostgreSQL (produktion via Render)

- Stöd för lokal media-hantering i DEV

### Media

- Produktionsbilder ligger i separat GitHub-repo (sarasblogg-media)

- Lokal utveckling använder en ignorerad lokal mapp

- Synk sker manuellt via script

---

## 🖼 Media & Bilder
### Produktion

Bilder hämtas från sarasblogg-media (raw GitHub).

### Lokal utveckling

API använder en lokal mapp (gitignorerad), t.ex:
```text
API/SarasBlogg-Media/
```
### DEV-verktyg (valfritt)

sync-media.ps1 synkar bilder från GitHub-repot till din lokala miljö.

---

## ☁️ Deployment (Render)

- Samma GitHub-repo

- Två separata Render-services:

 - Frontend: SarasBlogg

 - API: SarasBloggAPI

- Olika rootDir

- All konfiguration via environment variables

---

## 🧪 Lokal utveckling (kort)

1. Klona repot

2. Lägg lokala inställningar i secrets.json / appsettings.Development.json

3. Starta SarasBloggAPI och därefter SarasBlogg

Frontend fungerar endast när API är igång (by design).

---

## 🧠 Status

- Monorepo etablerat

- Gamla repos finns kvar under övergången (rollback möjligt)




