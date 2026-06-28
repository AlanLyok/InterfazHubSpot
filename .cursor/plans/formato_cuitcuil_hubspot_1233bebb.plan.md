---
name: Formato cuitcuil HubSpot
overview: Formatear la clave cuitcuil_unica con puntos (grupos de 3 desde la derecha) para todos los tipos de documento, en un único helper.
todos:
  - id: format-helper
    content: Agregar FormatearParaHubSpot en HubSpotCuitCuilHelper y usarlo en TryGetClaveUnica
    status: pending
  - id: format-tests
    content: "Tests: CUIT 30-54798102-9, DNI 13018824, TryGetClaveUnica actualizado"
    status: pending
  - id: format-docs
    content: Postman env ejemplo + 1 línea hubspot-crm.md; correr tests unitarios
    status: pending
isProject: false
---

# Formato `cuitcuil_unica` con puntos (HubSpot)

## Confirmado por negocio

**Mismo formato para todos los tipos de documento:** grupos de 3 dígitos desde la derecha, separados por `.` (estilo miles AR / HubSpot).

| Tipo | ERP (normalizado) | HubSpot `cuitcuil_unica` |
|------|-------------------|--------------------------|
| CUIT | `30547981029` (`30-54798102-9`) | `30.547.981.029` |
| DNI | `13018824` | `13.018.824` |

No hay regla especial para 11 dígitos vs otros largos: **un solo algoritmo**.

## Problema

[`TryGetClaveUnica`](SolucionInterfazHubSpot/InterfazHubSpot.Business/Integration/HubSpotCuitCuilHelper.cs) devuelve solo dígitos; HubSpot persiste con puntos → el `EQ` en search no matchea.

## Solución (ponytail — 1 archivo + tests)

### Cambio en [`HubSpotCuitCuilHelper.cs`](SolucionInterfazHubSpot/InterfazHubSpot.Business/Integration/HubSpotCuitCuilHelper.cs)

1. `Normalizar` — sin cambios (quita `-`, `.`, `,`).
2. Nuevo `FormatearParaHubSpot(string soloDigitos)`:
   - ≤3 dígitos → devolver tal cual
   - resto → partir en grupos de 3 de derecha a izquierda, `string.Join(".", grupos)`
3. `TryGetClaveUnica` → `clave = FormatearParaHubSpot(Normalizar(...))`

**Sin cambios** en runner, SPs, CRM client (todos ya usan `TryGetClaveUnica`).

```csharp
// Ejemplos
FormatearParaHubSpot("30547981029") → "30.547.981.029"
FormatearParaHubSpot("13018824")    → "13.018.824"
FormatearParaHubSpot("123")         → "123"
```

## Tests ([`HubSpotCuitCuilHelperTests.cs`](SolucionInterfazHubSpot/InterfazHubSpot.Tests.Unit/Integration/HubSpotCuitCuilHelperTests.cs))

- `30-54798102-9` → `30.547.981.029`
- `13018824` → `13.018.824`
- `Normalizar` sigue devolviendo solo dígitos (tests existentes intactos)
- Actualizar `TryGetClaveUnica_ConDocumentoValido` (formato con puntos)

## Docs / Postman (mínimo)

- [`postman/InterfazHubSpot-Dev.postman_environment.json`](postman/InterfazHubSpot-Dev.postman_environment.json): ejemplo `cuitCuilUnica` con puntos
- Una línea en [`docs/reference/hubspot-crm.md`](docs/reference/hubspot-crm.md)

## Verificación

```powershell
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Unit -SkipBuild
```

## Fuera de alcance

- SQL 004/006 (siguen entregando dígitos limpios)
- Doble búsqueda con/sin puntos
- Validación de tipo o checksum de documento
