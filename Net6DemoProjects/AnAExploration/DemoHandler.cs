
public class DemoHandler : DelegatingHandler
{
    const string IntrospectResponse = "{\"active\": true,\"scope\": \"read write\",\"permissions\": \"Search Create\",\"client_id\":\"client_id_for_introspection_endpoint\",\"username\":\"chandler bing\",\"exp\":1671799899}";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.OK;
        httpResponseMessage.Content = new StringContent(IntrospectResponse,System.Text.Encoding.UTF8, "application/json");
        return Task.FromResult(httpResponseMessage);// base.SendAsync(request, cancellationToken);
    }
}