# Documentación del proyecto Banco Ruby

## Resumen
Este proyecto tiene dos partes principales:

- Banco_Ruby: API del banco con operaciones de cuentas, transferencias y auditoría.
- Usuario_Cliente: cliente de consola que consume la API.

La carpeta Documentacion reúne los documentos, diagramas y reglas de negocio del sistema.

## Arquitectura actual
El backend usa una mezcla de vertical slice y un enfoque DDD ligero:

- Features/: contiene los slices de aplicación por caso de uso.
- Domain/: encapsula la regla de negocio crítica de las transferencias.
- Infrastructure/: abstrae la integración externa con el banco destino.
- Common/: modelos y request DTOs compartidos.

## Reglas de negocio actuales
- Depósito: aplica una comisión fija de $0.41.
- Retiro: exige monto múltiplo de 10, máximo $500 y aplica comisión de $0.41.
- Transferencia: si la integración externa falla por timeout o excepción, el sistema revierte el movimiento y devuelve un mensaje de transacción fallida.
- Todas las operaciones registran auditoría con descripciones legibles.

## Cómo usarlo
### Ejecutar el servidor
Desde PowerShell:

```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby\Banco_Ruby"
dotnet run
```

### Ejecutar el cliente
En otra terminal:

```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby\Usuario_Cliente"
dotnet run
```

### Ejecutar pruebas
```powershell
cd "c:\Users\jerenmi.flores\Downloads\Bnaco_Ruby\Bnaco_Ruby"
dotnet test BancoRuby.Tests/BancoRuby.Tests.csproj
```

### Opciones disponibles
- Consultar saldo
- Depositar
- Retirar
- Transferir
- Ver historial

## Documentos incluidos
- Reglas_Del_Negocio
- vertical-slice.md
- interceptor_transferencia.md
- DOCUMENTO GENERAL.md
- DOCUMENTO PARA EXPLICAR.md
