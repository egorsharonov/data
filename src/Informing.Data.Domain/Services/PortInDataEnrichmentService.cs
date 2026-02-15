using System.Diagnostics;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Contracts.Dal.Interfaces;
using Informing.Data.Domain.Contracts.Observability;
using Informing.Data.Domain.Enums;
using Informing.Data.Domain.Exceptions.Domain.PortIn;
using Informing.Data.Domain.Exceptions.Infrastructure;
using Informing.Data.Domain.Exceptions.Infrastructure.Camunda;
using Informing.Data.Domain.Mappers.PortIn;
using Informing.Data.Domain.Models.PortIn;
using Informing.Data.Domain.Models.PortIn.Common;
using Informing.Data.Domain.Models.Rtm;
using Informing.Data.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Informing.Data.Domain.Services;

public class PortInDataEnrichmentService : IPortInDataEnrichmentService
{
    private readonly ILogger<PortInDataEnrichmentService> _logger;
    private readonly IPortInOrdersRepository _ordersRepository;
    private readonly ICamundaClient _camundaClient;
    private readonly IWorkerInstrumentation _portInWorkerInstrumentation;

    public PortInDataEnrichmentService(
        ILogger<PortInDataEnrichmentService> logger,
        IPortInOrdersRepository portInOrdersRepository,
        [FromKeyedServices(CamundaWorkerTag.PortIn)]
        ICamundaClient camundaClient,
        [FromKeyedServices(CamundaWorkerTag.PortIn)]
        IWorkerInstrumentation portInWorkerInstrumentation
    )
    {
        _logger = logger;
        _ordersRepository = portInOrdersRepository;
        _camundaClient = camundaClient;
        _portInWorkerInstrumentation = portInWorkerInstrumentation;
    }

    public async Task ProcessEnrichmentTasks(CancellationToken cancellationToken)
    {
        using var processTasksActivity = _portInWorkerInstrumentation.StartProcessCamundaTasksActivity();

        try
        {
            IReadOnlyList<EnrichProcessTaskContainer> taskContainers =
                                    await _camundaClient.FetchAndLockEnrichmentTasks(cancellationToken);

            processTasksActivity?.SetTag(
                key: _portInWorkerInstrumentation.CamundaTaskNumberKey,
                value: taskContainers.Count);

            if (taskContainers.Count != 0)
            {
                _logger.LogInformation("Received {numOfTasks} tasks from Camunda", taskContainers.Count);

                foreach (var task in taskContainers)
                {
                    await ProcessEnrichmentTask(
                        taskContainer: task,
                        cancellationToken: cancellationToken
                    );
                }
            }
            else
            {
                _logger.LogDebug("No tasks avaliable. Waiting before next pole");
                processTasksActivity?.Stop();
            }

        }
        catch (Exception ex)
        {
            processTasksActivity?.AddException(ex);
            processTasksActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(ex, "Exception occured during PortIn enrichment tasks processing.");
            throw;
        }

    }

