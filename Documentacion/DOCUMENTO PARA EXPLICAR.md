# Documento para explicar

Este documento muestra el código central de cada archivo y explica qué hace cada bloque. Está organizado con el nombre del archivo, el código y su comentario.

---

## Banco_Ruby/Program.cs

| Código | Qué hace |
|---|---|
| ```csharp
builder.Services.AddDbContext<BancoRubyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BancoRuby")));
``` | Registra el contexto de EF Core con la cadena de conexión `BancoRuby` para PostgreSQL. |
| ```csharp
builder.Services.AddScoped<AccountAuthorizationFilter>();
``` | Registra el filtro que valida cuentas en cada petición. |
| ```csharp
app.MapGet("/health", () => Results.Ok(new { status = "OK" }))
   .WithName("Health");
``` | Define el endpoint de salud básica. |
| ```csharp
app.MapGet("/saldo/{numeroCuenta}", async (string numeroCuenta, BancoRubyDbContext db) =>
{
    return await AutenticacionSlice.ConsultarSaldoAsync(numeroCuenta, db);
})
.WithName("ConsultarSaldo")
.AddEndpointFilter<AccountAuthorizationFilter>();
``` | Ruta de consulta de saldo que usa `AutenticacionSlice` y valida la cuenta con el filtro. |
| ```csharp
app.MapPost("/deposito", DepositarSlice.DepositarAsync)
    .WithName("Depositar")
    .AddEndpointFilter<AccountAuthorizationFilter>();
app.MapPost("/retiro", RetirarSlice.RetirarAsync)
    .WithName("Retirar")
    .AddEndpointFilter<AccountAuthorizationFilter>();
app.MapPost("/transferencia", TransferirSlice.TransferirAsync)
    .WithName("Transferir")
    .AddEndpointFilter<AccountAuthorizationFilter>();
app.MapGet("/historial/{numeroCuenta}", HistorialSlice.ObtenerAsync)
    .WithName("Historial")
    .AddEndpointFilter<AccountAuthorizationFilter>();
``` | Registra los endpoints principales de depósito, retiro, transferencia y historial. Cada uno usa su slice y el filtro compartido. |
| ```csharp
app.MapPost("/api/cuentas/{numero}/autenticar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
{
    Cuenta? cuenta = await db.Cuentas.Include(c => c.Usuario).AsNoTracking().FirstOrDefaultAsync(c => c.NumeroCuenta == numero && c.Estado);
    if (cuenta is null) return Results.NotFound(new { error = "Cuenta no encontrada o inactiva." });

    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
    }
    catch
    {
    }

    return Results.Ok(new { titular = cuenta.Usuario?.Nombre, cuenta = cuenta.NumeroCuenta });
});
``` | Endpoint de compatibilidad con el cliente `Usuario_Cliente`; devuelve titular y número de cuenta, pero no valida el PIN. |
| ```csharp
app.MapPost("/api/cuentas/{numero}/depositar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
{
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("Monto", out JsonElement m) && !root.TryGetProperty("monto", out m))
        {
            return Results.BadRequest(new { error = "Cuerpo inválido" });
        }

        decimal monto = m.GetDecimal();
        return await DepositarSlice.DepositarAsync(new DepositoRequest(numero, monto), db);
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido" });
    }
})
.AddEndpointFilter<AccountAuthorizationFilter>();
``` | Convierte el JSON del cliente antiguo en `DepositoRequest` y ejecuta la lógica de depósito. |
| ```csharp
app.MapPost("/api/cuentas/{numero}/retirar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
{
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("Monto", out JsonElement m) && !root.TryGetProperty("monto", out m))
        {
            return Results.BadRequest(new { error = "Cuerpo inválido" });
        }

        decimal monto = m.GetDecimal();
        return await RetirarSlice.RetirarAsync(new RetiroRequest(numero, monto), db);
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido" });
    }
})
.AddEndpointFilter<AccountAuthorizationFilter>();
``` | Igual que depósito, pero para la ruta de retiro del cliente antiguo. |
| ```csharp
app.MapPost("/api/cuentas/{numeroOrigen}/transferir", async (string numeroOrigen, HttpRequest req, BancoRubyDbContext db) =>
{
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        JsonElement root = doc.RootElement;
        string cuentaDestino = root.GetProperty("CuentaDestino").GetString() ?? string.Empty;
        decimal monto = root.GetProperty("Monto").GetDecimal();

        return await TransferirSlice.TransferirAsync(new TransferenciaRequest(numeroOrigen, cuentaDestino, monto), db);
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido para transferencia." });
    }
})
.AddEndpointFilter<AccountAuthorizationFilter>();
``` | Adaptador de transferencia para el cliente; toma `CuentaDestino` y `Monto` del JSON y llama al slice. |
| ```csharp
app.MapGet("/api/cuentas/{numero}/historial", async (string numero, BancoRubyDbContext db) =>
{
    return await HistorialSlice.ObtenerAsync(numero, db);
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.Run();
``` | Finaliza y ejecuta la aplicación. |

