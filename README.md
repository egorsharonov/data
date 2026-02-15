# Informing Parameter Service

.NET 8 worker service for Camunda external task topic `portin-ext-data-enrich`.

Service responsibilities:
- fetch task variables (`orderId`, `eventType`, optional `requestedParameters`);
- resolve which external parameters are required (from task variable first, from configuration fallback);
- request each parameter via pluggable `IExternalParameterProvider` handlers;
- return resolved data to Camunda in process variable `externalParameters`.

## How to add new external parameter
1. Add parameter key in DMN/process (or pass via `requestedParameters`).
2. Implement `IExternalParameterProvider` for that key.
3. Register provider in `AddExternalParameterProviders`.
4. Optionally configure fallback mapping in `Infrastructure:Parameters:Resolution`.
