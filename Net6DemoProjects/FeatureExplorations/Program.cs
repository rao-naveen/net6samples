// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
HttpClient client = new HttpClient();
client.RequestTokenAsync(new TokenRequest() { });

void DoSomething()
{
    List<string> list = default;

    // lazy initialization of list if not null
    list ??= new();

}
