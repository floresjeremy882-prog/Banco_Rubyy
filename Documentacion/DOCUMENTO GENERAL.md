# DOCUMENTO GENERAL

## 1. Propósito del proyecto

Este proyecto es una aplicación bancaria simple formada por dos partes:

- `Banco_Ruby`: servidor web API con operaciones bancarias.
- `Usuario_Cliente`: cliente de consola que consume esas APIs.

El objetivo es ofrecer un servicio de banca básica con operaciones de consulta de saldo, depósito, retiro, transferencia y historial de movimientos.

## 2. Estructura general

### 2.1 Carpetas principales

- `Banco_Ruby/Banco_Ruby`: servidor API principal.
- `Banco_Ruby/Usuario_Cliente`: cliente de consola que consume la API.
- `Banco_Ruby/Documentacion`: documentos, diagramas y guías del proyecto.

### 2.2 Partes del servidor

Dentro de `Banco_Ruby/Banco_Ruby`:

- `Common/`: tipos de dominio y request/response compartidos.
- `Features/`: lógica de negocio agrupada por casos de uso.
- `Program.cs`: configuración de la aplicación, registro de servicios y definición de endpoints.
- `BancoRubyDbContext.cs`: Entity Framework Core para acceso a PostgreSQL.

## 3. Arquitectura del servidor

El servidor usa una arquitectura vertical slice ligera:

- Cada operación del negocio (saldo, depósito, retiro, transferencia, historial) está encapsulada en un servicio específico.
- Los endpoints minimal API en `Program.cs` delegan directamente a los servicios.
- No hay una separación tradicional rígida entre controlador, servicio y repositorio: en su lugar se usa un modelo más directo y funcional.

## 4. Flujo de una petición

### 4.1 Paso a paso general

1. El cliente llama a un endpoint HTTP en `Program.cs`.
2. El endpoint ejecuta un filtro de autorización (`AccountAuthorizationFilter`) para validar las cuentas.
3. El endpoint llama a `BankService`.
4. `BankService` valida entrada y reglas de negocio usando `OperationResult<T>`.
5. Si la validación es exitosa, `BankService` aplica el cambio de saldo o consulta de datos.
6. Para transferencias, `BankService` invoca un `ITransferInterceptor` antes de ajustar los saldos.
7. Después de la operación, se publican eventos en `IEventBus` para auditoría y notificaciones.
8. El resultado se devuelve al cliente en formato JSON.

### 4.2 Flujo de transferencia específica

1. El endpoint `/transferencia` acepta `TransferenciaRequest`.
2. `BankService.TransferirAsync` valida que origen y destino sean distintos y que existan ambas cuentas.
3. Se ejecuta `ITransferInterceptor.InterceptTransferAsync(...)`.
4. Se debita la cuenta origen y se abona la cuenta destino.
5. Se guarda el cambio en la base de datos.
6. Se publican eventos de auditoría y `PagoCompletadoEvent`.

## 5. Componentes clave del servidor

### 5.1 `Program.cs`

- Configura `WebApplicationBuilder`.
- Registra servicios:
  - `BancoRubyDbContext` con PostgreSQL.
  - `InMemoryEventBus` como bus de eventos y como `IHostedService`.
  - `IBankService` y `ITransferInterceptor`.
  - `AccountAuthorizationFilter`.
- Define endpoints para:
  - `/health`
  - `/saldo/{numeroCuenta}`
  - `/deposito`
  - `/retiro`
  - `/transferencia`
  - `/historial/{numeroCuenta}`
  - Rutas de compatibilidad para `Usuario_Cliente`.

### 5.2 `BancoRubyDbContext.cs`

- Define las entidades `Usuario`, `Cuenta` y `Auditoria`.
- Configura mapeo de tablas y columnas a PostgreSQL.
- Define relaciones:
  - un `Usuario` tiene muchas `Cuenta`.
  - una `Cuenta` tiene muchas `Auditoria`.

### 5.3 `Common/`.

#### `Cuenta.cs`

Define entidades de dominio:

- `Usuario`
- `Cuenta`
- `Auditoria`

#### `Requests.cs`

Define request DTOs para operaciones:

