# Arquitectura Vertical Slice de Banco Ruby

## Concepto
Una arquitectura vertical slice agrupa cada caso de uso en un módulo autónomo que contiene:
- entrada
- lógica de negocio
- respuesta

No se basa en las capas clásicas "Controller → Service → Repository".

## Implementación en este proyecto

### `Banco_Ruby`
- `Program.cs`: define las rutas mínimas de la API.
- `Common/`: contiene tipos de dominio compartidos.
- `Features/`: contiene cada slice de funcionalidad.

Cada slice implementa un único caso de uso:
- `AutenticacionSlice.cs`: consulta de saldo.
- `DepositarSlice.cs`: depósito con comisión.
- `RetirarSlice.cs`: retiro con comisión.
- `TransferirSlice.cs`: transferencia entre cuentas.
- `HistorialSlice.cs`: historial de transacciones.

### Estado actual
- Las rutas ya no dependen de una capa de servicio compartida.
- Cada slice hace su propia validación y devoluciones.
- La lógica está separada por caso de uso, no por capa.

### Flujo
1. El cliente llama la ruta HTTP.
2. El endpoint mapea el request a un slice.
3. El slice ejecuta la lógica y devuelve el resultado.

## Ventajas
- Mayor claridad por caso de uso.
- Menos dependencias cruzadas.
- Más fácil agregar nuevas operaciones.

## Estado actual
Este proyecto ya tiene una implementación simple de vertical slice en el servidor, con el cliente separado en `Usuario_Cliente`.

## Nota
Las rutas del servidor están en `Banco_Ruby/Program.cs` y no se mezclan con lógica de presentación o infraestructura pesada.
