using System.Runtime.Serialization;

namespace Informing.Data.Domain.Models.Rtm;

/// <summary>
/// Например, "0" или "1".
/// Технический параметр для определения альтернативных шаблонов SMS
/// или для использования в других аспектах SMS-информирования.
/// </summary>
public enum RtmOptionalType
{
    [EnumMember(Value = "0")]
    Disabled = 0,
    [EnumMember(Value = "1")]
    Enabled = 1
}