using System.Runtime.Serialization;

namespace Informing.Data.Domain.Models.PortIn.Common;

/// <summary>
/// TODO: Используются преобразования в численный эквивалент и опечатки в arbitration- и -waiting кодах до правок AV 29.01.2026
/// покуда не смигрированны статус коды в основном сервисе взаимодействия с БД portin-orders 
/// </summary>
public enum OrderStateCode
{
    [EnumMember(Value = "cancel-rejected")]
    CancelRejected = -51,

    [EnumMember(Value = "arbitation-timeout")]
    ArbitrationTimeout = -4,

    [EnumMember(Value = "donor-rejected")]
    DonorRejected = -3,

    [EnumMember(Value = "canceled")]
    Canceled = -2,

    [EnumMember(Value = "cdb-rejected")]
    CdbRejected = -1,

    [EnumMember(Value = "created")]
    Created = 0,

    [EnumMember(Value = "sent-cdb")]
    SentCdb = 1,

    [EnumMember(Value = "arbitration")]
    Arbitration = 2,

    [EnumMember(Value = "donor-verification")]
    DonorVerification = 3,

    [EnumMember(Value = "arbitation-pending")]
    ArbitrationPending = 4,

    [EnumMember(Value = "debt-checking")]
    DebtChecking = 5,

    [EnumMember(Value = "debt-collection")]
    DebtCollection = 6,

    [EnumMember(Value = "portation-waitng")]
    PortationWaiting = 7,

    [EnumMember(Value = "portation-due")]
    PortationDue = 8,

    [EnumMember(Value = "portation-ready")]
    PortationReady = 9,

    [EnumMember(Value = "portation-exec")]
    PortationExec = 20,

    [EnumMember(Value = "portation-complete")]
    PortationComplete = 21,

    [EnumMember(Value = "closed")]
    Closed = 22,

    [EnumMember(Value = "duedate-changed")]
    DuedateChanged = 23,

    [EnumMember(Value = "cancel-request")]
    CancelRequest = 50,

    [EnumMember(Value = "cancel-confirmed")]
    CancelConfirmed = 51
}