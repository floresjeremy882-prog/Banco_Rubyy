# Documento para explicar

Este documento resume la estructura actual del proyecto y explica los puntos clave para entender el flujo de negocio y la arquitectura.

## Arquitectura general

El proyecto está organizado con un enfoque práctico:

- Features/: contiene los slices de aplicación por caso de uso.
- Domain/: concentra la regla de negocio crítica de las transferencias.
- Infrastructure/: encapsula la integración con el banco destino.
- Common/: modelos y DTOs compartidos.

## Banco_Ruby/Program.cs

- Registra el contexto de EF Core con PostgreSQL.
- Registra el filtro de autorización de cuentas.
- Registra el gateway de transferencias.
- Expone los endpoints de saldo, depósito, retiro, transferencia e historial.

## Banco_Ruby/BancoRubyDbContext.cs

- Define las entidades Usuario, Cuenta y Auditoria.
- Mapea esas entidades a tablas PostgreSQL.

## Banco_Ruby/Common

- Cuenta.cs: define el modelo de cuenta, usuario y auditoría.
- Requests.cs: define los request DTOs para depósito, retiro y transferencia.

## Banco_Ruby/Features

- Autenticacion: valida si una cuenta existe y está activa.
- Operaciones: implementa depósitos y retiros con sus reglas.
- Transferencias: orquesta la transferencia y usa el servicio de dominio.
- Historial: devuelve los movimientos registrados en auditoría.

## Banco_Ruby/Domain/Transferencias

- TransferenciaService.cs: aplica la lógica central de la transferencia y la reversión si falla la integración externa.

## Banco_Ruby/Infrastructure

- TransferenciaGateway.cs: representa la integración externa con el banco destino.
- En este momento está simulada, pero puede reemplazarse por una llamada HTTP real.

## Flujo de transferencia actual

1. Se valida la cuenta origen, la cuenta destino y el monto.
2. Se descuenta el monto de la cuenta origen y se acredita al destino en memoria.
3. Se invoca el gateway externo.
4. Si falla la integración, los saldos vuelven a su estado anterior y la respuesta indica que la transacción falló.
5. Se registra la auditoría del resultado.

## Usuario_Cliente

- Consulta el saldo, hace depósitos y retiros, y envía transferencias al backend.
- Usa rutas compatibles bajo /api/cuentas para interactuar con el servidor.

## Resumen rápido

- La aplicación está separada por caso de uso.
- La regla más importante de la transferencia está encapsulada en el dominio.
- El fallback de reversión protege el saldo ante fallos externos.
