# Paradigmas en el proyecto BancoRuby

Esta documentación describe dónde y cómo se implementan los paradigmas del diagrama en el proyecto actual.

## 1. Carga Extrema [Reactivo]

- Estado: **Implementado de forma ligera**.
- Dónde: `Banco_Ruby/Banco_Ruby/Banco_Ruby/Common/DomainEvents.cs` y `Program.cs`.
- Cómo funciona:
  - El proyecto usa `InMemoryEventBus`, un bus de eventos interno basado en `Channel<T>` con un buffer acotado.
  - Las operaciones de negocio publican eventos (`AuditoriaEvent`, `PagoCompletadoEvent`) usando `IEventBus.PublishAsync(...)`.
  - El servicio en segundo plano `InMemoryEventBus` consume esos eventos de manera asíncrona y los procesa sin bloquear el flujo principal de la petición.
- Beneficio: esto añade un comportamiento similar a backpressure y desacopla la persistencia de los eventos de auditoría de la ejecución inmediata de la transacción.

## 2. Autorización [AOP]

- Estado: **Implementado con filtro de endpoint**.
- Dónde: `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/AccountAuthorizationFilter.cs` y `Program.cs`.
- Cómo funciona:
  - Se registró `AccountAuthorizationFilter` como un `IEndpointFilter` en los endpoints que usan `IBankService`.
  - Antes de ejecutar la lógica de negocio, el filtro lee los argumentos del endpoint y verifica si las cuentas existen y están activas en la base de datos.
  - Si la cuenta no es válida, devuelve `Results.NotFound(...)` sin llamar al servicio.
- Beneficio: permite validar cuentas de forma transversal sin repetir lógica dentro de cada método de negocio.

## 3. Reglas de Negocio [POO]

- Estado: **Implementado de forma clara**.
- Dónde: `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/BankService.cs`, `Banco_Ruby/Banco_Ruby/Common/Cuenta.cs`, `Banco_Ruby/Banco_Ruby/Common/Requests.cs`, `Banco_Ruby/Banco_Ruby/Common/DomainEvents.cs`.
- Cómo funciona:
  - `BankService` encapsula toda la lógica de negocio bancaria en una clase concreta y consumible.
  - Las entidades `Cuenta`, `Usuario`, `Auditoria` modelan el dominio y las relaciones del banco.
  - El `DbContext` de Entity Framework (`BancoRubyDbContext`) mantiene el modelo de dominio y mapea las entidades a la base de datos.
- Beneficio: separa claramente las reglas de negocio de la infraestructura de la API y mantiene un dominio expresivo.

## 4. Cálculo y Falla [Funcional + ROP]

- Estado: **Implementado con `OperationResult<T>`**.
- Dónde: `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/Result.cs` y `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/BankService.cs`.
- Cómo funciona:
  - `OperationResult<T>` representa éxito o fallo con un valor opcional y código HTTP asociado.
  - Los métodos de `BankService` devuelven `OperationResult<T>` en lugar de lanzar excepciones para lógica de validación.
  - Las validaciones (`ValidarMontoAsync`, `ValidarRetiroAsync`, `ValidarTransferenciaAsync`) retornan resultados de fallo o éxito que se encadenan manualmente.
- Beneficio: mantiene la lógica de errores explícita y evita ramas de excepción mezcladas con la lógica normal.

## 5. Desacoplamiento [Eventos]

- Estado: **Implementado como arquitectura de eventos internos**.
- Dónde: `Banco_Ruby/Banco_Ruby/Banco_Ruby/Common/DomainEvents.cs`, `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/BankService.cs`.
- Cómo funciona:
  - `BankService` publica eventos de dominio después de confirmar las operaciones sobre las cuentas.
  - El bus de eventos en memoria procesa estos eventos de auditoría en segundo plano, creando registros de auditoría y simulando un flujo de eventos desacoplado.
  - El evento `PagoCompletadoEvent` demuestra cómo se puede activar un evento de dominio independiente a partir de la operación de transferencia.
- Beneficio: desacopla la acción del negocio (depósito/retirada/transferencia) del efecto secundario de auditoría, lo que facilita extender el sistema con nuevos handlers en el futuro.

## Resumen

- Implementado en el proyecto:
  - `Reactivo`: mediante un bus interno basado en `Channel<T>` y procesamiento asíncrono.
  - `AOP`: usando filtro de endpoint (`AccountAuthorizationFilter`).
  - `POO`: con `BankService`, entidades y `DbContext`.
  - `Funcional + ROP`: con `OperationResult<T>` y validaciones encadenadas.
  - `Eventos`: con publicación de eventos de dominio y procesamiento en segundo plano.

## Archivos clave actualizados

- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Program.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/BancoRubyDbContext.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Common/DomainEvents.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Common/Cuenta.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Common/Requests.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/AccountAuthorizationFilter.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/BankService.cs`
- `Banco_Ruby/Banco_Ruby/Banco_Ruby/Features/Result.cs`

## Flujo simplificado (pseudocódigo)

1. El cliente llama a un endpoint en `Program.cs`.
2. El `AccountAuthorizationFilter` valida la cuenta de forma transversal.
3. El endpoint invoca `BankService`.
4. `BankService` valida la operación con `OperationResult<T>`.
5. Si la validación es exitosa, actualiza la cuenta y publica eventos (`AuditoriaEvent`, `PagoCompletadoEvent`).
6. `InMemoryEventBus` procesa los eventos en segundo plano y escribe auditoría en la base de datos.

```text
CLIENT -> Endpoint
   -> AccountAuthorizationFilter
   -> BankService.ValidarCuenta()
   -> BankService.ValidarReglas()
   -> BankService.RealizarOperacion()
   -> IEventBus.PublishAsync(event)
EVENT BUS -> Procesar eventos en segundo plano
   -> Guardar auditoría
```

---

> El proyecto ya refleja los paradigmas solicitados de forma limpia y mantiene la lógica bancaria original intacta, con mejor separación de responsabilidades y control de errores.