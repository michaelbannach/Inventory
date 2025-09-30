# Inventory (ASP.NET Core 8 + React)

Kleine Lagerverwaltung (Items, Typen, Eigenschaften, Buchungen). Fokus: saubere Struktur, DTO-Boundary, ProblemDetails, Migrations.

## Stack
- Backend: ASP.NET Core 8, EF Core (SQL Server), Swagger, Health
- Frontend: Vite + React + TypeScript + MUI/DataGrid

## Lokal starten
**Backend**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<dein-conn-string>"
dotnet user-secrets set "FrontendOrigin" "http://localhost:5173"
dotnet ef database update
dotnet run
