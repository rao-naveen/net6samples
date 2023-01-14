// See https://aka.ms/new-console-template for more information
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

using AmazonSecurityTokenServiceClient client = new();
//AWSCredentials stsUser = new Credentials();
//GetSessionTokenRequest getSessionTokenRequest = new GetSessionTokenRequest() { DurationSeconds = 50 };
//var sessonToken = client.GetSessionTokenAsync(getSessionTokenRequest).GetAwaiter().GetResult();
//stsUser = sessonToken.Credentials;

var req = new GetCallerIdentityRequest();
//var webRequest = req as IAmazonWebServiceRequest;
//webRequest.AddBeforeRequestHandler((sender, eventorgs) =>
//{
//    global::System.Console.WriteLine(eventorgs);
//});

var result = client.GetCallerIdentityAsync(req).GetAwaiter().GetResult();

Console.WriteLine(result);

