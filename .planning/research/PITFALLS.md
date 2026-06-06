# Pitfalls Research — Integración HubSpot

**Researched:** 2026-06-06

## 1. Tabla HubSpotCompanyId desconocida

**Señal:** SP `USER_HS_Cliente_GuardarHubSpotId` no compila o no persiste.  
**Prevención:** Confirmar con Calzetta antes de implementar SP 6.  
**Fase:** 1 (SQL)

## 2. Rate limit HubSpot (429)

**Señal:** Errores intermitentes con muchos contactos por cliente.  
**Prevención:** Delay 120ms entre llamadas; backoff máx. 3 intentos; detener job en agotamiento.  
**Fase:** 2 (HubSpotClient)

## 3. Token PAT en repositorio

**Señal:** Commit accidental de `Web.config` con token.  
**Prevención:** Claves comentadas en repo; `UseDevelopmentMock=true` en dev; revisar `.gitignore`.  
**Fase:** 2 (config)

## 4. Clientes sin HubSpotCompanyId en 2B

**Señal:** Batches fallan o omiten registros.  
**Prevención:** Omitir y loguear advertencia; no interrumpir proceso completo (PRD §7.5).  
**Fase:** 4 (Flujo 2B)

## 5. Duplicados en cola

**Señal:** Mismo cliente procesado múltiples veces.  
**Prevención:** `USER_POS_Clientes_Agregar` verifica `Pendiente` existente.  
**Fase:** 1 (SQL)

## 6. Formato cuenta corriente incorrecto

**Señal:** Campo HubSpot ilegible o rechazado.  
**Prevención:** Tests unitarios del formateador: `DD/MM/YYYY --- $NNN.NNN,NN` y línea sin deuda.  
**Fase:** 4 (Flujo 2B)

## 7. Migración incompleta desde SpertaAPI

**Señal:** Jobs siguen llamando `GetIntegracionesClienteAsync`.  
**Prevención:** Fase 5 de limpieza; grep de `HttpSpertaApiClient` en `InterfazHubSpot`.  
**Fase:** 5 (Limpieza)
