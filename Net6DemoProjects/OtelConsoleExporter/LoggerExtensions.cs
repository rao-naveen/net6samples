using OpenTelemetry;
using OpenTelemetry.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtelConsoleExporter
{
    public static class LoggerExtensions
    {
        public static OpenTelemetryLoggerOptions AddMyExporter(this OpenTelemetryLoggerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return options.AddProcessor(new BatchLogRecordExportProcessor(new CustomConsoleExporter()));
        }
    }
}
