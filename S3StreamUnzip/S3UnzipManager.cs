using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <summary>
        /// using CSharpZipLib
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="inputObjectKey"></param>
        /// <param name="outputprefix"></param>
        public void Unzip2(string bucketName, string inputObjectKey, string outputprefix)
        {
            
             // Get Zip Object from S3
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = inputObjectKey
            };
            using GetObjectResponse response = amazonS3.GetObjectAsync(request).GetAwaiter().GetResult();
            //using (ZipInputStream s = new ZipInputStream()
            using Stream responseStream = response.ResponseStream;

            var stopWatch = Stopwatch.StartNew();
            // Create ZipArchive from the inputStream of S3 Object
            using ZipInputStream zipInputStream = new ZipInputStream(responseStream);
            ZipEntry theEntry;
            // for each enty in Zip
            while ((theEntry = zipInputStream.GetNextEntry()) != null)
            {
                Console.WriteLine($"Unzip file {theEntry.Name} of compression method {theEntry.CompressionMethod}");
                if (theEntry.IsDirectory)
                {
                    // ignore directory entry
                    Directory.CreateDirectory(Path.Combine(@"C:\dev\temp\fors3demo",theEntry.Name));
                    continue;
                }
                Stream stream = Stream.Null;

                
                FileBufferingReadStream fileBufferingReadStream = new FileBufferingReadStream(zipInputStream, MaxMemoryThreshodinBytes, null, TempFileDir);
                fileBufferingReadStream.DrainAsync(CancellationToken.None).GetAwaiter().GetResult();
                fileBufferingReadStream.Seek(0, SeekOrigin.Begin);
                stream = fileBufferingReadStream;
                // for debug
                if (!string.IsNullOrEmpty(fileBufferingReadStream.TempFileName))
                {
                    var backup = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Debug]Temp file is created for {theEntry.Name} -> {fileBufferingReadStream.TempFileName} : {Bytes.GetReadableSize(theEntry.Size)}");
                    Console.ForegroundColor = backup;
                }
                

                // for debug
                //using (fileBufferingReadStream)
                //{
                //    using var file = File.Create(Path.Combine(@"C:\dev\temp\fors3demo", theEntry.Name));
                //    fileBufferingReadStream.CopyTo(file);
                //}

                using (stream)
                {
                    string outputObjectKey = $"{outputprefix}/{theEntry.Name}";
                    TransferUtility transferUtility = UploadStreamToS3(bucketName, theEntry.Name, theEntry.Size, outputObjectKey, stream);
                }

            }
            stopWatch.Stop();
            Console.WriteLine($"Unzip & Upload Completed in {stopWatch.Elapsed.ToString()}");
        }
        /// <summary>
        ///  using Windows Builtin Zip Archive which loads everything to memory
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="inputObjectKey"></param>
        /// <param name="outputprefix"></param>
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
                    TransferUtility transferUtility = UploadStreamToS3(bucketName, zipEntry.FullName,zipEntry.Length, outputObjectKey, fileBufferingReadStream);
                }
            }
        }

        private TransferUtility UploadStreamToS3(string bucketName, String fileName,long size, string outputObjectKey, Stream stream)
        {
            TransferUtility transferUtility = new TransferUtility(amazonS3);
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
            return Path.GetTempPath();

        }
    }
}
