# Documento general

## 1. Propósito del proyecto

Este proyecto es una app bancaria simple con dos partes:

- Banco_Ruby: sirve la API del banco.
- Usuario_Cliente: es un cliente de consola que usa esa API.

La idea es ofrecer funciones básicas como ver saldo, depositar, retirar, transferir y revisar el historial.

## 2. Estructura general

### 2.1 Carpetas principales

- Banco_Ruby: servidor principal.
- Usuario_Cliente: cliente para probar la API.
- Documentacion: guías, diagramas y explicaciones.

### 2.2 Partes del servidor

Dentro de Banco_Ruby:

- Common/: modelos, eventos y peticiones compartidas.
- Features/: lógica de negocio por área.
  - Autenticacion: validación de cuentas.
  - Operaciones: saldo, depósito, retiro y resultados.
  - Transferencias: transferencias e interceptor.
  - Historial: consultas del historial.
- Program.cs: define las rutas y registra servicios.
- BancoRubyDbContext.cs: acceso a datos con Entity Framework y PostgreSQL.

## 3. Cómo funciona el servidor

El servidor usa una estructura simple y directa:

- Cada operación tiene su parte propia.
- Los endpoints llaman al servicio principal.
- El servicio valida datos y reglas antes de hacer cambios.
- Si todo está bien, se actualiza el saldo o se consulta información.
- Para transferencias, se usa un interceptor antes de aplicar los cambios.

## 4. Flujo de una petición

1. El cliente llama a una ruta del servidor.
2. El servidor valida si la cuenta es válida.
3. Se llama al servicio para hacer la operación.
4. El servicio revisa reglas y datos.
5. Se guarda el cambio o se devuelve la información.
6. Si la operación fue correcta, se publican eventos para auditoría.

## 5. Componentes clave

### Program.cs

- Configura la app.
- Registra servicios y rutas.
- Define endpoints para saldo, depósito, retiro, transferencia e historial.

### BancoRubyDbContext.cs

- Define las entidades de usuario, cuenta y auditoría.
- Conecta con PostgreSQL.

### Common/

- Cuenta.cs: modelos de dominio.
- Requests.cs: peticiones de entrada.
- DomainEvents.cs: eventos y bus de eventos.

### Features/Operaciones/BankService.cs

Es el corazón del sistema.

- Consultar saldo
- Depositar
- Retirar
- Transferir
- Ver historial

Usa un resultado de operación para manejar errores de forma clara.

### Features/Autenticacion/AccountAuthorizationFilter.cs

Valida que las cuentas existan y estén activas antes de seguir.

## 6. Cliente de usuario

El cliente de consola llama al servidor para hacer las operaciones. Se encarga de pedir datos al usuario y mostrar los resultados.

## 7. Cómo ejecutar el proyecto

### Servidor

```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby\Banco_Ruby"
dotnet run
```

### Cliente

```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby\Usuario_Cliente"
dotnet run
```

## 8. Lo importante para explicar

- El proyecto es simple y fácil de extender.
- La lógica de negocio está bien separada en partes claras.
- Hay validaciones antes de ejecutar operaciones.
- El sistema usa eventos para auditoría y seguimiento.
