# Paradigmas en el proyecto BancoRuby

Esta documentación describe dónde y cómo se implementan los paradigmas del proyecto actual.

## 1. Arquitectura vertical slice

- Estado: **Implementado directamente**.
- Dónde: `Program.cs` y `Features/`.
- Cómo funciona:
  - Cada operación tiene un slice propio (`DepositarSlice`, `RetirarSlice`, `TransferirSlice`, `HistorialSlice`, `AutenticacionSlice`).
  - `Program.cs` define rutas mínimas y delega la lógica al slice correspondiente.
  - Cada slice recibe el `BancoRubyDbContext`, valida la petición y devuelve un `Results.*` adecuado.
- Beneficio: claridad por caso de uso y menor acoplamiento entre operaciones.

## 2. Autorización [AOP]

- Estado: **Implementado con filtro de endpoint**.
- Dónde: `Features/AccountAuthorizationFilter.cs` y `Program.cs`.
- Cómo funciona:
  - `AccountAuthorizationFilter` valida que las cuentas existan y estén activas antes de ejecutar la operación.
  - Si la cuenta no es válida, devuelve un error sin ejecutar el slice.
- Beneficio: evita repetir validación de cuenta en cada slice.

## 3. Reglas de negocio [POO simplificado]

- Estado: **Implementado en slices y entidades de dominio**.
- Dónde: `Features/Operaciones/DepositarSlice.cs`, `Features/Operaciones/RetirarSlice.cs`, `Features/Transferencias/TransferirSlice.cs`, `Common/Cuenta.cs`, `Common/Requests.cs`.
- Cómo funciona:
  - Las entidades `Cuenta`, `Usuario`, `Auditoria` modelan el dominio.
  - El `DbContext` mantiene el acceso a datos.
  - Los slices encapsulan reglas de negocio específicas.
- Beneficio: mantiene la lógica bancaria clara sin introducir capas de servicio adicionales.

## 4. Comisiones y validaciones

- Estado: **Implementado en los slices de depósito y retiro**.
- Dónde: `DepositarSlice.cs`, `RetirarSlice.cs`.
- Cómo funciona:
  - Ambos slices aplican una comisión fija de `$0.41`.
  - El depósito acredita el neto y registra auditoría con descripción detallada.
  - El retiro descuenta el total (`monto + comisión`) y registra auditoría con descripción clara.
- Beneficio: la lógica de comisión es explicita y está cerca de la operación.

## 5. Historial y auditoría

- Estado: **Implementado con registros directos en la tabla de auditoría**.
- Dónde: `Features/Historial/HistorialSlice.cs`.
- Cómo funciona:
  - Cada operación crea un registro de auditoría en `db.Auditoria`.
  - Historial proyecta esos registros para el cliente.
- Beneficio: el historial muestra transacciones con tipo, monto, descripción y fecha.

## Resumen

- Implementado en el proyecto:
  - `Vertical Slice`: con slices directos por operación.
  - `AOP`: con filtro de endpoint para autorización de cuenta.
  - `POO simplificado`: con entidades y `DbContext`.
  - `Validaciones explícitas`: dentro de cada slice.
  - `Auditoría directa`: insertando registros en la tabla de auditoría.

## Archivos clave actuales

- `Banco_Ruby/Program.cs`
- `Banco_Ruby/BancoRubyDbContext.cs`
- `Banco_Ruby/Common/Cuenta.cs`
- `Banco_Ruby/Common/Requests.cs`
- `Banco_Ruby/Features/AccountAuthorizationFilter.cs`
- `Banco_Ruby/Features/Operaciones/DepositarSlice.cs`
- `Banco_Ruby/Features/Operaciones/RetirarSlice.cs`
- `Banco_Ruby/Features/Transferencias/TransferirSlice.cs`
- `Banco_Ruby/Features/Historial/HistorialSlice.cs`

## Flujo simplificado (pseudocódigo)

1. El cliente llama a un endpoint en `Program.cs`.
2. El `AccountAuthorizationFilter` valida la cuenta.
3. El endpoint invoca el slice adecuado.
4. El slice valida la operación y actualiza la base de datos.
5. Se devuelve un `Results.Ok(...)` o `Results.BadRequest(...)` según corresponda.

```text
CLIENT -> Endpoint
   -> AccountAuthorizationFilter
   -> Slice (Depositar, Retirar, Transferir, Historial)
   -> DbContext actualiza datos y registra auditoría
   -> Respuesta HTTP
```

---

> El proyecto ya refleja los paradigmas solicitados de forma limpia y mantiene la lógica bancaria original intacta, con mejor separación de responsabilidades y control de errores.