---

## Banco_Ruby/BancoRubyDbContext.cs

| Código | Qué hace |
|---|---|
| ```csharp
public DbSet<Usuario> Usuarios => Set<Usuario>();
public DbSet<Cuenta> Cuentas => Set<Cuenta>();
public DbSet<Auditoria> Auditoria => Set<Auditoria>();
``` | Define las tablas del modelo de datos: usuarios, cuentas y auditorías. |
| ```csharp
entity.ToTable("usuario");
entity.HasKey(e => e.UsuarioId);
entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired();
entity.HasMany(e => e.Cuentas).WithOne(e => e.Usuario).HasForeignKey(e => e.UsuarioId);
``` | Mapea la entidad `Usuario` a la tabla `usuario` y establece la relación con cuentas. |
| ```csharp
entity.ToTable("cuenta");
entity.HasKey(e => e.CuentaId);
entity.Property(e => e.NumeroCuenta).HasColumnName("numero_cuenta").IsRequired();
entity.HasMany(e => e.Auditorias).WithOne(e => e.Cuenta).HasForeignKey(e => e.CuentaId);
``` | Mapea `Cuenta` a la tabla `cuenta` y conecta sus auditorías. |
| ```csharp
entity.ToTable("auditoria");
entity.HasKey(e => e.AuditoriaId);
entity.Property(e => e.Tipo).HasColumnName("tipo").IsRequired();
entity.Property(e => e.Descripcion).HasColumnName("descripcion").IsRequired();
``` | Mapea `Auditoria` a la tabla `auditoria` y define sus columnas principales. |

---

## Banco_Ruby/Common/Cuenta.cs

| Código | Qué hace |
|---|---|
| ```csharp
public sealed class Usuario
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = default!;
    public string Pin { get; set; } = default!;
    public DateTime CreadoEn { get; set; }
    public List<Cuenta> Cuentas { get; set; } = new();
}
``` | Modelo de usuario con su PIN y lista de cuentas. |
| ```csharp
public sealed class Cuenta
{
    public int CuentaId { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string NumeroCuenta { get; set; } = default!;
    public decimal Saldo { get; set; }
    public bool Estado { get; set; }
    public DateTime CreadoEn { get; set; }
    public List<Auditoria> Auditorias { get; set; } = new();
}
``` | Modelo de cuenta con saldo, estado y las auditorías asociadas. |
| ```csharp
public sealed class Auditoria
{
    public int AuditoriaId { get; set; }
    public int CuentaId { get; set; }
    public Cuenta? Cuenta { get; set; }
    public string NumeroCuenta { get; set; } = default!;
    public string Tipo { get; set; } = default!;
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = default!;
    public DateTime CreadoEn { get; set; }
}
``` | Registro de movimiento que almacena tipo, monto y descripción. |

---

## Banco_Ruby/Common/Requests.cs

| Código | Qué hace |
|---|---|
| ```csharp
public sealed record DepositoRequest(string NumeroCuenta, decimal Monto);
public sealed record RetiroRequest(string NumeroCuenta, decimal Monto);
public sealed record TransferenciaRequest(string NumeroCuentaOrigen, string NumeroCuentaDestino, decimal Monto);
``` | Request DTOs usados por los endpoints para separar la lógica de negocio de la serialización HTTP. |

---

## Banco_Ruby/Features/Autenticacion/AccountAuthorizationFilter.cs

