using OpenTelemetry;
using OpenTelemetry.Logs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtelConsoleExporter
{
    internal class CustomConsoleExporter : BaseExporter<LogRecord>
    {
        private readonly string name;
        public CustomConsoleExporter(string name = "MyExporter")
        {
            this.name = name;
        }
        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            // SuppressInstrumentationScope should be used to prevent exporter
            // code from generating telemetry and causing live-loop.
            using var scope = SuppressInstrumentationScope.Begin();

            var sb = new StringBuilder();
            foreach (var record in batch)
            {
                IReadOnlyList<KeyValuePair<string, object>> stateDictionary = record.State as IReadOnlyList<KeyValuePair<string, object>>;
                string stateDict=String.Empty; 
                if (stateDictionary != null)
                {
                    stateDict = String.Join(",", stateDictionary.Where ( kvp => kvp.Key != "{OriginalFormat}"));
                }

                Console.WriteLine($"{record.Timestamp.ToString("O")}|{record.CategoryName}|{record.EventId}|{record.TraceId}|{record.SpanId}|{record.FormattedMessage}|{stateDict}");

            }

            return ExportResult.Success;
        }

        protected override bool OnShutdown(int timeoutMilliseconds)
        {
            Console.WriteLine($"{this.name}.OnShutdown(timeoutMilliseconds={timeoutMilliseconds})");
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            Console.WriteLine($"{this.name}.Dispose({disposing})");
        }
    }
}
