// See https://aka.ms/new-console-template for more information

using Amazon.S3;
using S3StreamUnzip;
using System.Diagnostics;
using System.Runtime;
using Amazon.Runtime;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


var memInfo = GC.GetGCMemoryInfo();
var gcType = GCSettings.IsServerGC ? "Server GC" : "Workstation GC";
Console.WriteLine($"GC type {gcType}");
Console.WriteLine($"TotalAvailableMemory - {Bytes.GetReadableSize(memInfo.TotalAvailableMemoryBytes)},TotalCommitted - { Bytes.GetReadableSize(memInfo.TotalCommittedBytes)} ");

if (args.Length < 4)
{
    Console.WriteLine($"Usage S3StreamUnzip <<inputbucket>>  <<input_zip_object_key>> <<outputbucket>> <<out_put_dir_prefix>> [<<s3_service_url>>]");
    return;
}

string acceskey = "minioadmin";
string secretKey = "minioadmin";

string mode = args[0];
string inputBucketName = args[1];
string inputZipObjectKey = args[2];
string outputBucketName = args[3];
string outputPrefix = args[4];
string s3Url = string.Empty;
if (args.Length > 5)
{
    s3Url = args[5];
}
string downloadDir = string.Empty;
if (args.Length > 6)
{
    downloadDir = args[6];
}


var logger = Helper.GetLogger();

IAmazonS3 s3Client;
if (string.IsNullOrEmpty(s3Url))
{
    // create from .aws profile
    s3Client = Helper.GetS3ClientUsingAwsProfile();
}
else
{
    // using minio
    s3Client = Helper.GetS3ClientForMinio(s3Url, acceskey, secretKey);
}


S3UnzipManager unzipManager = new S3UnzipManager(s3Client, logger);
try
{
    Stopwatch sw = Stopwatch.StartNew();
    if (mode.ToLower() == "download")
    {
        var list = unzipManager
            .DirectoryUnzipUsingCSharpziplib(inputBucketName, inputZipObjectKey, outputBucketName, string.Empty, downloadDir)
            .GetAwaiter().GetResult();
        sw.Stop();
        Console.WriteLine($"Download + zip + upload took {sw.ElapsedMilliseconds} ms");
    }
    else
    {
        var list = unzipManager
            .StreamUnzipUsingCSharpziplib(inputBucketName, inputZipObjectKey, outputBucketName, string.Empty)
            .GetAwaiter().GetResult();
        sw.Stop();
        Console.WriteLine($"Stream unzip took {sw.ElapsedMilliseconds} ms");
    }
}
catch (Exception exp)
{
    Console.WriteLine(exp.ToString());

}

Console.WriteLine("Done");