| Código | Qué hace |
|---|---|
| ```csharp
public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
{
    IReadOnlyCollection<string> accountNumbers = GetAccountNumbers(context.Arguments);
    if (accountNumbers.Count > 0)
    {
        foreach (string numeroCuenta in accountNumbers.Distinct())
        {
            bool exists = await _db.Cuentas.AsNoTracking().AnyAsync(c => c.NumeroCuenta == numeroCuenta && c.Estado);
            if (!exists)
            {
                return Results.NotFound(new { error = $"Cuenta {numeroCuenta} no encontrada o inactiva." });
            }
        }
    }

    return await next(context);
}
``` | Valida que todas las cuentas en los argumentos existan y estén activas antes de ejecutar el endpoint. |
| ```csharp
private static IReadOnlyCollection<string> GetAccountNumbers(IList<object?> arguments)
{
    List<string> accountNumbers = new List<string>();

    foreach (object? arg in arguments)
    {
        switch (arg)
        {
            case string value when !string.IsNullOrWhiteSpace(value):
                accountNumbers.Add(value);
                break;
            case DepositoRequest request:
                accountNumbers.Add(request.NumeroCuenta);
                break;
            case RetiroRequest request:
                accountNumbers.Add(request.NumeroCuenta);
                break;
            case TransferenciaRequest request:
                accountNumbers.Add(request.NumeroCuentaOrigen);
                accountNumbers.Add(request.NumeroCuentaDestino);
                break;
        }
    }

    return accountNumbers;
}
``` | Extrae números de cuenta de los argumentos del endpoint. |

---

## Banco_Ruby/Features/Autenticacion/AutenticacionSlice.cs

| Código | Qué hace |
|---|---|
| ```csharp
public static async Task<object> ConsultarSaldoAsync(string numeroCuenta, BancoRubyDbContext db)
{
    Cuenta? cuenta = await db.Cuentas
        .Include(c => c.Usuario)
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta && c.Estado);

    return cuenta is null
        ? Results.NotFound(new { error = "Cuenta no encontrada o inactiva." })
        : Results.Ok(new { saldo = cuenta.Saldo, titular = cuenta.Usuario?.Nombre ?? string.Empty });
}
``` | Consulta saldo y titular si la cuenta existe; devuelve 404 de lo contrario. |

---

## Banco_Ruby/Features/Operaciones/DepositarSlice.cs

| Código | Qué hace |
|---|---|
| ```csharp
private const decimal COMISION = 0.41m;
``` | Define la comisión fija usada en depósito y retiro. |
| ```csharp
decimal neto = request.Monto - COMISION;
cuenta.Saldo += neto;
db.Auditoria.Add(new Auditoria
{
    CuentaId = cuenta.CuentaId,
    NumeroCuenta = cuenta.NumeroCuenta,
    Tipo = "Depósito",
    Monto = request.Monto,
    Descripcion = $"Se acreditó a la cuenta ${neto:N2} con comisión de ${COMISION:N2} aplicada.",
    CreadoEn = DateTime.UtcNow
});
``` | Resta comisión, actualiza saldo e inserta el registro de auditoría con descripción clara. |
| ```csharp
return Results.Ok(new { mensaje = $"Depósito de ${request.Monto:N2} realizado con comisión de ${COMISION:N2}.", saldo = cuenta.Saldo });
``` | Devuelve el mensaje final y el saldo actualizado. |

---

## Banco_Ruby/Features/Operaciones/RetirarSlice.cs

| Código | Qué hace |
|---|---|
| ```csharp
if (request.Monto % 10 != 0)
{
    return Results.BadRequest(new { error = "El retiro debe ser múltiplo de 10." });
}

if (request.Monto > 500)
{
    return Results.BadRequest(new { error = "El retiro excede el límite de 500." });
}
``` | Valida reglas de negocio: múltiplo de 10 y tope de 500. |
| ```csharp
decimal totalDebitado = request.Monto + COMISION;
if (totalDebitado > cuenta.Saldo)
{
    return Results.BadRequest(new { error = "Fondos insuficientes." });
}

cuenta.Saldo -= totalDebitado;
db.Auditoria.Add(new Auditoria
{
    CuentaId = cuenta.CuentaId,
    NumeroCuenta = cuenta.NumeroCuenta,
    Tipo = "Retiro",
    Monto = totalDebitado,
    Descripcion = $"Se debitó de la cuenta ${request.Monto:N2} más comisión de ${COMISION:N2}.",
    CreadoEn = DateTime.UtcNow
});
``` | Calcula el total con comisión, valida saldo y guarda la auditoría. |

---

## Banco_Ruby/Features/Transferencias/TransferirSlice.cs

| Código | Qué hace |
|---|---|
| ```csharp
if (request.NumeroCuentaOrigen == request.NumeroCuentaDestino)
{
    return Results.BadRequest(new { error = "La cuenta origen y destino no pueden ser la misma." });
}
``` | Evita transferencias a la misma cuenta. |
| ```csharp
origen.Saldo -= request.Monto;
destino.Saldo += request.Monto;
``` | Ajusta saldos de origen y destino. |
| ```csharp
db.Auditoria.Add(new Auditoria
{
    CuentaId = origen.CuentaId,
    NumeroCuenta = origen.NumeroCuenta,
    Tipo = "Transferencia enviada",
    Monto = request.Monto,
    Descripcion = $"Se envió transferencia de ${request.Monto:N2} a la cuenta {destino.NumeroCuenta}.",
    CreadoEn = DateTime.UtcNow
});
``` | Crea la auditoría de salida. |
| ```csharp
return Results.Ok(new
{
    mensaje = $"Transferencia de ${request.Monto:N2} realizada de {origen.NumeroCuenta} a {destino.NumeroCuenta}.",
    saldoOrigen = origen.Saldo,
    saldoDestino = destino.Saldo
});
``` | Devuelve los saldos actualizados y el mensaje de éxito. |

