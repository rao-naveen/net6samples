This sample demonstrate the unzipping of zip file stored in S3 bucket without loading whole zip file in to client memory/disk 
It uses CSharpziplib zipinputstream to extract the zip entry iteratively. 

For **each zip entry** , if the **size > 50 MB ** and temorary file will be created else , it will be loaded in memory.
It uses AWS TransferUtility to upload the extracted zip file on disk or in memory 

Sample usage.
```shell
dotnet S3StreamUnzip.dll <<inputbucket>>  <<input_zip_object_key>> <<outputbucket>> <<out_put_dir_prefix>> [<<s3_service_url>>]
```

Sample usage with local minio
```shell
dotnet S3StreamUnzip.dll myinputbucket  sample.zip outputBucket demo_prefix  http://127.0.0.1:9000
```

Sample usage with AWS S3. Will use credentails & region using .aws profile 
```shell
dotnet S3StreamUnzip.dll myinputbucket  sample.zip outputBucket demo_prefix
```
using the API
```csharp
// logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddFilter("S3StreamUnzip", LogLevel.Debug);
    builder.ClearProviders();
    builder.AddConsole();
});
var s3StreamLogger = loggerFactory.CreateLogger("S3StreamUnzip");

// s3 client
AmazonS3Config config = new AmazonS3Config();
AmazonS3Client s3Client = new AmazonS3Client(config);

S3UnzipManager unzipManager = new S3UnzipManager(s3Client, logger);
try
{
    var listOfExtractedObjectKeys = unzipManager.UnzipUsingCSharpziplib(inputBucketName, inputZipObjectKey, outputBucketName, string.Empty).GetAwaiter().GetResult();
    foreach (var objectKey in listOfExtractedObjectKeys)
    {
        Console.WriteLine(objectKey);
    }
}
catch (Exception exp)
{
    Console.WriteLine(exp.ToString());

}
```
