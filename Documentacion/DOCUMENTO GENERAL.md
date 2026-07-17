# Documento general

## 1. Propósito del proyecto

Este proyecto implementa un pequeño sistema bancario con dos componentes principales:

- Banco_Ruby: API REST para manejar cuentas, saldos, depósitos, retiros, transferencias e historial.
- Usuario_Cliente: cliente de consola que consume la API para probar las operaciones.

## 2. Estructura general

### 2.1 Carpetas principales

- Banco_Ruby: servidor principal.
- Usuario_Cliente: cliente de consola.
- Documentacion: guías, diagramas y reglas del negocio.

### 2.2 Dependencias principales

- Microsoft.EntityFrameworkCore 8.0.0: acceso a datos.
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0: proveedor PostgreSQL.
- Spectre.Console: interfaz de consola del cliente.

### 2.3 Estructura del servidor

Dentro de Banco_Ruby:

- Common/: modelos y request DTOs compartidos.
- Features/: slices de aplicación por caso de uso.
  - Autenticacion
  - Operaciones
  - Transferencias
  - Historial
- Domain/: lógica de dominio para reglas críticas, como la transferencia externa.
- Infrastructure/: integración externa para la transferencia al banco destino.
- Program.cs: define rutas y registra servicios.
- BancoRubyDbContext.cs: contexto de EF Core con PostgreSQL.

## 3. Cómo funciona el servidor

El servidor usa un enfoque híbrido:

- Vertical slice para separar cada caso de uso.
- Capa de dominio ligera para concentrar reglas sensibles.
- Infrastructure para encapsular la interacción externa.

Esto permite que la lógica de negocio no quede mezclada con la API y que las operaciones sean más fáciles de mantener.

## 4. Flujo de una petición

1. El cliente llama a una ruta del servidor.
2. El endpoint valida que las cuentas existan y estén activas.
3. El slice correspondiente ejecuta la operación.
4. La regla de dominio decide si la operación es válida.
5. Si la operación es correcta, se actualiza el saldo y se registra auditoría.

## 5. Flujo de transferencia actual

La transferencia ahora sigue este comportamiento:

1. Se descuenta el monto de la cuenta origen y se acredita en la cuenta destino en memoria.
2. Se intenta enviar la operación al banco destino mediante el gateway externo.
3. Si la integración falla por timeout o excepción, el sistema revierte los saldos y devuelve un mensaje de transacción fallida.
4. Se registra la auditoría de la operación con el resultado final.

## 6. Componentes clave

### Program.cs

- Configura la app web.
- Registra el contexto de base de datos y el gateway de transferencias.
- Define endpoints para saldo, depósito, retiro, transferencia e historial.

### BancoRubyDbContext.cs

- Define las entidades Usuario, Cuenta y Auditoria.
- Mapea los modelos a tablas PostgreSQL.

### Common/

- Cuenta.cs: modelos de dominio.
- Requests.cs: DTOs de entrada.

### Domain/Transferencias

- TransferenciaService.cs: aplica la regla de negocio y la reversión ante fallo externo.

### Infrastructure/

- TransferenciaGateway.cs: punto de integración con el proceso externo de transferencia.

## 7. Ejecución

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

### Pruebas

```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby"
dotnet test BancoRuby.Tests/BancoRuby.Tests.csproj
```

## 8. Puntos clave para explicar

- El proyecto es simple, pero ya incorpora una separación más clara entre aplicación, dominio e infraestructura.
- La transferencia tiene protección contra fallos externos.
- La auditoría permite rastrear cada movimiento.
- El sistema está preparado para evolucionar hacia una integración real con un servicio bancario externo.
