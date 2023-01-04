using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MELConsoleLogIntegraton
{
    public class TenantInformation
    {
        public string Name { get; set; }    
        public string Id { get; set; }
    }
    public record class UsageInformation(string StudiesCount);
    public static partial class LoggingExtension
    {
        private static List<KeyValuePair<string, object>> StdServiceLogProperties = new List<KeyValuePair<string, object>>()
        {
            new KeyValuePair<string, object>("LogType","Service"),
        };
        private static List<KeyValuePair<string, object>> StdUsageLogProperties = new List<KeyValuePair<string, object>>()
        {
            new KeyValuePair<string, object>("LogType","Usage"),
        };


        [LoggerMessage(EventId = 1,Level = LogLevel.Critical,Message = "TenantInfo {TenantId} {TenantName}")]
        public static partial void LogTenant(this ILogger logger, string TenantId, string TenantName);
        public static void LogTenantInformation(this ILogger logger, TenantInformation usageInfo)
        {
            using (logger.BeginScope(StdServiceLogProperties))
            {
                logger.LogTenant(usageInfo.Id, usageInfo.Name);
            }


        }

        [LoggerMessage(EventId = 1,Level = LogLevel.Critical,Message = "LogUsage No: Studies{StudiesCount}")]
        public static partial void LogUsage(this ILogger logger, string StudiesCount );
        public static void LogUsageInformation(this ILogger logger, UsageInformation usageInfo)
        {
            using (logger.BeginScope(StdUsageLogProperties))
            {
                logger.LogUsage(usageInfo.StudiesCount);
            }
        }

        //public static void LogService(this ILogger logger,string usageInfo)
        //{
        //    using (logger.BeginScope(StdServiceLogProperties))
        //    {
        //        logger.LogServiceUsingScope(usageInfo);
        //    }


        //}


        //[LoggerMessage(
        //EventId = 1,
        //Level = LogLevel.Critical,
        //Message = "UsageInformation {Category} {AccessCount}")]
        //public static partial void LogServiceUsage(this ILogger logger,string category, string accessCount);


        //[LoggerMessage(
        //EventId = 2,
        //Level = LogLevel.Critical,
        //Message = "UsageInformation {AccessCount}")]
        //public static partial void LogServiceUsingScope(this ILogger logger, string accessCount);


    }
}
