using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GcLog;
using Microsoft.Extensions.Hosting;
using NewVoiceMedia.DotNetGrpcServiceExamples.GcLog;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.ClrCounters
{
    public class ClrEventsService : IHostedService
    {
        private EventPipeGcLog _gcLog;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _gcLog = EventPipeGcLog.GetLog(Process.GetCurrentProcess().Id);
            _gcLog.Start(new StreamWriter(Console.OpenStandardOutput()));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _gcLog.Stop();
            return Task.CompletedTask;
        }
    }
}
