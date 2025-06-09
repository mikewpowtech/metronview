using System;

namespace TelemetryServer.Telemetry.Reactors;

/// <summary>
/// Allowed states for a Metron2BulkReactor's finite state machine.
/// </summary>
[Flags]
public enum Metron2BulkTelemetryProcessorState
{
    // Atomic flags
    ConnectionErrorIsExpected = 0x01,
    AnotherTransactionIsExpected = 0x02,
    // Composite flags
    ExpectingTransaction = 0x100,
    ExpectingAck = 0x200,
    ExpectingAckThenTransaction = ExpectingAck | AnotherTransactionIsExpected,
    Killing = 0x10 | ConnectionErrorIsExpected,
    WaitingAfterKill = 0x20 | ConnectionErrorIsExpected,
    Closed = 0x30 | ConnectionErrorIsExpected
}