# MCP MSSQL (desarrollo, Cursor)

Herramienta local opcional para conectar Cursor al **servidor MCP** oficial [mssql-mcp-server en PyPI](https://pypi.org/project/mssql-mcp-server/). En este monorepo el id recomendado del servidor es **`mssql-spertaapi-dev`** (ver [`docs/reference/mcp-mssql-spertaapi-dev.md`](../../docs/reference/mcp-mssql-spertaapi-dev.md)).

Permite explorar tablas y ejecutar SQL *desde el agente IDE* cuando trabajamos con `StoresAPI`, migraciones (`scripts/SpertaAPI/migrations/`, etc.), alineado a las bases que usa el proyecto. **No sustituye** scripts versionados ni revisiones formales del esquema.

## Requisitos

- **Python 3.11 o superior** (el paquete lo exige según metadata en PyPI). Si `bootstrap-venv.ps1` avisa que no hay 3.11:
  ```powershell
  winget install Python.Python.3.11
  ```
  Tras instalar, **cerrá y volvé a abrir** PowerShell y comprobá: `py -3.11 --version`.
- **[Microsoft ODBC Driver for SQL Server](https://learn.microsoft.com/sql/connect/odbc/download-odbc-driver-for-sql-server)** acorde al entorno donde corre el proceso Python (muchas combinaciones ODBC + `pyodbc`/`pymssql` fallan sin driver instalado).

## Arranque rápido

1. Crear virtualenv solo con 3.11 (recomendado: script `bootstrap-venv.ps1` en esta carpeta).

   ```powershell
   cd services/mssql-mcp-server
   powershell -NoProfile -ExecutionPolicy Bypass -File .\bootstrap-venv.ps1
   ```

2. **Conexión en Cursor:** editá [.cursor/mcp.json](../../.cursor/mcp.json) y completá las variables en el objeto **`env`** (`MSSQL_HOST`, `MSSQL_USER`, `MSSQL_PASSWORD`, `MSSQL_DATABASE`, `MSSQL_DRIVER`, `TrustServerCertificate`, `Trusted_Connection`). Por defecto el repo lleva placeholders (`CAMBIAME`); **no commitees contraseñas reales.** Guardá el JSON y **reiniciá Cursor o recargá los servidores MCP**.

3. Opcional — **solo para [`test-connection.py`](./test-connection.py)** (CLI): copiá `.env.example` → `.env` (`services/mssql-mcp-server/.env` está en `.gitignore`). El MCP puede vivir sólo del bloque `env` del JSON sin `.env`.

4. Probar proceso manualmente (stdio MCP) con los mismos datos que pusiste en `env`:

   El paquete PyPI **no** soporta `python -m mssql_mcp_server` (no define `__main__`). Arrancá [`run_mcp_stdio.py`](./run_mcp_stdio.py) con las variables cargadas antes (ej. PowerShell: `$env:MSSQL_HOST='...'` etc.):

   ```powershell
   .\.venv\Scripts\Activate.ps1
   python run_mcp_stdio.py
   ```

## Probar conexión (sin abrir Cursor)

Con `.env` listo (si lo usás para `test-connection`):

```powershell
cd services/mssql-mcp-server
.\.venv\Scripts\python.exe .\test-connection.py
```

Debe imprimir `OK: conexion SQL Server establecida` y la base efectiva (`DB_NAME()`). No muestra usuario ni contraseña.

## Variables de entorno (`MSSQL_*`)

Las documenta el proyecto en PyPI; convención usada aquí:

| Variable | Obligatoriedad | Descripción |
|----------|----------------|-------------|
| `MSSQL_HOST` | Sí | Instancia/host SQL Server |
| `MSSQL_USER` | Depende si `Trusted_Connection` | Usuario SQL (dev, mínimos privilegios) |
| `MSSQL_PASSWORD` | Ídem | Contraseña (no commitear) |
| `MSSQL_DATABASE` | Sí | Base de trabajo |
| `MSSQL_DRIVER` | Según código del paquete | Nombre ODBC u otro driver soportado |
| `Trusted_Connection` | Opcional (`yes`/`no`) | Autenticación Windows vs SQL login |
| `TrustServerCertificate` | Opcional | Típico en dev |

Convenciones de paquetes y versiones: ver [PyPI User Documentation — Installing packages](https://docs.pypi.org/).

Fusión manual MCP o otra máquina: [.cursor/mcp.mssql-mcp-server.example.json](../../.cursor/mcp.mssql-mcp-server.example.json). How-to detallado: [docs/how-to/mcp-mssql-desarrollo-cursor.md](../../docs/how-to/mcp-mssql-desarrollo-cursor.md).

- Solo **bases de desarrollo**; crear un **usuario SQL dedicado** con privilegios mínimos. Si las credenciales viven sólo en el JSON de MCP, mejor **config global de Cursor** o un `mcp.json` local fuera del control de versiones, para no commitearlas.

- Para producción aplicar políticas corporativas (`docs/reference/seguridad-produccion-spertaapi.md`); **no exponer MCP** así a usuarios finales ERP sin borde auth/auditoría.

## Skill Cursor

Cuando necesites prácticas coherentes del monorepo, ver [`.cursor/skills/mssql-mcp-desarrollo/SKILL.md`](../../.cursor/skills/mssql-mcp-desarrollo/SKILL.md).

Para interpretar **`EmpresaID = 0`** en maestros ERP y **`dbo.TablasCompartidas`**, ver [`docs/reference/msgestion-tablas-compartidas-y-empresaid.md`](../../docs/reference/msgestion-tablas-compartidas-y-empresaid.md).
