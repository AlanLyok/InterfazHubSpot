# Configurar entorno de desarrollo local

**Tipo:** How-to.  
**Tutorial rápido:** [`../QUICKSTART.md`](../QUICKSTART.md).  
**Claves config:** [`../reference/configuracion.md`](../reference/configuracion.md).

---

## 1. Clonar y compilar

```powershell
git clone <repo-url>
cd INTERFAZHUBSPOT
copy SolucionInterfazHubSpot\Web.config.example SolucionInterfazHubSpot\InterfazHubSpot\Web.config
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Build-InterfazHubSpot.ps1
```

---

## 2. Web.config mínimo

Editar `InterfazHubSpot/Web.config`:

| Setting | Dev sin HubSpot real | Dev smoke HubSpot |
|---------|---------------------|-------------------|
| `MSGestion` connection string | BD local/dev Calzetta | Igual |
| `HubSpot:UseDevelopmentMock` | `true` | `false` |
| `HubSpot:PrivateAppToken` | vacío | `pat-...` |
| `EmailErrPara` | email dev (opcional) | email dev |

---

## 3. Base de datos

Ejecutar [`../../scriptsSQL/000_Deploy_All.sql`](../../scriptsSQL/000_Deploy_All.sql) contra MSGestion de dev.

Sin BD: solo build + tests unitarios (`Verify-InterfazHubSpot.ps1 -LibrariesOnly`).

---

## 4. Abrir solución

`SolucionInterfazHubSpot/InterfazHubSpot.sln` en Visual Studio → F5 en proyecto `InterfazHubSpot`.

Home muestra botones de trazas. Ver [`debug-integracion.md`](debug-integracion.md).

---

## 5. Verificar antes de commit

```powershell
powershell.exe -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1
```

---

## MCP SQL en Cursor

1. Copiar `.cursor/mcp.mssql-mcp-server.example.json` → configuración MCP local.
2. Leer schema tools en `mcps/project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA/tools/`.
3. Agentes: [`../agents/INDEX.md`](../agents/INDEX.md).
