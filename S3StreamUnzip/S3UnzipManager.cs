using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3StreamUnzip
{
    /// <summary>
    /// Unzips the given Zip S3 object without loading zip file into memory.
    /// Here is how it works,
    /// Uses .NET ZipArchive stream API with input pointing to Zip S3 Object Stream
    /// For each ZipEntry found in ZipArchive, 
    /// Creates a FileBufferingStream backed by temp file if size > 50 MB else in Memory
    /// Uses AWS Trasnsfer Utility to create new zipped object with given prefix
    /// </summary>
    /// <remarks>
    /// Inspired from https://github.com/nejckorasa/s3-stream-unzip
    /// </remarks>
    public class S3UnzipManager
    {
        static readonly int MB = 1024 * 1024;
        static readonly int MaxMemoryThreshodinBytes = 50 * MB;
        private readonly IAmazonS3 amazonS3;
        

        public S3UnzipManager(IAmazonS3 amazonS3)
        {
            this.amazonS3 = amazonS3;
        }

        public void Unzip(string bucketName,string inputObjectKey,string outputprefix)
        {
            // Get Zip Object from S3
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = inputObjectKey
            };
            using GetObjectResponse response = amazonS3.GetObjectAsync(request).GetAwaiter().GetResult();
            using Stream responseStream = response.ResponseStream;
            // Create ZipArchive from the inputStream of S3 Object
            using ZipArchive zipArchive = new ZipArchive(responseStream,ZipArchiveMode.Read);
            // for each enty in Zip
            foreach(var zipEntry in zipArchive.Entries)
            {
                if (zipEntry.Length == 0)
                {
                    // ignore directory entry
                    continue;
                }
                
                FileBufferingReadStream fileBufferingReadStream = MakeSeekableStream(zipEntry);

                using (fileBufferingReadStream)
                {
                    string outputObjectKey = $"{outputprefix}/{zipEntry.FullName}";
                    TransferUtility transferUtility = UploadStreamToS3(bucketName, zipEntry, outputObjectKey, fileBufferingReadStream);
                }
            }
        }

        private TransferUtility UploadStreamToS3(string bucketName, ZipArchiveEntry zipEntry, string outputObjectKey, FileBufferingReadStream fileBufferingReadStream)
        {
            TransferUtility transferUtility = new TransferUtility(amazonS3);
            TransferUtilityUploadRequest transferUtilityUploadRequest = new TransferUtilityUploadRequest();
            transferUtilityUploadRequest.BucketName = bucketName;
            transferUtilityUploadRequest.InputStream = fileBufferingReadStream;
            transferUtilityUploadRequest.Key = outputObjectKey;
            try
            {
                transferUtility.Upload(transferUtilityUploadRequest);
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Zip Entry upload failed {zipEntry.FullName}");
            }
            Console.WriteLine($"Zip Entry upload successfully {zipEntry.FullName}; Size {Bytes.GetReadableSize(zipEntry.Length)}");
            return transferUtility;
        }

        /// <summary>
        /// Creates FileBufferingStream from the ZipEntry.
        /// This stream will be backed by temporary file if length > 50 MB, else it will be backed by Memory
        /// If temp file is created, it will be disposed when the stream is disposed.
        /// </summary>
        /// <param name="zipEntry"></param>
        /// <returns></returns>
        private static FileBufferingReadStream MakeSeekableStream(ZipArchiveEntry zipEntry)
        {
            FileBufferingReadStream fileBufferingReadStream = default(FileBufferingReadStream);

            using (var zipEntryStream = zipEntry.Open())
            {
                fileBufferingReadStream = new FileBufferingReadStream(zipEntryStream, MaxMemoryThreshodinBytes, null, TempFileDir);
                fileBufferingReadStream.DrainAsync(CancellationToken.None).GetAwaiter().GetResult();
                fileBufferingReadStream.Seek(0, SeekOrigin.Begin);
                if (!string.IsNullOrEmpty(fileBufferingReadStream.TempFileName))
                {
                    var backup = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Debug]Temp file is created for {zipEntry.FullName} -> {fileBufferingReadStream.TempFileName} : {Bytes.GetReadableSize(zipEntry.Length)}");
                    Console.ForegroundColor = backup;
                }
            }

            return fileBufferingReadStream;
        }

        private static string TempFileDir()
        {
            return @"C:\dev\temp\fors3demo";//Path.GetTempPath();

        }
    }
}
