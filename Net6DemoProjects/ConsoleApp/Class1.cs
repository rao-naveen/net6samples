using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp;

public record Person(string FirstName, string LastName);
public record Person1
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
}


// using Record types for Poco https://github.com/dotnet/runtime/issues/46299
public sealed record LogLevel
{
    public string LogCategory { get; init; }
    public string logValue { get; init; }
}
public sealed record Logging
{
    public LogLevel LogLevel { get; init; }

}
