using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Informing.Data.Domain.Models.Rtm;

public class RtmMessage
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm:sszzz";
    private const string PortTimeFormat = "HH:mm";
    private static readonly string DateTimeFormat = $"{DateFormat}T{TimeFormat}";
    private static readonly TimeZoneInfo MoscowTimeZone = ResolveMoscowTimeZone();

    private static TimeZoneInfo ResolveMoscowTimeZone()
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById("Russian Standard Time", out var mscTimeZone) == false)
        {
            TimeZoneInfo.TryFindSystemTimeZoneById("Europe/Moscow", out mscTimeZone);
        }

        return mscTimeZone ?? throw new ArgumentException("Unable to find Moscow time zone for convertion.");
    }

    [JsonIgnore]
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        DateFormatString = DateTimeFormat,
        NullValueHandling = NullValueHandling.Ignore,
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


    // Required
    [JsonIgnore]
    public required string Key {get; init;}

    [JsonProperty("Type")]
    public required RtmPortInEventType Type { get; init; }

    private readonly DateTimeOffset _dateEvent;
    [JsonProperty("DateEvent")]
    public required DateTimeOffset DateEvent
    {
        get => _dateEvent;
        init => _dateEvent = TimeZoneInfo.ConvertTime(value, MoscowTimeZone);
    }

    [JsonProperty("PortIN_Number")]
    public required string PortInNumber { get; init; }

    // Optional
    [JsonProperty("URL")]
    public string? URL { get; set; }

    [JsonIgnore]
    public DateTimeOffset? PreOrderDate { get; set; } = null;
    [JsonProperty("PreOrder_DATE")]
    private string? _preOrderDate => PreOrderDate?.ToString(DateFormat);

    [JsonProperty("CustomerLastName")]
    public string? CustomerLastName { get; set; } = null;

    [JsonProperty("CustomerFirstName")]
    public string? CustomerFirstName { get; set; } = null;

    [JsonProperty("CustomerParentName")]
    public string? CustomerParentName { get; set; } = null;

    [JsonProperty("PortIN_Temporary")]
    public string? PortInTemporary { get; set; }

    private DateTimeOffset? _portDateTime;

    [JsonProperty("Port_DateTime")]
    public DateTimeOffset? PortDateTime
    {
        get => _portDateTime;
        set => _portDateTime = value.HasValue
                ? TimeZoneInfo.ConvertTime(value.Value, MoscowTimeZone)
                : null;
    }


    [JsonProperty("Port_Date")]
    private string? _portDate => PortDateTime?.ToString(DateFormat);

    [JsonProperty("Port_Time")]
    private string? _portTime => PortDateTime?.ToString(PortTimeFormat);

    [JsonIgnore]
    public DateTimeOffset? DateOfAction { get; set; } = null;
    [JsonProperty("Date_of_action")]
    private string? _dateOfAction => DateOfAction?.ToString(DateFormat);

    [JsonProperty("Code_reject")]
    public int? RejectCode { get; set; } = null;

    [JsonProperty("Options")]
    public RtmOptionalType? Options { get; set; } = null;

    public string Serialize()
    {
        return JsonConvert.SerializeObject(
                    value: this,
                    settings: _serializerSettings
                );
    }

}