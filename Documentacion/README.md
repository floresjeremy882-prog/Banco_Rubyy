# Documentación del Proyecto Banco Ruby

## Resumen
Este proyecto contiene dos aplicaciones separadas:

- `Banco_Ruby`: servidor de la API bancaria con arquitectura vertical slice ligera.
- `Usuario_Cliente`: cliente de consola que consume las APIs del servidor.

La carpeta `Documentacion` conserva los diagramas originales.

## Arquitectura
El servidor usa una organización vertical slice simple:

- `Common/`: tipos de dominio compartidos entre slices.
- `Features/`: cada característica tiene su propia clase de slice.
  - `AutenticacionSlice.cs`
  - `DepositarSlice.cs`
  - `RetirarSlice.cs`
  - `HistorialSlice.cs`

Cada slice encapsula su lógica de negocio y expone una operación concreta para la API.

## ¿Por qué es vertical slice?
- Cada función (consultar saldo, depositar, retirar, historial) vive en su propio slice.
- No hay grandes capas tradicionales separadas por persistencia, servicio y controlador.
- El servidor expone rutas mínimas que delegan directamente a los slices.

## Uso
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

### Flujos disponibles
- `Consultar saldo`
- `Depositar`
- `Retirar`
- `Historial`

## Documentos incluidos
- `diagrama_alto_nivel.png`
- `diagrama_componentes.png`
- `diagrama_dependencia.png`
- `Reglas_Del_Negocio`
- `vertical-slice.md`
