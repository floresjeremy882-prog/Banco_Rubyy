# DOCUMENTO PARA EXPLICAR

Este documento explica archivo por archivo qué hace el código del proyecto y cómo se relacionan los componentes.

## 1. Arquitectura general

El servidor usa una arquitectura **vertical slice** ligera:

- Cada caso de uso es una pieza independiente del sistema.
- Las operaciones bancarias no se agrupan en capas tradicionales (controlador/servicio/repo).
- Se sigue un patrón donde cada slice contiene su propia lógica de negocio.

## 2. Archivo por archivo

### 2.1 `Banco_Ruby/Banco_Ruby/Program.cs`

- Punto de entrada de la aplicación.
- Configura `WebApplicationBuilder` y registra servicios en DI.
- Registra `BancoRubyDbContext` para PostgreSQL.
- Registra `InMemoryEventBus` como `IEventBus` y `IHostedService`.
- Registra `IBankService` y `ITransferInterceptor`.
- Registra `AccountAuthorizationFilter`.
- Define rutas minimal API para:
  - `/health`
  - `/saldo/{numeroCuenta}`
  - `/deposito`
  - `/retiro`
  - `/transferencia`
  - `/historial/{numeroCuenta}`
  - rutas de compatibilidad para el cliente `Usuario_Cliente`.
- Cada endpoint delega al servicio `BankService` y usa `FromResult(...)` para devolver respuestas HTTP claras.

### 2.2 `Banco_Ruby/Banco_Ruby/BancoRubyDbContext.cs`

- Define la clase `BancoRubyDbContext` que hereda de `DbContext`.
- Expone los `DbSet` para `Usuarios`, `Cuentas` y `Auditoria`.
- Configura el mapeo de tablas y columnas de PostgreSQL en `OnModelCreating`.
- Define las relaciones:
  - `Usuario` → `Cuentas`
  - `Cuenta` → `Auditorias`

### 2.3 `Banco_Ruby/Banco_Ruby/Common/Cuenta.cs`

- Define entidades de dominio del sistema:
  - `Usuario`
  - `Cuenta`
  - `Auditoria`
- Cada entidad contiene propiedades primarias, relaciones de navegación y campos de auditoría.
- Es el modelo de datos compartido que usa Entity Framework y la lógica de negocio.

### 2.4 `Banco_Ruby/Banco_Ruby/Common/Requests.cs`

- Define los DTO de entrada para las operaciones API:
  - `DepositoRequest`
  - `RetiroRequest`
  - `TransferenciaRequest`
- Son tipos inmutables y simples, usados por los endpoints y el servicio.

### 2.5 `Banco_Ruby/Banco_Ruby/Common/DomainEvents.cs`

- Define el modelo de eventos de dominio y el bus de eventos:
  - `IDomainEvent`
  - `AuditoriaEvent`
  - `PagoCompletadoEvent`
  - `IEventBus`
  - `InMemoryEventBus`
- `InMemoryEventBus` usa `Channel<IDomainEvent>` para procesar eventos en segundo plano.
- Esto desacopla la publicación de eventos de la ejecución principal de las operaciones.

### 2.6 `Banco_Ruby/Banco_Ruby/Features/Result.cs`

- Define `OperationResult<T>`.
- Encapsula el resultado de una operación con:
  - `IsSuccess`
  - `IsFailure`
  - `Value`
  - `Error`
  - `StatusCode`
- Proporciona métodos de fábrica:
  - `Ok(...)`
  - `BadRequest(...)`
  - `NotFound(...)`
  - `Fail(...)`
- Permite manejar errores de negocio sin lanzar excepciones.

### 2.7 `Banco_Ruby/Banco_Ruby/Features/Responses.cs`

- Define DTOs de salida utilizados por el servicio:
  - `SaldoResponse`
  - `OperacionResponse`
  - `TransferenciaResponse`
  - `HistorialItem`
  - `HistorialResponse`
- Permite devolver datos estructurados a los clientes HTTP.

### 2.8 `Banco_Ruby/Banco_Ruby/Features/AccountAuthorizationFilter.cs`

- Implementa un filtro de endpoint `IEndpointFilter`.
- Extrae números de cuenta de los argumentos del endpoint.
- Verifica en la base de datos que las cuentas existan y estén activas.
- Si la validación falla, devuelve `Results.NotFound(...)` sin ejecutar el servicio.
- Esto es la implementación AOP del proyecto: una validación transversal aplicada antes de la lógica de negocio.

