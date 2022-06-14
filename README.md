# SourceGenTest
This is a proof-of-concept source generator to build `Switch` methods on enums and record types.

# Enum example
Suppose you have an enum like this:

```csharp
public enum States
{
  Off,
  On
}
```

You can decorate it with a `[Switchable]` attribute, like so:

```csharp
[Switchable]
public enum States
{
  Off,
  On
}
```

This will generate a `Switch(...)` extension method, which you can use like so:

```csharp
public static void WriteState(States state)
{
  Console.WriteLine(
    state.Switch(
      on: () => "on",
      off: () => "off"
    )
  );
}
```

# But why?
As soon as you add a new value to your enum the compiler will regenerate the `Switch` method, which will force you to handle the new case everywhere you've used `Switch`.

# Record example
Similarly, you can define a `record` like this:

```csharp
[Switchable]
public abstract record Result
{
    public sealed record NotFoundResult : Result;

    public sealed record OkResult(string Message) : Result;
}
```

You can then write code like this:

```csharp
Console.WriteLine(
    new Result.OkResult("everything is ok").Switch(
        notFoundResult: nfr => "not found",
        okResult: okr => okr.Message
    )
);
```

The advantage is the same as for enums: as soon as you add a new sub-type the compiler will force you to handle the new case everywhere you have used `Switch`.
