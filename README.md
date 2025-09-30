# Inventory (ASP.NET Core 8 + React)

Kleine Lagerverwaltung (Items, Typen, Eigenschaften, Buchungen). Fokus: saubere Struktur, DTO-Boundary, ProblemDetails, Migrations.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=000)
![Vite](https://img.shields.io/badge/Vite-React%20TS-646CFF?logo=vite&logoColor=fff)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4)

## Inhalt
- [Features](#features)
- [Stack](#stack)
- [Lokal starten](#lokal-starten)
- [Screenshots](#screenshots)
- [API](#api)
- [Projektstruktur](#projektstruktur)

---

## Features

- Artikel inkl. Beschreibung, Typ, Menge & kritischer Menge  
- Artikelsuche (Serverfilter) & Typ-/Raum-Filter
- Dynamische Eigenschaften pro Artikeltyp (PropertyDefinitions)
- Ein-/Ausgänge mit mehreren Positionen (StockIn/StockOut)
- Saubere Fehlerausgabe via **ProblemDetails** (RFC 7807)
- Swagger UI & Health-Endpoint
- EF Core Migrations (SQL Server)

---

## Stack

- **Backend:** ASP.NET Core 8, EF Core (SQL Server), Swagger, Health
- **Frontend:** Vite + React + TypeScript + MUI/DataGrid

---

## Lokal starten

### Backend
```bash
cd Inventory

# 1) User-Secrets initialisieren und Konfiguration setzen
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Inventory_db;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "FrontendOrigin" "http://localhost:5173"

# 2) Datenbank migrieren
dotnet ef database update

# 3) Starten (standardmäßig http://localhost:5195)
dotnet run
# Swagger: http://localhost:5195/swagger
# Health:  http://localhost:5195/health
