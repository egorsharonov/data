using Informing.Data.Domain.Models.PortIn.Common;
using Informing.Data.Domain.Models.Rtm;

namespace Informing.Data.Domain.Mappers.PortIn;

public static class OrderStateCodeMapper
{
    public static RtmPortInEventType ToRtmEventType(this OrderStateCode stateCode)
    {
        // TODO: AV - уточнить мапинг c учетом Code_reject, Date_of_action
        return stateCode switch
        {
            OrderStateCode.SentCdb => RtmPortInEventType.NP_Create,
            OrderStateCode.ArbitrationPending => RtmPortInEventType.NP_Verification_Exec,
            OrderStateCode.DonorVerification => RtmPortInEventType.NP_Verification_Complete,
            OrderStateCode.DebtChecking => RtmPortInEventType.NP_Donor_Confirm,
            OrderStateCode.PortationComplete => RtmPortInEventType.NP_Recipient_Activate_Confirm,
            OrderStateCode.Canceled => RtmPortInEventType.NP_Cancel,
            // OrderStateCode.CdbRejected => default,
            // OrderStateCode.DonorRejected => default
            // OrderStateCode.DebtCollection => RtmPortInEventType.NP_Donor_Info,
            _ => throw new ArgumentException($"Invalid not-convertable to RTM EventType order state code: {stateCode}")
        };
    }
}