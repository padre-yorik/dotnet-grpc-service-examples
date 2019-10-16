using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ClrCounters;
using GcLog;
using NewVoiceMedia.StatsD;
using NLog;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.GcLog
{
    public class EventPipeGcLog : GcLogBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // TODO: don't forget to update the header if you are adding more columns 
        private const string Header =
            "StartRelativeMSec,Number,Generation,Type,Reason,IsCompacting,SuspensionDurationInMilliSeconds,PauseDurationInMilliSeconds,FinalPauseDurationInMilliSeconds,Gen0Size,Gen1Size,Gen2Size,LOHSize,ObjGen0Before,ObjGen1Before,ObjGen2Before,ObjLOHBefore,ObjGen0After,ObjGen1After,ObjGen2After,ObjLOHAfter";

        private int _pid;
        private GcEventListener _listener;
        private StringBuilder _line = new StringBuilder(2048);

        private EventPipeGcLog(int PID)
        {
            _pid = PID;
        }

        public static EventPipeGcLog GetLog(int pid)
        {
            return new EventPipeGcLog(pid);
        }


        protected override void OnStart()
        {
            if (_listener != null)
                throw new InvalidOperationException("Already started");

            _listener = new GcEventListener();

            _listener.GcEvents += OnGc;
            //WriteLine(Header);
        }

        protected override void OnStop()
        {
            if (_listener == null)
                throw new InvalidOperationException("Can't stop if not started");

            _listener.Stop();
            _listener = null;
        }

        private void OnGc(object sender, GarbageCollectionArgs e)
        {
            /*_line.Clear();
            _line.AppendFormat("{0},", e.StartRelativeMSec.ToString());
            _line.AppendFormat("{0},", e.Number.ToString());
            _line.AppendFormat("{0},", e.Generation.ToString());
            _line.AppendFormat("{0},", e.Type);
            _line.AppendFormat("{0},", e.Reason);
            _line.AppendFormat("{0},", e.IsCompacting.ToString());
            _line.AppendFormat("{0},", e.SuspensionDuration.ToString());
            _line.AppendFormat("{0},", e.PauseDuration.ToString());
            _line.AppendFormat("{0},", e.BGCFinalPauseDuration.ToString());
            _line.AppendFormat("{0},", e.Gen0Size.ToString());
            _line.AppendFormat("{0},", e.Gen1Size.ToString());
            _line.AppendFormat("{0},", e.Gen2Size.ToString());
            _line.AppendFormat("{0},", e.LOHSize.ToString());
            _line.AppendFormat("{0},", e.ObjSizeBefore[0].ToString());
            _line.AppendFormat("{0},", e.ObjSizeBefore[1].ToString());
            _line.AppendFormat("{0},", e.ObjSizeBefore[2].ToString());
            _line.AppendFormat("{0},", e.ObjSizeBefore[3].ToString());
            _line.AppendFormat("{0},", e.ObjSizeAfter[0].ToString());
            _line.AppendFormat("{0},", e.ObjSizeAfter[1].ToString());
            _line.AppendFormat("{0},", e.ObjSizeAfter[2].ToString());
            _line.AppendFormat("{0}", e.ObjSizeAfter[3].ToString());

            WriteLine(_line.ToString());*/

            _logger.Info($"ClrEvents - GarbageCollection: [{e.ProcessId,7}] gen{e.Generation} #{e.Number} suspension={e.SuspensionDuration:0.00}ms | compacting={e.IsCompacting} ({e.Type} - {e.Reason}) " +
                              $"gen0: {e.Gen0Size / 1024 / 1024}Mb  gen1: {e.Gen1Size / 1024 / 1024}Mb  gen2: {e.Gen2Size / 1024 / 1024}Mb  loh: {e.LOHSize / 1024 / 1024}Mb");
            StatsDClient.IncrementCounter("GarbageCollection", new { e.Generation, e.IsCompacting, e.Type, e.Reason });
            StatsDClient.RecordMetric("HeapSize", e.Gen0Size, new { Type = "Gen0" });
            StatsDClient.RecordMetric("HeapSize", e.Gen1Size, new { Type = "Gen1" });
            StatsDClient.RecordMetric("HeapSize", e.Gen2Size, new { Type = "Gen2" });
            StatsDClient.RecordMetric("HeapSize", e.LOHSize, new { Type = "LOH" });
            var memoryInfo = GC.GetGCMemoryInfo();
            StatsDClient.RecordMetric("MemoryInfo", GC.GetTotalMemory(false), new { Type = "TotalMemory" });
            StatsDClient.RecordMetric("MemoryInfo", memoryInfo.HighMemoryLoadThresholdBytes, new { Type = "HighMemoryLoadThresholdBytes" });
            StatsDClient.RecordMetric("MemoryInfo", memoryInfo.TotalAvailableMemoryBytes, new { Type = "TotalAvailableMemoryBytes" });
            StatsDClient.RecordMetric("MemoryInfo", memoryInfo.FragmentedBytes, new { Type = "FragmentedBytes" });
            StatsDClient.RecordMetric("MemoryInfo", memoryInfo.HeapSizeBytes, new { Type = "HeapSizeBytes" });
            StatsDClient.RecordMetric("MemoryInfo", memoryInfo.MemoryLoadBytes, new { Type = "MemoryLoadBytes" });
            var process = Process.GetCurrentProcess();
            StatsDClient.RecordMetric("MemoryInfo", process.WorkingSet64, new { Type = "WorkingSet64" });
            StatsDClient.RecordMetric("MemoryInfo", process.VirtualMemorySize64, new { Type = "VirtualMemorySize64" });
            if (File.Exists("/proc/self/status"))
            {
                _logger.Info("/proc/self/status: " + File.ReadAllText("/proc/self/status"));
            }
            _logger.Info("ps aux".Bash());
        }
    }

    public static class ShellHelper
    {
        public static string Bash(this string cmd)
        {
            if (!File.Exists("/bin/bash"))
            {
                return $"/bin/bash not available to run command: {cmd}";
            }

            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}
