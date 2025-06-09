namespace TelemetryServer.Enums;

/// <summary>
/// Error codes.  The values represent the value returned to the unit.
/// Ensure this always matches Powelectrics' error definitions.
/// </summary>
public enum BulkProtocolHostErrorCode
{
    HeaderTimeout = 0,
    DeviceTypeNotRecognised = 1,
    SerialNumberNotRecognised = 2,
    InvalidSecurityToken = 3,
    IncorrectDateTimeFormat = 4,
    IncorrectParametersHeader = 9,
    UnexpectedErrorBeforeIdentification = 10,
    UnexpectedErrorBeforeAuthentication = 11,
    IncorrectFormatOrContent = 12
}