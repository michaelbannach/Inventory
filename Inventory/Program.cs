// Program.cs
using System.Text.Json;
using Inventory.Data;
using Inventory.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// --- Konfigquellen: appsettings, ENV, (Dev) User-Secrets ---------------------
builder.Configuration
    // <- WICHTIG: optional = true, damit das Projekt auch ohne echte appsettings.json läuft
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

// --- CORS-Origin(s) aus Config ----------------------------------------------
// Entweder einzelner String "FrontendOrigin": "http://localhost:5173"
// ODER Array "FrontendOrigins": ["http://localhost:5173","http://127.0.0.1:5173"]
var singleOrigin = builder.Configuration["FrontendOrigin"];
var multipleOrigins = builder.Configuration.GetSection("FrontendOrigins").Get<string[]>();
var frontendOrigins = (multipleOrigins?.Length > 0
        ? multipleOrigins
        : (string.IsNullOrWhiteSpace(singleOrigin) ? Array.Empty<string>() : new[] { singleOrigin }))
    .DefaultIfEmpty("http://localhost:5173") // Fallback Dev
    .ToArray();

// --- ConnectionString: Fail-Fast, wenn nicht gesetzt -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("__SET_IN_USER_SECRETS__"))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection ist nicht gesetzt. " +
        "Bitte per User-Secrets (Dev) oder ENV (Prod) konfigurieren.");
}

// --- Services ----------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Services.AddProblemDetails();           // saubere Fehler (RFC7807)
builder.Services.AddHealthChecks();             // /health

builder.Services.AddDbContextPool<InventoryContext>(opt =>
    opt.UseSqlServer(connectionString));

builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p => p
        .WithOrigins(frontendOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// --- Swagger nur im Dev ------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Pipeline ----------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Bequemer Dev-Helper: ausstehende Migrationen anwenden
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<InventoryContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();   // nutzt ProblemDetails
// app.UseHttpsRedirection(); // optional in Dev

app.UseCors("frontend");

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
