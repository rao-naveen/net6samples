using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Logging;

namespace S3StreamUnzip
{
    public static class Helper
    {
        /// <summary>
        /// Gets S3 Client for Minio
        /// </summary>
        /// <param name="serviceUrl">Minio Service Url . Ex: http://127.0.0.1:9000</param>
        /// <param name="accessKey">Minio access key, ex: minioadmin</param>
        /// <param name="secretKey">Minio secret key, ex: minioadmin </param>
        /// <returns></returns>
        public static IAmazonS3 GetS3ClientForMinio(string serviceUrl, string accessKey, string secretKey)
        {
            AmazonS3Config config = new AmazonS3Config() { ServiceURL = serviceUrl };
            AmazonS3Client s3Client = new AmazonS3Client(accessKey, secretKey, config);
            return s3Client;
        }
        /// <summary>
        /// Gets S3 Client using credentails & region using .aws profile 
        /// </summary>
        /// <returns></returns>
        public static IAmazonS3 GetS3ClientUsingAwsProfile()
        {
            AmazonS3Config config = new AmazonS3Config();
            AmazonS3Client s3Client = new AmazonS3Client(config);
            return s3Client;
        }

        public static Microsoft.Extensions.Logging.ILogger GetLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("S3StreamUnzip", LogLevel.Debug);
                builder.ClearProviders();
                builder.AddConsole();
            });
            var s3StreamLogger = loggerFactory.CreateLogger("S3StreamUnzip");
            return s3StreamLogger;
        }
    }
}
