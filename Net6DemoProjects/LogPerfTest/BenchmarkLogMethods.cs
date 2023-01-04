using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogPerfTest
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class BenchmarkLogMethods
    {
        private static readonly ILogger Logger;
        private static readonly Random Random;

        private int Data => Random.Next();

        static BenchmarkLogMethods()
        {
            Logger = new LoggerFactory().CreateLogger("Logger");
            Random = new Random();
        }

        [Benchmark]
        public void Templated()
        {
            Logger.LogInformation("data: {Data}", Data);
        }

        [Benchmark]
        public void Interpolated()
        {
            Logger.LogInformation($"data: {Data}");
        }

        [Benchmark]
        public void Formatted()
        {
            Logger.LogInformation(String.Format("data: {0}", Data));
        }
    }
}
