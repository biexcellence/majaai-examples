# maja.AI Examples

This repository contains examples for using [maja.ai](https://maja.ai).

## maja.AI Action Examples

You can create custom actions for maja.ai to return custom answers for user queries.
Before you start create a new Action on https://partner.maja.ai.


### maja.AI Action using WebSockets

Add a reference to the [`BiExcellence.MajaAi.Action.WebSocket`](https://www.nuget.org/packages/BiExcellence.MajaAi.Action.WebSocket/) NuGet package to your project.

```csharp
static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

static async Task MainAsync(string[] args)
{
    using (var action = new MajaAiWebSocketAction("ACTION ID", HandleMajaAiAction))
    {
        await action.Start();

        Console.ReadLine();
    }
}

private static async Task<MajaAiActionResponse> HandleMajaAiAction(IMajaAiActionRequest request, CancellationToken cancellationToken)
{
    Console.WriteLine($"Query: {request.Query}");

    await Task.Delay(1000);

    return new MajaAiActionResponse(new MajaAiSimpleAnswer("Hello World!"));
}
```

### maja.AI Action using AspNet.Core Webhook

Add a reference to the [`BiExcellence.MajaAi.Action.AspNetCore`](https://www.nuget.org/packages/BiExcellence.MajaAi.Action.AspNetCore/) NuGet package to your project.

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseMajaAiAction("ACTION ID", HandleMajaAiAction);
    }

    private async Task<MajaAiActionResponse> HandleMajaAiAction(IMajaAiActionRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Query: {request.Query}");

        await Task.Delay(1000);

        return new MajaAiActionResponse(new MajaAiSimpleAnswer("Hello World!"));
    }
}
```
