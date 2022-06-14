using MakeEnumsGreatAgain;

namespace ConsoleApp2;

[Switchable]
public abstract record Result
{
    public sealed record NotFoundResult : Result;

    public sealed record OkResult(string Message) : Result;
}