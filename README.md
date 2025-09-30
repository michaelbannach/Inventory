# Inventory (ASP.NET Core 8 + React)

Kleine Lagerverwaltung (Items, Typen, Eigenschaften, Buchungen). Fokus: saubere Struktur, DTO-Boundary, ProblemDetails, Migrations.

## Features

- Artikel inkl. Beschreibung, Typ, kritische Menge

- Artikelarten mit frei definierbaren Eigenschaften

- Wareneingänge &  Warenausgänge (mehrere Positionen)

- Standorte (Raum, Regal, Fach) & Bestandsanzeige je Raum

- Suche/Filter (Name, Beschreibung, Typ, Raum)

- DTO-Boundary, ProblemDetails (RFC 7807), Swagger/OpenAPI

- EF Core (SQL Server) mit Migrations

## Stack
- Backend: ASP.NET Core 8, EF Core (SQL Server), Swagger, Health
- Frontend: Vite + React + TypeScript + MUI/DataGrid

## Screenshots

<p align="center">
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-35-21%20Items%20%C3%9Cbersicht.png" alt="Items Übersicht 1" width="32%" />
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-35-33%20Items%20%C3%9Cbersicht.png" alt="Items Übersicht 2" width="32%" />
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-36-29%20Items%20%E2%80%93%20Raum%201.png" alt="Items – Raum 1 (1)" width="32%" />
</p>
<p align="center">
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-36-43%20Items%20%E2%80%93%20Raum%201.png" alt="Items – Raum 1 (2)" width="32%" />
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-37-00%20Items%20%E2%80%93%20Raum%201.png" alt="Items – Raum 1 (3)" width="32%" />
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-37-13%20Items%20%C3%9Cbersicht.png" alt="Items Übersicht 3" width="32%" />
</p>
<p align="center">
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-37-24%20Items%20%C3%9Cbersicht.png" alt="Items Übersicht 4" width="32%" />
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-37-41%20Items%20%C3%9Cbersicht.png" alt="Items Übersicht 5" width="32%" />
  <img src="docs/screenshots/Screenshot%202025-09-29%20at%2020-37-57%20Items%20%C3%9Cbersicht.png" alt="Items Übersicht 6" width="32%" />
</p>

## Lokal starten
**Backend**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<dein-conn-string>"
dotnet user-secrets set "FrontendOrigin" "http://localhost:5173"
dotnet ef database update
dotnet run