---

## Banco_Ruby/Features/Historial/HistorialSlice.cs

| Código | Qué hace |
|---|---|
| ```csharp
List<HistorialResumen> auditorias = await db.Auditoria
    .AsNoTracking()
    .Where(a => a.CuentaId == cuenta.CuentaId)
    .OrderByDescending(a => a.CreadoEn)
    .Select(a => new HistorialResumen(a.Tipo, a.Monto, a.Descripcion, a.CreadoEn))
    .ToListAsync();
``` | Recupera y ordena los movimientos de auditoría de la cuenta. |
| ```csharp
return Results.Ok(new { titular = cuenta.Usuario?.Nombre ?? string.Empty, historial = auditorias });
``` | Devuelve el titular y la lista de movimientos. |

---

## Usuario_Cliente/Program.cs

| Código | Qué hace |
|---|---|
| ```csharp
string apiBaseUrl = Environment.GetEnvironmentVariable("BANK_API_BASE_URL") ?? "http://localhost:5000";
CajeroApiClient apiClient = new CajeroApiClient(apiBaseUrl);
CajeroConsole console = new CajeroConsole(apiClient);
await console.RunAsync();
``` | Inicializa el cliente y arranca la interfaz de consola. |

---

## Usuario_Cliente/Services/CajeroApiClient.cs

| Código | Qué hace |
|---|---|
| ```csharp
HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numero}/autenticar", new { Pin = pin });
``` | Envía la autenticación al backend. |
| ```csharp
HttpResponseMessage res = await _http.GetAsync($"/api/cuentas/{numero}/saldo");
``` | Consulta el saldo de la cuenta. |
| ```csharp
HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numero}/depositar", new { Monto = monto });
``` | Envía la petición de depósito. |
| ```csharp
HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numero}/retirar", new { Monto = monto });
``` | Envía la petición de retiro. |
| ```csharp
HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numeroOrigen}/transferir", new { CuentaDestino = cuentaDestino, Banco = bancoDestino, Monto = monto, Concepto = concepto });
``` | Envía la transferencia al backend. |
| ```csharp
HttpResponseMessage res = await _http.GetAsync($"/api/cuentas/{numero}/historial");
``` | Recupera el historial de movimientos. |

---

## Usuario_Cliente/Services/CajeroConsole.cs

| Código | Qué hace |
|---|---|
| ```csharp
string option = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Seleccione una opción:")
        .AddChoices("Insertar tarjeta", "Salir"));
``` | Muestra el menú inicial de la consola. |
| ```csharp
_pin = AnsiConsole.Prompt(new TextPrompt<string>("Ingrese su PIN").Secret());
``` | Pide el PIN de forma oculta. |
| ```csharp
string result = await _apiClient.ConsultarSaldoAsync(_cuenta);
``` | Llama al endpoint de saldo y lee la respuesta. |
| ```csharp
AnsiConsole.MarkupLine($"[yellow]Se cobrará una comisión de ${comision:N2}. El monto neto acreditado será ${neto:N2}.[/]");
``` | Informa al usuario de la comisión antes de confirmar el depósito. |
| ```csharp
if (tipoLower.Contains("retiro") || tipoLower.Contains("withdrawal"))
{
    desc = $"Se debitó de la cuenta ${monto:N2}";
}
else if (tipoLower.Contains("deposit") || tipoLower.Contains("depósito") || tipoLower.Contains("dep"))
{
    desc = $"Se acreditó a la cuenta ${monto:N2}";
}
``` | Normaliza descripciones en el historial para mostrar textos claros de débito y crédito. |

---

## Resumen rápido

- El backend usa slices para separar el negocio.
- El filtro `AccountAuthorizationFilter` valida cuentas antes de ejecutar cada endpoint.
- El cliente de consola usa rutas compatibles bajo `/api/cuentas/...`.
- Las descripciones del historial se adaptan en el cliente para mostrar `Se debitó...` y `Se acreditó...`.
- El PIN se pide en el cliente, pero el servidor actual no lo verifica.
