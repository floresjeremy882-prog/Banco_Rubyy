# Documentación del proyecto Banco Ruby

## Resumen
Este proyecto tiene dos partes:

- Banco_Ruby: es la API del banco.
- Usuario_Cliente: es un programa de consola que usa esa API.

La carpeta Documentacion guarda los documentos y diagramas del proyecto.

## Estructura simple
El servidor está organizado de forma clara y sencilla:

- Common/: modelos y peticiones compartidas.
- Features/: cada operación tiene su propia parte.
  - Autenticacion
  - Operaciones
  - Transferencias
  - Historial

## ¿Por qué está organizado así?
- Cada función vive en su propio espacio.
- Es más fácil entender y mantener el código.
- El servidor expone rutas simples para cada acción.

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
- archivos con la explicación general del proyecto
