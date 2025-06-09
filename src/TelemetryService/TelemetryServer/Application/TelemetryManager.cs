using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TelemetryServer.Application.Listeners;

namespace TelemetryServer.Application
{
    /// <summary>
    /// The telemetry Manager holds the current state of the service, plus references to anything else required.
    /// </summary>
    public class TelemetryManager(IEnumerable<IListener> listeners) : BackgroundService
    {
        private readonly IEnumerable<IListener> _listeners = listeners;

        private static readonly ISet<Task> TelemetryProcessingTasks;

        static TelemetryManager()
        {
            // We could initialise the connection string when it was first used, but here is simpler as there's no need to deal with threading.
            TelemetryProcessingTasks = new HashSet<Task>();
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            Task.WhenAll(_listeners.Select(l => l.StartListeningAsync(stoppingToken)));


        public override void Dispose()
        {
            // We can't get new telemetryProcessorTasks as the listeners have closed. Wait for all present telemetryProcessorTasks to complete.
            lock (TelemetryProcessingTasks)
            {
                Task.WaitAll(TelemetryProcessingTasks.ToArray());
            }

            base.Dispose();
        }

        /// <summary>
        /// The telemetryProcessorTask has just been started; keep hold of it to prevent garbage collection and allow enumeration.
        /// </summary>
        /// <param name="telemetryProcessorTask"></param>
        internal static Task NoteNewTelemetryProcessorTask(Task telemetryProcessorTask)
        {
            lock (TelemetryProcessingTasks)
            {
                TelemetryProcessingTasks.Add(telemetryProcessorTask);
            }
            return telemetryProcessorTask.ContinueWith((task) => NoteReactorTaskCompleted(task));
        }

        /// <summary>
        /// The telemetryProcessorTask has closed down; remove it from our telemetryProcessorTasks list to allow garbage collection and prevent enumeration.
        /// </summary>
        /// <param name="telemetryProcessorTask"></param>
        private static void NoteReactorTaskCompleted(Task telemetryProcessorTask)
        {
            lock (TelemetryProcessingTasks)
            {
                TelemetryProcessingTasks.Remove(telemetryProcessorTask);
            }
        }
    }
}
