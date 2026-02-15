using Informing.Data.Domain.Contracts.Dal.Entities;
using Informing.Data.Domain.Exceptions.Domain.PortIn;
using Informing.Data.Domain.Models.PortIn;
using Informing.Data.Domain.Models.PortIn.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Informing.Data.Domain.Mappers.PortIn;

public static class OrderMapper
{
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Include,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
            {
                OverrideSpecifiedNames = false,
            }
        },
        Converters = new List<JsonConverter>
        {
            new StringEnumConverter()
        }
    };

    public static PortInOrder ToModel(this PortInOrderEntity entity)
    {
        try
        {
            var orderData = JsonConvert.DeserializeObject<PortInOrder>(
                value: entity.OrderData,
                settings: _serializerSettings
            ) ?? throw new PortInOrderDataInvalidFormatException(
                orderId: entity.OrderId,
                message: $"Invalid OrderData for PortIn order with id: {entity.OrderId}"
            );

            var stateCodeName = (OrderStateCode)entity.State;

            var orderState = new OrderState(
                Code: stateCodeName,
                Message: orderData.State.Message,
                StatusDate: orderData.State.StatusDate,
                Name: orderData.State.Name
            );

            var order = new PortInOrder(
                ID: entity.OrderId.ToString(),
                CdbProcesID: entity.CdbProcessId?.ToString(),
                Source: orderData.Source,
                DueDate: entity.DueDate,
                Comment: orderData.Comment,
                PortationNumbers: orderData.PortationNumbers,
                Donor: orderData.Donor,
                Recipient: orderData.Recipient,
                Person: orderData.Person,
                Company: orderData.Company,
                Government: orderData.Government,
                Individual: orderData.Individual,
                Contract: orderData.Contract,
                State: orderState
            );

            return order;
        }
        catch (Exception ex)
        {
            throw new PortInOrderDataInvalidFormatException(
                orderId: entity.OrderId,
                message: $"Exception occurred during data convertion for PortIn order with id: {entity.OrderId}",
                innerException: ex
            );
        }
    }
}