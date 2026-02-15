# Informing Parameter Service

`Informing Parameter Service` — это .NET 8 worker-сервис, который обрабатывает Camunda external task по топику `portin-ext-data-enrich`.

## Что делает сервис

На каждой итерации worker:
1. Забирает задачи из Camunda (`fetchAndLock`) по топику parameter service.
2. Извлекает переменные задачи:
   - `orderId`
   - `eventType`
   - `requestedParameters` (опционально)
3. Определяет, какие внешние параметры нужны:
   - сначала берёт `requestedParameters` из задачи,
   - если там пусто — пытается найти правила в конфиге `Infrastructure:Parameters:Resolution:EventTypeToParameters`,
   - если и там нет — использует `DefaultParameters`.
4. Для каждого ключа параметра ищет соответствующий `IExternalParameterProvider`.
5. Собирает значения параметров и завершает task в Camunda, возвращая:
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

## Структура решения

- `src/Informing.Data.Worker` — точка входа, background worker, hosting.
- `src/Informing.Data.Domain` — бизнес-логика параметризации и обогащения.
- `src/Informing.Data.Infrastructure` — Camunda клиент, конфигурация, healthcheck/observability.
- `tests/Informing.Data.Domain.Tests` — unit-тесты domain-слоя.
- `parameter_requirements_decision.dmn` — пример DMN для правил выбора параметров.

---

## Настройка конфигурации

Основные секции в `src/Informing.Data.Worker/appsettings*.json`:

- `Infrastructure:Camunda:PollingOptions`
  - `BaseUrl` — базовый URL Camunda (без `/engine-rest` тоже допустимо).
  - `NormalPollingInterval` — интервал опроса при успешной итерации.
  - `ErrorPollingInterval` — интервал при ошибке.
- `Infrastructure:Camunda:WorkerOptions:ParameterService`
  - `WorkerId`, `TopicName`, `LockDurationMs`, `MaxBatchTasks`, `RetriesOnFailure`, `TenantId`.
- `Infrastructure:Parameters:Resolution`
  - `EventTypeToParameters` — fallback-правила для `eventType`.
  - `DefaultParameters` — fallback второго уровня.

### Быстрое переопределение через переменные окружения

Пример для локального запуска:

```bash
export Infrastructure__Camunda__PollingOptions__BaseUrl="http://localhost:8080"
export Infrastructure__Camunda__WorkerOptions__ParameterService__TopicName="portin-ext-data-enrich"
```

---

## Как собрать проект

Из корня репозитория:

```bash
dotnet restore Informing.Data.sln
dotnet build Informing.Data.sln -c Release --no-restore
```

---

## Как прогнать тесты

### Все тесты

```bash
dotnet test Informing.Data.sln -c Release
```

### Только domain unit-тесты

```bash
dotnet test tests/Informing.Data.Domain.Tests/Informing.Data.Domain.Tests.csproj -c Release
```

Тесты покрывают:
- выбор источника списка параметров в `ParameterRequirementsResolver`;
- happy-path обработки задачи в `ParameterEnrichmentService`;
- fail-path (если провайдер параметра не зарегистрирован).

---

## Как запустить локально

### Вариант 1: запуск из проекта worker

```bash
dotnet run --project src/Informing.Data.Worker/Informing.Data.Worker.csproj
```

### Вариант 2: запуск через профиль `Development`

```bash
DOTNET_ENVIRONMENT=Development dotnet run --project src/Informing.Data.Worker/Informing.Data.Worker.csproj
```

После старта в логах должны появиться сообщения о запуске parameter worker и попытках опроса Camunda.

---

## Как добавить новый внешний параметр

1. Добавь ключ параметра в BPMN/DMN (или передавай его в `requestedParameters`).
2. Реализуй новый класс `IExternalParameterProvider`.
3. Зарегистрируй реализацию в `AddExternalParameterProviders`.
4. При необходимости добавь fallback-правила в `Infrastructure:Parameters:Resolution`.
5. Добавь/обнови unit-тесты для новой логики.

---

## Docker (если нужен)

В репозитории есть `Dockerfile` и `Dockerfile.dev`.

Типовой сценарий:

```bash
docker build -t informing-parameter-service -f Dockerfile .
docker run --rm -e Infrastructure__Camunda__PollingOptions__BaseUrl="http://host.docker.internal:8080" informing-parameter-service
```
