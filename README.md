# SarasBlogg – Monorepo Workspace

Detta repo samlar hela **SarasBlogg-ekosystemet** i ett gemensamt workspace (monorepo),  
med tydligt separerade projekt för frontend och backend.

Syftet är att:
- ha **en gemensam solution**
- behålla **tydlig ansvarsfördelning**
- förenkla lokal utveckling, drift och vidareutveckling

---

## 🧱 Struktur

```text
sarasblogg-workspace/
├── SarasBlogg/           # Razor Pages frontend
├── SarasBloggAPI/        # Backend API (Identity, DB, media-hantering)
├── SarasBlogg.sln        # Gemensam solution
├── sync-media.ps1        # Lokalt DEV-verktyg för mediasynk
└── README.md             # Detta dokument
