using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MELConsoleLogIntegraton
{
    public static class ConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddCustomFormatter(
            this ILoggingBuilder builder,
            Action<ConsoleFormatterOptions> configure)
        {
            return builder.AddConsole(options => options.FormatterName = "democustomformat")
                .AddConsoleFormatter<CustomFormatter, ConsoleFormatterOptions>(configure);
        }
    }

    public class CustomFormatter : ConsoleFormatter, IDisposable
    {
        private bool disposedValue;
        private static List<KeyValuePair<string, object>> StdServiceLogProperties = new List<KeyValuePair<string, object>>()
        {
            new KeyValuePair<string, object>("LogType","Service"),
        } ;
        private static List<KeyValuePair<string, object>> StdUsageLogProperties = new List<KeyValuePair<string, object>>()
        {
            new KeyValuePair<string, object>("LogType","Usage"),
        };
        private static IEnumerable<KeyValuePair<string, object>> EmptyProperties =
            Enumerable.Empty<KeyValuePair<string, object>>();
        private readonly IOptionsMonitor<ConsoleFormatterOptions> options;

        public CustomFormatter(IOptionsMonitor<ConsoleFormatterOptions> options) : base("democustomformat")
        {
            this.options = options;
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            // this check is already done in ConsoleLogger implementation.
            //if (!IsEnabled(logLevel)) return;

            var description = logEntry.Formatter!(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && description == null)
            {
                return;
            }
            string timestampFormat = options.CurrentValue.TimestampFormat;
            string timestampStamp = String.Empty;
            // respect user provided time stamp
            if (timestampFormat != null)
            {
                timestampStamp = (options.CurrentValue.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now).ToString(timestampFormat);
            }

            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["TimeStamp"] = timestampStamp;
            dict["LogLevel"] = GetLogLevelString(logEntry.LogLevel);
            dict["EventId"] = Convert.ToString(logEntry.EventId.Id);
            dict["Category"] = logEntry.Category;
            dict["TraceId"] = Activity.Current?.TraceId.ToString() ?? "traceId";
            dict["SpanId"] = Activity.Current?.SpanId.ToString() ?? "spanId";
            ///dict["description"] = description;



            /// from https://github.com/elmahio/Elmah.Io.Extensions.Logging/blob/main/src/Elmah.Io.Extensions.Logging/ElmahIoLogger.cs#L94
            /// https://github.com/Xpl0itR/Extensions.Logging/blob/master/Extensions.Logging.Console.BetterFormatter/BetterConsoleFormatter.cs
            /// https://github.com/GoogleCloudPlatform/gce-license-tracker/blob/eda678999f4a6976730ea4483bc26bb4f32a0027/sources/Google.Solutions.LcenseTracker/Util/JsonConsoleFormatter.cs#L30
            /// https://github.com/serilog/serilog-formatting-compact/blob/dev/src/Serilog.Formatting.Compact/Formatting/Compact/RenderedCompactJsonFormatter.cs
            /// https://github.com/jskeet/google-cloud-dotnet/tree/main/apis/Google.Cloud.Logging.Console/Google.Cloud.Logging.Console
            var properties = EmptyProperties;
            if (logEntry.EventId.Id > 2000 && logEntry.EventId.Id < 3000)
            {
                // add service category to key/value props
                properties = properties.Concat(StdServiceLogProperties);
            }
            if (logEntry.EventId.Id > 3000 && logEntry.EventId.Id < 4000)
            {
                // add usage category to key/value props
                properties = properties.Concat(StdUsageLogProperties);
            }
            if (logEntry.State is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                // IEnumerable.Concat returns  new list which concat of source & dest
                properties = properties.Concat(stateProperties);
            }
            if (options.CurrentValue.IncludeScopes)
            {
                scopeProvider?.ForEachScope<object>((scope, _) =>
                {
                    if (scope == null) return;
                    if (scope is IEnumerable<KeyValuePair<string, object>> scopeProperties)
                    {
                        properties = properties.Concat(scopeProperties);
                    }
                    // Strings and primitive types are ignored for now
                    else if (!(scope is string) && !scope.GetType().IsPrimitive)
                    {
                        properties = properties.Concat(scope
                            .GetType()
                            // Only fetch public instance properties declared directly on the scope object. In time we
                            // may want to support complex inheritance structures here.
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                            // Only look at non-indexer properties which we can read from
                            .Where(p => p.CanRead && !string.IsNullOrWhiteSpace(p.Name) && p.Name != "Item" && p.GetIndexParameters().Length == 0)
                            .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(scope)?.ToString())));
                    }
                }, null);
            }

            

            string keyp = string.Join(" ", properties.Where(kvp => kvp.Key != "{OriginalFormat}").Select( kvp => $"{kvp.Key}=\"{kvp.Value}\"")); ;
            dict["|"] = $"\"{description}\" {keyp}";

            textWriter.WriteLine(String.Join('|', dict.Values));
        }
        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                _ => throw new ArgumentOutOfRangeException("logLevel"),
            };
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CustomFormatter()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

       
    }
}
