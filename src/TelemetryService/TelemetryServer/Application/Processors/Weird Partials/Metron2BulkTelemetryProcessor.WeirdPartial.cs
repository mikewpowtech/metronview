using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using TelemetryServer.Application;
using TelemetryServer.Enums;
using TelemetryServer.Telemetry.Reactors;

namespace TelemetryServer.Telemetry;

// TODO make this a property of the telemetry processor
// Why is this so complicated???
partial class Metron2BulkTelemetryProcessor
{
    private static int transactionId = 0;

    private Activity currentActivity;
    
    private void UnexpectedError(Exception exception)
    {
        currentActivity?.SetStatus(ActivityStatusCode.Error, exception.Message);
    }

    private Activity InitialiseActivity()
    {
        try
        {
            var txId = Interlocked.Increment(ref transactionId).ToString("X8");
            var clientEndpoint = tcpClient.Client.RemoteEndPoint?.ToString();
            currentActivity = ActivitySources.Metron2Receive.StartActivity(kind: ActivityKind.Server,  tags: new List<KeyValuePair<string, object>>
            {
                new("client.endpoint", clientEndpoint)
            });
            using var loggerScope = _logger.BeginScope(
                "TransactionId {TransactionId} ClientIp {ClientIp}",
                txId,
                clientEndpoint);
            return currentActivity;
        }
        catch
        {
            currentActivity?.Dispose();
            throw;
        }
    }

    private void GotManufacturerId()
    {
        currentActivity?.SetTag("user.id", manufacturerId);
    }

    private Activity StartingToInterpretTransaction()
    {
        return ActivitySources.Metron2Transaction.StartActivity();
    }

    private void ProtocolErrorHandler(BulkProtocolHostErrorCode errorCode)
    {
        currentActivity.SetStatus(ActivityStatusCode.Error, $"Protocol error: {errorCode}");
    }

    private void IllegalStateHandler(Metron2BulkTelemetryProcessorState illegalState)
    {
        currentActivity.SetStatus(ActivityStatusCode.Error, $"Illegal State: {illegalState}");
    }
}