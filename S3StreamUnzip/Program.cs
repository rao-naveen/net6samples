// See https://aka.ms/new-console-template for more information

using Amazon.S3;
using S3StreamUnzip;
using System.Diagnostics;
using System.Runtime;

var memInfo = GC.GetGCMemoryInfo();
var gcType = GCSettings.IsServerGC ? "Server GC" : "Workstation GC";
Console.WriteLine($"GC type {gcType}");
Console.WriteLine($"TotalAvailableMemory - {Bytes.GetReadableSize(memInfo.TotalAvailableMemoryBytes)},TotalCommitted - { Bytes.GetReadableSize(memInfo.TotalCommittedBytes)} ");

if (args.Length < 4)
{
    Console.WriteLine($"Usage S3StreamUnzip <<inputbucket>> <<input_zip_object_key>> <<out_put_dir_prefix <<s3_service_url>>");
}

string acceskey = "minioadmin";
string accessecret = "minioadmin";

string bucketName = args[0];
string inputzip = args[1];
string outputPrefix = args[2];
string s3Url = args[3] ?? "http://192.168.1.11:9000";

AmazonS3Config config = new AmazonS3Config();
config.ServiceURL = s3Url;

AmazonS3Client s3Client = new AmazonS3Client(acceskey, accessecret, config);
S3UnzipManager unzipManager = new S3UnzipManager(s3Client);
try
{
    unzipManager.Unzip2(bucketName, inputzip, outputPrefix);
}
catch (Exception exp)
{
    Console.WriteLine(exp.ToString() );
	
}

Console.WriteLine("Done");

