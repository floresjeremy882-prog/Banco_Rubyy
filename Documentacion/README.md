# Documentación del proyecto Banco Ruby

## Resumen
Este proyecto tiene dos partes:

- Banco_Ruby: es la API del banco.
- Usuario_Cliente: es un programa de consola que usa esa API.

La carpeta Documentacion guarda los documentos y diagramas del proyecto.

## Estructura simple
El servidor está organizado de forma clara y sencilla usando slices verticales:

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
- El backend evita capas innecesarias y pone la lógica de negocio en slices directos.

## Comisiones y comportamiento actual
- Depósitos y retiros aplican una comisión fija de $0.41.
- Las transacciones registran auditoría con descripciones legibles.
- El cliente de consola confirma la comisión antes de enviar la operación.

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
