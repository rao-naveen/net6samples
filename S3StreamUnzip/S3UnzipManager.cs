using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.WebUtilities;
using SharpCompress.Readers;
using System.Diagnostics;

namespace S3StreamUnzip
{
    /// <summary>
    /// Unzips the given Zip S3 object without loading zip file into memory.
    /// Here is how it works,
    /// Uses .NET CSharpziplib/sharpcompress stream API with input pointing to Zip S3 Object Stream
    /// For each ZipEntry found in Zip, 
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
        /// <summary>
        /// using CSharpZipLib
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="inputObjectKey"></param>
        /// <param name="outputprefix"></param>
        public void UnzipUsingCSharpziplib(string bucketName, string inputObjectKey, string outputprefix)
        {
            
             // Get Zip Object from S3
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = inputObjectKey
            };
            using GetObjectResponse response = amazonS3.GetObjectAsync(request).GetAwaiter().GetResult();
            using Stream responseStream = response.ResponseStream;

            var stopWatch = Stopwatch.StartNew();
            // Create ZipInputStream from the inputStream of S3 Object
            using ZipInputStream zipInputStream = new ZipInputStream(responseStream);
            ZipEntry theEntry;
            // for each enty in Zip
            while ((theEntry = zipInputStream.GetNextEntry()) != null)
            {
                Console.WriteLine($"Unzip file {theEntry.Name} of compression method {theEntry.CompressionMethod}");
                if (theEntry.IsDirectory)
                {
                    // ignore directory entry
                    continue;
                }
                Stream stream = Stream.Null;

                
                FileBufferingReadStream fileBufferingReadStream = new FileBufferingReadStream(zipInputStream, MaxMemoryThreshodinBytes, null, TempFileDir);
                fileBufferingReadStream.DrainAsync(CancellationToken.None).GetAwaiter().GetResult();
                fileBufferingReadStream.Seek(0, SeekOrigin.Begin);
                stream = fileBufferingReadStream;
                // for debug print if it is file backed
                if (!string.IsNullOrEmpty(fileBufferingReadStream.TempFileName))
                {
                    var backup = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Debug]Temp file is created for {theEntry.Name} -> {fileBufferingReadStream.TempFileName} : {Bytes.GetReadableSize(theEntry.Size)}");
                    Console.ForegroundColor = backup;
                }

                using (stream)
                {
                    string outputObjectKey = $"{outputprefix}/{theEntry.Name}";
                    UploadStreamToS3(bucketName, theEntry.Name, theEntry.Size, outputObjectKey, stream);
                }

            }
            stopWatch.Stop();
            Console.WriteLine($"Unzip & Upload Completed in {stopWatch.Elapsed.ToString()}");
        }
        
        /// <summary>
        /// using SharpCompress API 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="inputObjectKey"></param>
        /// <param name="outputprefix"></param>
        public void UnzipUsingSharpCompress(string bucketName, string inputObjectKey, string outputprefix)
        {

            // Get Zip Object from S3
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = inputObjectKey
            };
            using GetObjectResponse response = amazonS3.GetObjectAsync(request).GetAwaiter().GetResult();
            using Stream responseStream = response.ResponseStream;
            var stopWatch = Stopwatch.StartNew();

            using (var reader = ReaderFactory.Open(responseStream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.IsDirectory)
                    {
                        continue;
                    }

                    using var entryStream = reader.OpenEntryStream();
                    
                    Console.WriteLine($"Unzip file {reader.Entry.Key} of compression method {reader.Entry.CompressionType}");

                    Stream stream = Stream.Null;


                    FileBufferingReadStream fileBufferingReadStream = new FileBufferingReadStream(entryStream, MaxMemoryThreshodinBytes, null, TempFileDir);
                    fileBufferingReadStream.DrainAsync(CancellationToken.None).GetAwaiter().GetResult();
                    fileBufferingReadStream.Seek(0, SeekOrigin.Begin);
                    stream = fileBufferingReadStream;

                    // for debug print if it is file backed
                    if (!string.IsNullOrEmpty(fileBufferingReadStream.TempFileName))
                    {
                        var backup = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[Debug]Temp file is created for {reader.Entry.Key} -> {fileBufferingReadStream.TempFileName} : {Bytes.GetReadableSize(reader.Entry.Size)}");
                        Console.ForegroundColor = backup;
                    }

                    using (stream)
                    {
                        string outputObjectKey = $"{outputprefix}/{reader.Entry.Key}";
                        UploadStreamToS3(bucketName, reader.Entry.Key, reader.Entry.Size, outputObjectKey, stream);
                    }
                }
            }

            stopWatch.Stop();
            Console.WriteLine($"Unzip & Upload Completed in {stopWatch.Elapsed.ToString()}");
        }
        /// <summary>
        /// uploads given stream to S3 using TransferUtility
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="fileName"></param>
        /// <param name="size"></param>
        /// <param name="outputObjectKey"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private void UploadStreamToS3(string bucketName, String fileName,long size, string outputObjectKey, Stream stream)
        {
            using TransferUtility transferUtility = new TransferUtility(amazonS3);
            TransferUtilityUploadRequest transferUtilityUploadRequest = new TransferUtilityUploadRequest();
            transferUtilityUploadRequest.BucketName = bucketName;
            transferUtilityUploadRequest.InputStream = stream;
            transferUtilityUploadRequest.Key = outputObjectKey;
            try
            {
                transferUtility.Upload(transferUtilityUploadRequest);
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Zip Entry upload failed {fileName}, Exception {exp}");
            }
            Console.WriteLine($"Zip Entry upload successfully {fileName}; Size {Bytes.GetReadableSize(size)}");
            return;
        }

        private static string TempFileDir()
        {
            return Path.GetTempPath();

        }
    }
}
