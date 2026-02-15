# Informing Parameter Service

`Informing Parameter Service` — .NET 8 worker-сервис для Camunda external task `portin-ext-data-enrich`.

## Как теперь определяется список параметров

Список параметров **должен приходить из процесса через DMN**:

1. В `portin-process.bpmn` перед шагом `Enrichment [Parameters Service]` вызывается `parameter_requirements_decision`.
2. DMN записывает результат в переменную `requestedParameters`.
3. Эта переменная пробрасывается как input в external task.
4. Worker читает `requestedParameters` и запрашивает только эти параметры у провайдеров.

Поддерживаемый формат `requestedParameters`:
- JSON array, например `"[\"msisdn\",\"segment\"]"`;
- либо CSV, например `msisdn,segment`.

## Что делает сервис

На каждой итерации worker:
1. Забирает задачи из Camunda (`fetchAndLock`) по топику parameter service.
2. Извлекает переменные задачи: `orderId`, `eventType`, `requestedParameters`.
3. Для каждого ключа из `requestedParameters` ищет `IExternalParameterProvider`.
4. Запрашивает значения параметров через провайдеры.
5. Завершает task и возвращает:
   - `externalParameters`
   - `externalParametersRequested`

---

## Требования для локального запуска

- .NET SDK 8.0+
- доступ к Camunda REST API
- (опционально) Docker

Проверка SDK:

```bash
dotnet --info
```

---

## Настройка конфигурации

Основные секции в `src/Informing.Data.Worker/appsettings*.json`:

- `Infrastructure:Camunda:PollingOptions`
  - `BaseUrl`
  - `NormalPollingInterval`
  - `ErrorPollingInterval`
- `Infrastructure:Camunda:WorkerOptions:ParameterService`
  - `WorkerId`, `TopicName`, `LockDurationMs`, `MaxBatchTasks`, `RetriesOnFailure`, `TenantId`

### Переопределение через env

```bash
export Infrastructure__Camunda__PollingOptions__BaseUrl="http://localhost:8080"
export Infrastructure__Camunda__WorkerOptions__ParameterService__TopicName="portin-ext-data-enrich"
```

---

## Сборка / тесты / запуск

```bash
dotnet restore Informing.Data.sln
dotnet build Informing.Data.sln -c Release --no-restore
dotnet test Informing.Data.sln -c Release
dotnet run --project src/Informing.Data.Worker/Informing.Data.Worker.csproj
```

---

## Как добавить новый внешний параметр

1. Добавь ключ параметра в `parameter_requirements_decision.dmn` для нужного `eventType`.
2. Убедись, что BPMN пробрасывает `requestedParameters` в шаг `Enrichment [Parameters Service]`.
3. Реализуй `IExternalParameterProvider` с `ParameterKey` этого ключа.
4. Зарегистрируй провайдер в `AddExternalParameterProviders`.
5. Добавь unit-тесты для провайдера/логики.

---

## Docker

```bash
docker build -t informing-parameter-service -f Dockerfile .
docker run --rm -e Infrastructure__Camunda__PollingOptions__BaseUrl="http://host.docker.internal:8080" informing-parameter-service
```
