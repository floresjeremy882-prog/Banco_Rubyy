Resumen de reemplazos de `var`

Fecha: 2026-07-14

Objetivo
- Reemplazar usos de `var` por tipos explícitos cuando mejora la claridad y la seguridad del código.

Cambios realizados
- Banco_Ruby/Program.cs
  - `WebApplicationBuilder`, `WebApplication`, `Cuenta`, `JsonDocument`, `JsonElement`, y DTOs (`DepositoRequest`, `RetiroRequest`, `TransferenciaRequest`) declarados explícitamente.
- Banco_Ruby/Features/
  - `AutenticacionSlice.cs`, `DepositarSlice.cs`, `RetirarSlice.cs`, `TransferirSlice.cs`: resultados de consultas EF (`Cuenta`, `origen`, `destino`) ahora con tipos explícitos.
  - `HistorialSlice.cs`: se mantuvo `var` para la proyección anónima (`Select(new { ... })`) porque no es posible declarar un tipo anónimo explícitamente sin cambiar la API.
- Usuario_Cliente/
  - `Program.cs`: `apiBaseUrl`, `CajeroApiClient` y `CajeroConsole` ahora explícitos.
  - `Services/CajeroApiClient.cs`: las respuestas HTTP usan `HttpResponseMessage` en lugar de `var`.
  - `Services/CajeroConsole.cs`: múltiples `var` reemplazados por `string`, `decimal`, `JsonDocument`, `JsonElement`, `Table`, `Panel`, `List<JsonElement>` y variables locales explícitas. También se cambió `var t` en patrones por `string t`.

Motivación y reglas aplicadas
- Reemplazado cuando el tipo es claro en la asignación (resultado de una consulta EF, retorno de `HttpClient`, parsing de JSON, entradas de usuario). 
- Mantuvimos `var` cuando el tipo es anónimo (proyecciones LINQ con `new { ... }`) o cuando la sintaxis lo requiere (por ejemplo, `await` con tipos anónimos). 
- En patrones `switch` se prefirió `string t when ...` para mantener el patrón y evitar `var`.

Verificación
- Ambos proyectos compilaron correctamente en salidas alternativas (`dotnet build -o _out -p:UseAppHost=false`).
- Advertencia remanente: `Npgsql 8.0.0` con advisory GHSA-x9vc-6hfv-hg8c. Considerar fijar versión o mitigación.

Dónde quedan `var`
- `HistorialSlice.cs`: `auditorias` es una proyección anónima (se mantiene `var`).
- Cualquier otra aparición de `var` donde la reescritura no mejore claridad se dejó sin tocar.

Siguientes pasos recomendados
- Ejecutar pruebas manuales end-to-end con la BD (ejecutar script en `Documentacion/postgresql_schema.sql`).
- Decidir estrategia para la advertencia de `Npgsql` (downgrade/upgrade o mitigación).

Generado por: agente de codificación (Copilot)
