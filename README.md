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
## Screenshots

## Screenshots

### Items Übersicht (1)
Startliste aller Artikel mit Suche, Artikeltyp, Menge, kritischer Menge und letzter Änderung.
![Items Übersicht 1](docs/screenshots/Screenshot%202025-09-29%20at%2020-35-21%20Items%20%C3%9Cbersicht.png)

---

### Items Übersicht (2)
Weitere Zeilen/Beispiele – zeigt, wie lange Beschreibungen und Typ-Links im Grid dargestellt werden.
![Items Übersicht 2](docs/screenshots/Screenshot%202025-09-29%20at%2020-35-33%20Items%20%C3%9Cbersicht.png)

---

### Artikel im Raum (Ansicht 1)
Gefilterte Ansicht für **einen Raum** (z. B. „Raum 1“) – zeigt nur Bestände an diesem Standort.
![Items – Raum 1 (1)](docs/screenshots/Screenshot%202025-09-29%20at%2020-36-29%20Items%20%E2%80%93%20Raum%201.png)

---

### Artikel im Raum (Ansicht 2)
Gleiche Raumansicht mit weiteren Zeilen; Artikeltyp ist klickbar, um nach Typ zu filtern.
![Items – Raum 1 (2)](docs/screenshots/Screenshot%202025-09-29%20at%2020-36-43%20Items%20%E2%80%93%20Raum%201.png)

---

### Artikel im Raum (Ansicht 3)
Raumansicht mit aktiver Top-Suche – schnelle Kombination aus Standort- und Textfilter.
![Items – Raum 1 (3)](docs/screenshots/Screenshot%202025-09-29%20at%2020-37-00%20Items%20%E2%80%93%20Raum%201.png)

---

### Dialoge/Listen – Beispiel 1
Beispielhafte UI-Interaktion (z. B. Anlage/Filter) – zeigt konsistente MUI-Dialoge und Validierung.
![Beispiel 1](docs/screenshots/Screenshot%202025-09-29%20at%2020-37-13%20Items%20%C3%9Cbersicht.png)

---

### Wareneingänge (Liste)
Übersicht der zuletzt gebuchten **Wareneingänge** mit Positionszusammenfassung und Zeitstempel.
![Wareneingänge](docs/screenshots/Screenshot%202025-09-29%20at%2020-37-24%20Items%20%C3%9Cbersicht.png)

---

### Warenausgänge (Liste)
Übersicht der zuletzt gebuchten **Warenausgänge** mit Positionszusammenfassung und Zeitstempel.
![Warenausgänge](docs/screenshots/Screenshot%202025-09-29%20at%2020-37-41%20Items%20%C3%9Cbersicht.png)

---

### Dialoge/Listen – Beispiel 2
Weiteres Interaktionsbeispiel – konsistentes Look-and-Feel über alle Dialoge/Formulare.
![Beispiel 2](docs/screenshots/Screenshot%202025-09-29%20at%2020-37-57%20Items%20%C3%9Cbersicht.png)


## Lokal starten
**Backend**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<dein-conn-string>"
dotnet user-secrets set "FrontendOrigin" "http://localhost:5173"
dotnet ef database update
dotnet run
