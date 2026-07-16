# Conectar un interceptor de transferencia

> Nota: la implementación actual del backend no utiliza un interceptor de transferencia ni `BankService`.
> Este documento explica un enfoque legado que ya no está presente en el código actual.

El código actual usa slices verticales directos y no incluye `ITransferInterceptor` ni la clase `BankService`.

## 1. Archivos clave

- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/ITransferInterceptor.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/DefaultTransferInterceptor.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/BankService.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Program.cs`

## 2. ¿Qué hace el interceptor?

El interceptor se ejecuta antes de que el servicio bancario ajuste los saldos en una transferencia. Su propósito es:

- validar reglas adicionales
- notificar otro servicio o proyecto
- registrar información externa
- cancelar la transferencia antes de aplicar el cambio de saldo

## 3. Cómo conectar tu interceptor

### 3.1 Implementa `ITransferInterceptor`

Crea una clase que implemente la interfaz:

```csharp
using BancoCenit.Common;
using BancoCenit.Features;

public sealed class MiTransferInterceptor : ITransferInterceptor
{
    public async Task InterceptTransferAsync(TransferenciaRequest request, Cuenta origen, Cuenta destino)
    {
        // Aquí va la lógica de conexión al otro proyecto.
        // Por ejemplo: llamar a un API externo, validar autorización, enviar notificación, etc.

        // Si necesitas cancelar la transferencia, lanza una excepción específica
        // o maneja el error en el interceptor y registra el resultado.
    }
}
```

### 3.2 Registrar el interceptor en DI

Abre `Banco_Ruby/Banco_Ruby/Banco_Ruby/Program.cs` y agrega el registro de tu clase:

```csharp
builder.Services.AddScoped<ITransferInterceptor, MiTransferInterceptor>();
```

Asegúrate de que esta línea esté antes de `builder.Build()`.

### 3.3 Validar que el interceptor está en el flujo

En `BankService` la transferencia ya llama al interceptor antes de modificar los saldos:

```csharp
await _transferInterceptor.InterceptTransferAsync(request, pair.origen, pair.destino);
```

Si esta llamada ocurre, el interceptor se ejecutará cada vez que se procese una transferencia.

## 4. Qué pasa si el interceptor falla

- Si lanza una excepción, la transferencia fallará y no se guardarán los cambios de saldo.
- Si el interceptor solo registra información, la transferencia continuará normalmente.

Para tener un control más claro, puedes usar un `try/catch` dentro del interceptor o dentro de `BankService`.

## 5. Ejemplo de uso con un proyecto externo

Supongamos que el interceptor debe notificar a un servicio externo antes de realizar la transferencia:

```csharp
public sealed class MiTransferInterceptor : ITransferInterceptor
{
    private readonly HttpClient _httpClient;

    public MiTransferInterceptor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InterceptTransferAsync(TransferenciaRequest request, Cuenta origen, Cuenta destino)
    {
        var payload = new
        {
            origen = request.NumeroCuentaOrigen,
            destino = request.NumeroCuentaDestino,
            monto = request.Monto,
            concepto = request.Concepto
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/external-transfer", payload);
        response.EnsureSuccessStatusCode();
    }
}
```

Y registra el cliente HTTP:

```csharp
builder.Services.AddHttpClient<MiTransferInterceptor>(client =>
{
    client.BaseAddress = new Uri("https://otro-proyecto-api");
});
```

## 6. Recomendaciones

- Mantén el interceptor ligero: no debe contener la lógica principal de la transferencia.
- Usa el interceptor para tareas transversales: auditoría externa, validación de reglas externas, llamadas a otros servicios.
- Si necesitas decisiones complejas, tu interceptor puede devolver un resultado o lanzar una excepción controlada.
- Siempre prueba la transferencia con el interceptor activado y desactivado.

## 7. Confirmación de estado actual

El proyecto ya está preparado para conectar el interceptor. Solo falta implementar y registrar la clase concreta en `Program.cs`.
