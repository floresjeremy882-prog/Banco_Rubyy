# Documento para explicar

Este documento explica de forma simple qué hace cada parte del proyecto y cómo se relacionan.

## 1. Idea general

El servidor usa una estructura simple por funciones:

- Cada caso de uso está separado.
- Las operaciones bancarias no se agrupan en capas muy complejas.
- Cada parte del sistema tiene una tarea clara.

## 2. Archivos importantes

### Program.cs

- Es el punto de entrada de la app.
- Registra servicios y define las rutas.
- Aquí se configuran saldo, depósito, retiro, transferencia e historial.

### BancoRubyDbContext.cs

- Define las tablas y relaciones de usuarios, cuentas y auditoría.
- Conecta la app con PostgreSQL.

### Common/Cuenta.cs

- Guarda los modelos del sistema.
- Aquí están las clases de usuario, cuenta y auditoría.

### Common/Requests.cs

- Define las peticiones que llegan desde el cliente.
- Ejemplos: depósito, retiro y transferencia.

### Common/DomainEvents.cs

- Define los eventos del sistema.
- Sirve para registrar acciones como auditoría o notificaciones.

### Features/Operaciones/BankService.cs

- Es el corazón de la lógica bancaria.
- Aquí se valida saldo, se hacen depósitos, retiros y transferencias.
- También se consulta el historial.

### Features/Operaciones/DepositarSlice.cs

- Maneja la parte de depósito.
- Recibe la petición, la envía al servicio y devuelve la respuesta.

### Features/Operaciones/RetirarSlice.cs

- Hace lo mismo para el retiro.

### Features/Historial/HistorialSlice.cs

- Devuelve el historial de una cuenta.

### Features/Autenticacion/AccountAuthorizationFilter.cs

- Valida que la cuenta exista y esté activa antes de seguir.
- Es una protección general para varias rutas.

### Features/Transferencias/ITransferInterceptor.cs y DefaultTransferInterceptor.cs

- Permiten extender o controlar el comportamiento de las transferencias.

### Features/Transferencias/TransferirSlice.cs

- Maneja la ruta de transferencia.
- Llama al servicio y devuelve la respuesta.

### Usuario_Cliente/Program.cs

- Inicia el cliente de consola.
- Conecta con la API del servidor.

### Usuario_Cliente/Services/CajeroApiClient.cs

- Envía las peticiones al servidor.
- Usa HttpClient para comunicarse con la API.

### Usuario_Cliente/Services/CajeroConsole.cs

- Muestra un menú simple al usuario.
- Recoge datos y llama al cliente HTTP.

## 3. Cómo se conectan las partes

- Program.cs define las rutas.
- Las rutas llaman al servicio principal.
- El servicio usa el contexto de base de datos.
- También puede publicar eventos.
- El cliente consume la API y muestra resultados.

## 4. Conceptos importantes

### Estructura por funciones

- El proyecto está pensado para ser fácil de entender.
- Cada función tiene su parte propia.

### Validaciones

- Antes de hacer una operación, se revisa que todo sea correcto.
- Esto ayuda a evitar errores.

### Eventos

- Algunos cambios generan eventos para auditoría y seguimiento.

## 5. Recomendación para presentar

- Empieza explicando la idea general del proyecto.
- Después menciona cómo funciona una operación básica.
- Luego explica el cliente de consola como la parte que usa la API.