    public async Task ProcessEnrichmentTask(EnrichProcessTaskContainer taskContainer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing task with id: {taskId} from process: {processId}", taskContainer.Id, taskContainer.ProcessInstanceId);

        using var processTaskActivity = _portInWorkerInstrumentation.StartProcessCamundaTaskActivity(
            taskId: taskContainer.Id,
            processInstaceId: taskContainer.ProcessInstanceId
        );

        try
        {
            await ProcessEnrichmentTaskUnsafe(taskContainer, cancellationToken);

            _logger.LogInformation("Task with id: {taskId} processed succesfully", taskContainer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process task with id: {taskId} from process: {processId} with orderId: {orderId}",
                            taskContainer.Id, taskContainer.ProcessInstanceId, taskContainer.EnrichmentTaskVariables?.OrderId);

            // Для ошибок в Camunda DTO нет переповторов
            int retiresLeft = ex is CamundaTaskInvalidVariableException ? 0 : taskContainer.RetriesLeft;

            await _camundaClient.FailTask(
                taskId: taskContainer.Id,
                taskError: ex,
                retries: retiresLeft,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task ProcessEnrichmentTaskUnsafe(EnrichProcessTaskContainer taskContainer, CancellationToken cancellationToken)
    {
        if (taskContainer.VariableException != null)
        {
            throw taskContainer.VariableException;
        }

        if (taskContainer.EnrichmentTaskVariables == null)
        {
            throw new CamundaTaskInvalidVariableException(
                message: $"Task with id: {taskContainer.Id} doesnt contain any required variable.",
                variableKey: "",
                taskId: taskContainer.Id
        );
        }

        using var transaction = _ordersRepository.CreateTransactionScope();

        var orderParams = await ResolveOrderParameters(
            orderId: taskContainer.EnrichmentTaskVariables.OrderId,
            cancellationToken
        );

        var rtmMesage = FormRtmMessage(
            orderId: taskContainer.EnrichmentTaskVariables.OrderId,
            eventType: taskContainer.EnrichmentTaskVariables.EventType,
            orderParams: orderParams,
            cancellationToken: cancellationToken
        );

        await _camundaClient.CompleteTask(
            taskId: taskContainer.Id,
            rtmMessage: rtmMesage,
            cancellationToken: cancellationToken
        );

        transaction.Complete();
    }

    private async Task<PortInOrder> ResolveOrderParameters(long orderId, CancellationToken cancellationToken)
    {
        try
        {
            var orderEntity = await _ordersRepository.GetByOrderId(
                orderId: orderId,
                cancellationToken: cancellationToken
            );

            return orderEntity.ToModel();
        }
        catch (EntityNotFoundException ex)
        {
            throw new PortInOrderNotFoundException(
                message: $"PortIn order with id: {orderId} not found",
                orderId: orderId,
                innerException: ex
            );
        }
    }

    private RtmMessage FormRtmMessage(long orderId, OrderStateCode eventType, PortInOrder orderParams, CancellationToken cancellationToken)
    {
        if (orderParams.PortationNumbers.Count == 0)
        {
            throw new PortInOrderInvalidStateException(
                message: $"PortIn order with id {orderId} does not contain portation numbers.",
                orderId: orderId
            );
        }

        var portInNum = orderParams.PortationNumbers[0];

        var message = new RtmMessage
        {
            Key = portInNum.Msisdn,
            Type = eventType.ToRtmEventType(),
            DateEvent = DateTimeOffset.Now,
            PortInNumber = portInNum.Msisdn,
        };

        // TODO: AV - уточнить мапинг c учетом Code_reject, где его брать, что определяет наличие Option, Date_of_action
        switch (eventType)
        {
            case OrderStateCode.SentCdb:
                message.PortInTemporary = portInNum.TelcoAccount.Msisdn;
                message.PortDateTime = orderParams.DueDate;
                break;
            case OrderStateCode.ArbitrationPending:
                message.PortInTemporary = portInNum.TelcoAccount.Msisdn;
                break;
            case OrderStateCode.DonorVerification:
                message.PortDateTime = orderParams.DueDate;
                // message.DateOfAction
                message.Options = RtmOptionalType.Disabled;
                break;
            case OrderStateCode.DebtChecking or OrderStateCode.PortationComplete:
                // Достаточно обязательных полей
                break;
            case OrderStateCode.Canceled:
                message.RejectCode = 7061;
                message.PortInTemporary = portInNum.TelcoAccount.Msisdn;
                break;
            // case OrderStateCode.CdbRejected:
            // break;
            // case OrderStateCode.DonorRejected:
            // break;
            // case OrderStateCode.DebtCollection:
            // break;
            default:
                throw new PortInOrderUnacceptableEventType(
                    message: $"Unacceptable event type for informing for order with id: {orderId}",
                    orderId: orderId,
                    unnaceptableEventType: eventType
                );
        }

        return message;
    }
}