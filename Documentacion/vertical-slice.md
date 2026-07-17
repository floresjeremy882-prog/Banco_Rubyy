# Arquitectura Vertical Slice de Banco Ruby

## Concepto
Una arquitectura vertical slice agrupa cada caso de uso en un módulo autónomo que contiene:
- entrada
- lógica de negocio
- respuesta

No se basa en las capas clásicas “Controller → Service → Repository”.

## Implementación actual en este proyecto

### Banco_Ruby
- Program.cs: define las rutas mínimas de la API.
- Common/: contiene tipos compartidos y DTOs.
- Features/: contiene los slices de cada caso de uso.
- Domain/: contiene la regla crítica de transferencias.
- Infrastructure/: encapsula la integración con el banco externo.

Cada slice implementa un caso de uso concreto:
- AutenticacionSlice.cs: consulta de saldo.
- DepositarSlice.cs: depósito con comisión.
- RetirarSlice.cs: retiro con comisión y límites.
- TransferirSlice.cs: transferencia entre cuentas.
- HistorialSlice.cs: historial de movimientos.

## Estado actual
- Las rutas siguen delegando en slices de aplicación.
- La lógica crítica de la transferencia se mueve a una capa de dominio ligera.
- La integración externa se encapsula en infrastructure para que sea más fácil reemplazarla por un servicio real.

## Flujo actual
1. El cliente llama a una ruta HTTP.
2. El endpoint transforma el request y delega al slice correspondiente.
3. El slice ejecuta la operación y, en el caso de transferencias, usa el servicio de dominio.
4. Si falla la integración externa, se revierte el cambio y se devuelve un mensaje de error.

## Ventajas
- Mayor claridad por caso de uso.
- Menos dependencias cruzadas.
- Fácil de extender con nuevas reglas o integraciones externas.

## Nota
El proyecto sigue usando vertical slice, pero ya incorpora una separación más limpia entre aplicación, dominio e infraestructura.
