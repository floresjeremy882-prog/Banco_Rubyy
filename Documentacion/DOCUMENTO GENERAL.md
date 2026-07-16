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

- Common/: modelos y peticiones compartidas.
- Features/: lógica de negocio por caso de uso.
  - Autenticacion: validación de cuentas.
  - Operaciones: depósito, retiro y respuestas de operación.
  - Transferencias: lógica de transferencia.
  - Historial: consultas del historial.
- Program.cs: define las rutas y registra servicios.
- BancoRubyDbContext.cs: acceso a datos con Entity Framework y PostgreSQL.

## 3. Cómo funciona el servidor

El servidor usa una estructura simple y directa con vertical slice:

- Cada operación tiene su parte propia.
- Los endpoints llaman directamente a un slice.
- Cada slice valida los datos y aplica las reglas correspondientes.
- Si todo está bien, se actualiza el saldo o se devuelve la información.
- Las transferencias se procesan directamente en `TransferirSlice`.

## 4. Flujo de una petición

1. El cliente llama a una ruta del servidor.
2. El servidor valida si la cuenta es válida.
3. Se llama al servicio para hacer la operación.
4. El servicio revisa reglas y datos.
5. Se guarda el cambio o se devuelve la información.
- Si la operación fue correcta, se guarda auditoría directamente en la base de datos.

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

### Features/Operaciones

Cada slice de `Features/Operaciones` maneja una operación concreta como depósito o retiro.

- `DepositarSlice.cs`: depósito con comisión y auditoría.
- `RetirarSlice.cs`: retiro con comisión y auditoría.

Usa respuestas directas de los slices para manejar errores y resultados.

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
- El servidor usa slices verticales y evita una capa de servicio única.
- Depósitos y retiros aplican comisión y registran auditoría con descripciones claras.
