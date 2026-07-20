# Banco_Ruby — Proyecto mínimo

Este repositorio contiene el microservicio `Banco_Ruby`, organizado con un `Program.cs` minimalista y extensiones para registro de servicios, endpoints y pipeline de middleware.

Estructura relevante

- `Program.cs` — entrada minimalista que llama a extensiones.
- `Extensions/ServiceExtensions.cs` — registra `DbContext`, filtros y gateway.
- `Extensions/EndpointExtensions.cs` — mapa de endpoints (saldo, depósito, retiro, transferencia, historial).
- `Extensions/MiddlewareExtensions.cs` — pipeline de middleware (HTTPS, manejo de errores, status pages).
- `Common/` — modelos y DTOs.
- `Infrastructure/` — implementaciones, incluido `BancoRubyDbContext`.
- `Domain/` y `Features/` — lógica del dominio y slices.

Ejecutar en desarrollo

1. Restaurar y compilar:

```bash
dotnet build Banco_Ruby/Banco_Ruby.csproj
```

2. Ejecutar:

```bash
dotnet run --project Banco_Ruby/Banco_Ruby.csproj
```

3. Tests:

```bash
dotnet test BancoRuby.Tests/BancoRuby.Tests.csproj
```

Notas

- Los cambios realizados para limpiar `Program.cs` se encuentran en las extensiones bajo `Extensions/`.
- No se hicieron commits automáticos; revisa y commitea cuando prefieras.
