// See https://aka.ms/new-console-template for more information
using ConsoleHostTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

//var host = Host.CreateDefaultBuilder().Build();
// initiazlize configuration manager 
ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
configurationBuilder
    .AddJsonFile("appSettings.json")
        .AddEnvironmentVariables()
    .AddEnvironmentVariables("PROCESSOR_");
var configuration = configurationBuilder.Build();
Console.WriteLine(configuration);

var jsonDoc = JsonDocument.Parse(File.ReadAllText(@"C:\dev\gitrepos\net6samples\Net6DemoProjects\KubeconfigRefresh\appsettings.json"));
var dict = new JsonFlattener().FlattenJson(jsonDoc.RootElement);

Console.WriteLine(dict);

public class JsonFlattener
{
	private readonly List<KeyValuePair<string, string>> _data = new List<KeyValuePair<string, string>>();

	private readonly Stack<string> _context = new Stack<string>();

	private string _currentPath;

	public List<KeyValuePair<string, string>> FlattenJson(JsonElement rootElement)
	{
		VisitJsonElement(rootElement);
		return _data;
	}

	private void VisitJsonProperty(JsonProperty property)
	{
		EnterContext(property.Name);
		VisitJsonElement(property.Value);
		ExitContext();
	}

	private void VisitJsonElement(JsonElement element)
	{
		switch (element.ValueKind)
		{
			case JsonValueKind.Object:
				{
					foreach (JsonProperty item in element.EnumerateObject())
					{
						VisitJsonProperty(item);
					}
					break;
				}
			case JsonValueKind.Array:
				{
					for (int i = 0; i < element.GetArrayLength(); i++)
					{
						EnterContext(i.ToString());
						VisitJsonElement(element[i]);
						ExitContext();
					}
					break;
				}
			case JsonValueKind.String:
			case JsonValueKind.Number:
			case JsonValueKind.True:
			case JsonValueKind.False:
			case JsonValueKind.Null:
				_data.Add(new KeyValuePair<string, string>(_currentPath, element.ToString()));
				break;
		}
	}

	private void EnterContext(string context)
	{
		_context.Push(context);
		_currentPath = ConfigurationPath.Combine(_context.Reverse());
	}

	private void ExitContext()
	{
		_context.Pop();
		_currentPath = ConfigurationPath.Combine(_context.Reverse());
	}
}