### 2.9 `Banco_Ruby/Banco_Ruby/Features/ITransferInterceptor.cs`

- Define la interfaz `ITransferInterceptor`.
- Este archivo declara el contrato para interceptar transferencias.
- Permite conectar lógica externa antes de ejecutar la transferencia real.

### 2.10 `Banco_Ruby/Banco_Ruby/Features/DefaultTransferInterceptor.cs`

- Implementa `ITransferInterceptor` con un comportamiento neutro.
- Es un placeholder que permite al sistema iniciar sin un interceptor externo.
- Si hay un interceptor concreto, se reemplaza en DI.

### 2.11 `Banco_Ruby/Banco_Ruby/Features/BankService.cs`

- Núcleo de la lógica bancaria:
  - `ConsultarSaldoAsync`
  - `DepositarAsync`
  - `RetirarAsync`
  - `TransferirAsync`
  - `ObtenerHistorialAsync`
- Usa `OperationResult<T>` para controlar errores de negocio.
- Valida saldos y reglas antes de aplicar cambios.
- Invoca `ITransferInterceptor` antes de ejecutar transferencias.
- Publica eventos de dominio en `IEventBus` después de operaciones exitosas.
- Maneja la consulta del historial y la conversión de datos a DTOs.

### 2.12 `Banco_Ruby/Usuario_Cliente/Program.cs`

- Inicializa el cliente de consola.
- Lee la variable `BANK_API_BASE_URL` o usa `http://localhost:5000`.
- Crea un `CajeroApiClient` y un `CajeroConsole`.
- Ejecuta la consola interactiva.

### 2.13 `Banco_Ruby/Usuario_Cliente/Services/CajeroApiClient.cs`

- Cliente HTTP que consume los endpoints del servidor.
- Métodos:
  - `AutenticarAsync`
  - `ConsultarSaldoAsync`
  - `DepositarAsync`
  - `RetirarAsync`
  - `TransferirAsync`
  - `ObtenerHistorialAsync`
- Usa `HttpClient` y `PostAsJsonAsync` para enviar datos.
- Devuelve respuestas en texto para que el cliente las muestre.

### 2.14 `Banco_Ruby/Usuario_Cliente/Services/CajeroConsole.cs`

- Interfaz de usuario de consola.
- Solicita datos al usuario y llama a `CajeroApiClient`.
- Muestra resultados, errores y menús.
- Contiene la navegación entre opciones de saldo, depósito, retiro, transferencia e historial.

## 3. Conexiones importantes entre archivos

- `Program.cs` → registra servicios y define endpoints.
- Endpoints -> `BankService` para toda la lógica bancaria.
- `BankService` → usa `BancoRubyDbContext` para acceso a datos.
- `BankService` → usa `ITransferInterceptor` para transferencias.
- `BankService` → publica eventos a `IEventBus`.
- `AccountAuthorizationFilter` → protege endpoints aplicando validación transversal.
- `Usuario_Cliente` → consume la API del servidor.

## 4. Conceptos clave para explicar

### 4.1 Vertical slice

- El proyecto está organizado por funcionalidades, no por capas.
- Cada operación es un slice independiente.
- Esto hace el código más fácil de entender y extender.

### 4.2 AOP

- El filtro `AccountAuthorizationFilter` valida cuentas antes de ejecutar la lógica.
- Se aplica a múltiples endpoints sin copiar la misma validación.

### 4.3 Resultados funcionales

- `OperationResult<T>` permite separar éxito y error.
- Evita mezclar lógica de control con manejo de excepciones.

### 4.4 Event bus

- `InMemoryEventBus` procesa eventos en segundo plano.
- Desacopla la lógica principal de la auditoría y notificaciones.

## 5. Dónde se usa cada concepto

- Vertical slice: `Program.cs`, `Features/BankService.cs`, `Features/AccountAuthorizationFilter.cs`.
- AOP: `Features/AccountAuthorizationFilter.cs`.
- Funcional/ROP: `Features/Result.cs`, `Features/BankService.cs`.
- Eventos: `Common/DomainEvents.cs`, `Features/BankService.cs`.

## 6. Recomendación para presentar

- Explica primero la estructura general del proyecto.
- Menciona que el servidor es vertical slice.
- Luego detalla cómo funciona una transferencia y cómo el interceptor entra en el flujo.
- Finaliza con el cliente de consola como consumidor de la API.