- `DepositoRequest`
- `RetiroRequest`
- `TransferenciaRequest`

#### `DomainEvents.cs`

Define el bus de eventos y los tipos de eventos:

- `IDomainEvent`
- `AuditoriaEvent`
- `PagoCompletadoEvent`
- `IEventBus`
- `InMemoryEventBus`

### 5.4 `Features/BankService.cs`

Es el núcleo de la lógica bancaria:

- `ConsultarSaldoAsync`
- `DepositarAsync`
- `RetirarAsync`
- `TransferirAsync`
- `ObtenerHistorialAsync`

Usa `OperationResult<T>` para manejar resultados y errores de forma explícita.

### 5.5 `Features/AccountAuthorizationFilter.cs`

- Filtro transversal que valida si las cuentas existen y están activas antes de ejecutar la operación.
- Se aplica en los endpoints relevantes.

### 5.6 `Features/Result.cs`

- `OperationResult<T>` encapsula éxito o fracaso.
- Incluye `StatusCode`, `Error` y métodos de creación:
  - `Ok(...)`
  - `BadRequest(...)`
  - `NotFound(...)`
  - `Fail(...)`

## 6. Interceptor de transferencias

### 6.1 `ITransferInterceptor`

Es la interfaz para extender el comportamiento de transferencia.

### 6.2 `DefaultTransferInterceptor`

Es el interceptor por defecto y no cambia nada. Sirve como punto de extensión cuando se conecta otro proyecto o servicio externo.

### 6.3 Registro y uso

- Se registra en `Program.cs`.
- `BankService` lo invoca antes de modificar los saldos de transferencia.

## 7. Bus de eventos y desacoplamiento

### 7.1 `InMemoryEventBus`

- Usa `Channel<IDomainEvent>` con buffer acotado.
- Procesa eventos de auditoría en segundo plano.
- Desacopla la persistencia de auditoría de la ejecución principal.

### 7.2 Eventos publicados

- `AuditoriaEvent` al crear un registro de auditoría.
- `PagoCompletadoEvent` tras una transferencia exitosa.

## 8. Cliente de usuario

### 8.1 `Usuario_Cliente/Program.cs`

- Crea `CajeroApiClient` apuntando a `BANK_API_BASE_URL` o `http://localhost:5000`.
- Inicia `CajeroConsole`.

### 8.2 `Usuario_Cliente/Services/CajeroApiClient.cs`

- Consume las APIs del servidor usando `HttpClient`.
- Métodos:
  - `AutenticarAsync`
  - `ConsultarSaldoAsync`
  - `DepositarAsync`
  - `RetirarAsync`
  - `TransferirAsync`
  - `ObtenerHistorialAsync`

### 8.3 `Usuario_Cliente/Services/CajeroConsole.cs`

- Interfaz de consola para el usuario.
- Pide datos y llama a `CajeroApiClient`.
- Muestra resultados y errores.

## 9. Cómo ejecutar el proyecto

### 9.1 Ejecutar el servidor

```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby\Banco_Ruby"
dotnet run
```

### 9.2 Ejecutar el cliente

```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby\Usuario_Cliente"
dotnet run
```

## 10. Qué debes explicar a tus superiores

### 10.1 Qué hace el proyecto

- Servicio bancario con API REST mínima.
- Cliente de consola que consume la API.
- Arquitectura simple y extensible.

### 10.2 Por qué es sólido

- Separación clara entre lógica de negocio y endpoints.
- Manejo explícito de errores con `OperationResult<T>`.
- Interceptor de transferencia listo para extender a otro proyecto.
- Bus de eventos desacoplado para auditoría.

### 10.3 Qué se puede ampliar fácilmente

- Conectar `ITransferInterceptor` a otro servicio externo.
- Añadir más eventos y handlers.
- Extender la API con nuevas operaciones.
- Mejorar validaciones de datos en la capa de dominio.

## 11. Recomendaciones finales

- Usa `Documentacion/interceptor_transferencia.md` para entender la conexión del interceptor.
- Revisa `Documentacion/paradigmas_proyecto.md` para entender los paradigmas aplicados.
- Con este documento puedes explicar el flujo desde el endpoint hasta la base de datos y el bus de eventos.
