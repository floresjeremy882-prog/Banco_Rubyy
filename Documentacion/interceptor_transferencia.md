# Integración de transferencias externas

## Estado actual

El proyecto ya no depende de un interceptor clásico como el que se describía antes. La transferencia externa ahora se maneja a través de un gateway en la capa de infraestructura y una regla de dominio que controla la reversión del movimiento si algo falla.

## Qué hace el flujo actual

1. La transferencia se procesa en el slice de aplicación.
2. El servicio de dominio aplica la lógica principal.
3. El gateway externo intenta ejecutar la operación con el banco destino.
4. Si ocurre un timeout o cualquier excepción, se revierte el cambio y se devuelve un mensaje de error.

## Archivos clave

- Banco_Ruby/Features/Transferencias/TransferirSlice.cs: orquesta la operación.
- Banco_Ruby/Domain/Transferencias/TransferenciaService.cs: implementa la regla de negocio y la reversión.
- Banco_Ruby/Infrastructure/TransferenciaGateway.cs: representa la integración externa.
- Banco_Ruby/Program.cs: registra el gateway para que el slice pueda usarlo.

## Comportamiento esperado

Si la transferencia falla:

- la cuenta origen recupera el monto original,
- la cuenta destino vuelve a su saldo anterior,
- la respuesta devuelve un mensaje tipo “Transacción fallida”.

## Recomendación futura

Cuando se conecte a un banco real, este gateway puede reemplazarse por una llamada HTTP o por un cliente de mensajería, sin cambiar la lógica de dominio.
