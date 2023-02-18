using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.WebUtilities;
using SharpCompress.Readers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;

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
        private readonly Microsoft.Extensions.Logging.ILogger _logger;


        public S3UnzipManager(IAmazonS3 amazonS3, Microsoft.Extensions.Logging.ILogger logger)
        {
            this.amazonS3 = amazonS3;
            _logger = logger;
        }

        public async Task<List<string>> DirectoryUnzipUsingCSharpziplib(string inputBucketName, string inputObjectKey,
            string outputBucketName, string? outputprefix, string downloadDir)
        {
            var downloadFile = Path.Combine(downloadDir, inputObjectKey);
            if (Path.GetExtension(downloadFile) != ".zip")
            {
                downloadFile += ".zip";
            }
            var extractedDir = Path.Combine(downloadDir, Path.GetFileNameWithoutExtension(inputObjectKey));
            
            await DownloadFileAsync(inputBucketName, inputObjectKey, downloadFile).ConfigureAwait(false);
            ExtractZip(extractedDir, downloadFile);
            await UploadDirectoryAsync(outputBucketName, outputprefix, extractedDir).ConfigureAwait(false);

            var uploadedEntries = Cleanup(extractedDir, downloadFile);
            return uploadedEntries;
        }

        /// <summary>
        /// Cleans up the zip file and extracted dir
        /// </summary>
        /// <param name="extractedDir"></param>
        /// <param name="downloadFile"></param>
        /// <returns></returns>
        private static List<string> Cleanup(string extractedDir, string downloadFile)
        {
            List<string> uploadedEntries = new List<string>();
            var files = Directory.EnumerateFiles(extractedDir, "*.*", SearchOption.AllDirectories);
            uploadedEntries.AddRange(files);
            Directory.Delete(extractedDir, true);
            File.Delete(downloadFile);
            return uploadedEntries;
        }

        /// <summary>
        /// Uploads the contents of a directory to output S3 bucket
        /// </summary>
        /// <param name="outputBucketName">Name of output bucket</param>
        /// <param name="outputprefix">Prefix to use for output objects</param>
        /// <param name="extractedDir">Directory to upload</param>
        /// <returns></returns>
        private async Task UploadDirectoryAsync(string outputBucketName, string? outputprefix, string extractedDir)
        {
            Stopwatch sw = Stopwatch.StartNew();
            TransferUtilityUploadDirectoryRequest uploadRequest = new TransferUtilityUploadDirectoryRequest();
            uploadRequest.BucketName = outputBucketName;
            uploadRequest.KeyPrefix = outputprefix;
            uploadRequest.Directory = extractedDir;
            uploadRequest.SearchOption = SearchOption.AllDirectories;
            uploadRequest.SearchPattern = "*.*";
            uploadRequest.UploadFilesConcurrently = true;
            TransferUtility tr = new TransferUtility(amazonS3);
            await tr.UploadDirectoryAsync(uploadRequest).ConfigureAwait(false);
            sw.Stop();
            _logger.LogInformation($"Upload took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Extracts the downloaded zip into a directory
        /// </summary>
        /// <param name="extractedDir">Path where to extract</param>
        /// <param name="downloadFile">Zip file</param>
        private void ExtractZip(string extractedDir, string downloadFile)
        {
            Stopwatch sw = Stopwatch.StartNew();
            FastZip fastZip = new FastZip();
            if (!Directory.Exists(extractedDir))
            {
                Directory.CreateDirectory(extractedDir);
            }

            fastZip.ExtractZip(downloadFile, extractedDir, null);
            _logger.LogInformation($"Extract took {sw.ElapsedMilliseconds} ms");
            sw.Stop();
        }

        /// <summary>
        /// Downloads the fle into local disk
        /// </summary>
        /// <param name="inputBucketName">Name of the bucket to download from</param>
        /// <param name="inputObjectKey">Key of the input zip file</param>
        /// <param name="downloadFile">Location on local disk to download</param>
        /// <returns></returns>
        private async Task DownloadFileAsync(string inputBucketName, string inputObjectKey, string downloadFile)
        {
            TransferUtilityDownloadRequest request = new TransferUtilityDownloadRequest();
            request.BucketName = inputBucketName;
            request.Key = inputObjectKey;
            request.FilePath = downloadFile;

            Stopwatch sw = Stopwatch.StartNew();
            TransferUtility transferUtility = new TransferUtility(amazonS3);
            await transferUtility.DownloadAsync(request).ConfigureAwait(false);
            sw.Stop();
            _logger.LogInformation($"Download took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Unzips the zipped object stored in s3 bucket.
        /// Uses CSharpZipLib library for unzip , preserving the directory structure
        /// Make sure input directory & file name inside zip file follows Object key naming guidelines.
        /// <see cref="https://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html"/>
        /// </summary>
        /// <param name="inputBucketName">input bucket where from zip file is read</param>
        /// <param name="inputObjectKey"> s3 object key of the zip file inside input bucket</param>
        /// <param name="outputBucketName">output bucker where unzipped objects are stored</param>
        /// <param name="outputprefix">[optional]prefix to be used while constructing the output object key</param>
        /// <returns>Returns the list of objects key's in the outputbucker where unzipped files are stored.</returns>
        public async Task<List<string>> StreamUnzipUsingCSharpziplib(string inputBucketName, string inputObjectKey, string outputBucketName, string? outputprefix)
        {

            List<string> uploadedEntries = new List<string>();

            //1. Get Zip Object from S3
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = inputBucketName,
                Key = inputObjectKey
            };
            using GetObjectResponse response = await amazonS3.GetObjectAsync(request);
            using Stream responseStream = response.ResponseStream;

            var stopWatch = Stopwatch.StartNew();

            //2. Create ZipInputStream from the inputStream of S3 Object
            using ZipInputStream zipInputStream = new ZipInputStream(responseStream);
            ZipEntry theEntry;


            //3. For each enty in Zip
            while ((theEntry = zipInputStream.GetNextEntry()) != null)
            {
              //  _logger.LogDebug("Unzipping file {Name} of compression method {CompressionMethod}", theEntry.Name, theEntry.CompressionMethod);

                if (theEntry.IsDirectory)
                {
                    // every zip entry name already contains full path hene ignore this for s3 upload
                    continue;
                }

                string s3ObjectKey = CreateObjectFlags(outputprefix, theEntry.Name);
                Stream stream = Stream.Null;


                FileBufferingReadStream fileBufferingReadStream = new FileBufferingReadStream(zipInputStream, MaxMemoryThreshodinBytes, null, TempFileDir);
                await fileBufferingReadStream.DrainAsync(CancellationToken.None);
                fileBufferingReadStream.Seek(0, SeekOrigin.Begin);
                stream = fileBufferingReadStream;

                // for debug print if it is file backed
                if (!string.IsNullOrEmpty(fileBufferingReadStream.TempFileName))
                {
                   // _logger.LogDebug("Temp file is created for {theEntry.Name} -> {fileBufferingReadStream.TempFileName} : {size}", theEntry.Name, fileBufferingReadStream.TempFileName, Bytes.GetReadableSize(theEntry.Size));
                }

                await using (stream)
                {
                    try
                    {
                        await UploadStreamToS3(outputBucketName, s3ObjectKey, stream);
                        uploadedEntries.Add(s3ObjectKey);
                        //_logger.LogInformation("Zip Entry upload successfully {s3ObjectKey}; Size {size}", s3ObjectKey, Bytes.GetReadableSize(theEntry.Size));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Zip Entry upload failed. {s3ObjectKey}; Size {size}", s3ObjectKey, Bytes.GetReadableSize(theEntry.Size));
                        throw;
                    }

                }

            }
            stopWatch.Stop();
            _logger.LogInformation("Unzip & Upload Completed in  {time}", stopWatch.Elapsed.ToString());
            return uploadedEntries;
        }
        /// <summary>
        /// creates s3 object key by combining input prefix & entry name( is full path)
        /// Note that , it does not validate the S3 object key , it is zip creator responsibility to
        /// Make sure input directory & file name inside zip file follows Object key naming guidelines.
        /// <see cref="https://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html"/
        /// </summary>
        /// <param name="outputprefix"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string CreateObjectFlags(string outputprefix, string fileName)
        {
            string objectKey = string.Empty;
            if (!string.IsNullOrEmpty(outputprefix))
            {
                objectKey = $"{outputprefix}/";
            }

            objectKey += fileName;

            return objectKey;
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
        private Task UploadStreamToS3(string outputBucketName, string outputObjectKey, Stream stream)
        {
            using TransferUtility transferUtility = new TransferUtility(amazonS3);
            TransferUtilityUploadRequest transferUtilityUploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = outputBucketName,
                InputStream = stream,
                Key = outputObjectKey
            };
            return transferUtility.UploadAsync(transferUtilityUploadRequest);
        }

        private static string TempFileDir()
        {
            return Path.GetTempPath();

        }
    }
}
