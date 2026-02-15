namespace Informing.Data.Domain.Models.Rtm;

/// <summary>
/// Тип события PortIn в формате Siebel/RTM для RTM
/// </summary>
public enum RtmPortInEventType
{
    NP_Create = 0,
    NP_Promo_Period_Activated,
    NP_Verification_Exec,
    NP_Verification_Complete,
    NP_Donor_Confirm,
    NP_Donor_Info,
    NP_OneDayBeforePort,
    NP_Recipient_Activate_Confirm,
    NP_Change,
    NP_Change_Confirm,
    NP_Change_Reject,
    NP_Cancel,
    NP_Cancel_Confirm,
    NP_Cancel_Reject,
    NP_Reject_PDN,
    NP_Reject_Block,
    NP_Reject_DocInvalid,
    NP_Reject_7101,
    NP_Reject_DateInvalid,
    NP_Reject_7011,
    NP_Reject_7122,
    NP_Reject_2015,
    NP_Reject_3013,
    NP_Reject_7113,
    NP_Reject_3048,
    NP_Reject_Acceptor,
    NP_Reject_MGTS,
    NP_Retry_Notify
}