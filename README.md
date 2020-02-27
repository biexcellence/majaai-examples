# majaAI Examples

This repository contains examples for using [majaAI](https://maja.ai).
If you want to make use of majaAI simply fill out the [contact](https://maja.ai/kontakt).

- [majaAI Chat JS](#majaai-chat-js)
- [majaAI Action Examples](#majaai-action-examples)

## majaAI Chat JS

Here are the options for embedding majaAI into your homepage.
Please note that the majaAI Chat JS currently requires jQuery and Bootstrap.

### JavaScript options

| Name | Type | Default Value | Description |
|-----------|-----------|-----------|-----------|
| apiKey | `string` | **required** | The API key |
| title | `string` | `"majaAI"` | The title of the chat box |
| teaserTitle | `string` | `"Frag majaAI"` | The title of the teaser |
| teaserImage | `string` | `"//cdn.biexcellence.com/majaai/img/maja_head.png"` | The image of the teaser |
| placeholderText | `string` | `"Ihre Frage..."` | The default placeholder for the input field |
| language | `string` | `"de"` | The primary chat language |
| languageRegion | `string` | `"de-DE"` | The primary chat language region |
| visible | `boolean` | `false` | If the chat box should be visible on start |
| welcomeText | `string` | `undefined` | The welcome chat text |
| theme | `string` | `undefined` | Additional css file which is loaded dynamically |
| hidePoweredBy | `boolean` | `false` | If the powered by text should be displayed |
| majaAvatar | `string` | `"//cdn.biexcellence.com/majaai/img/MajaAI_AI_120px.png"` | The avatar for majaAI answers |
| onInit | `function` | `undefined` | Callback after chat initilization |
| onQuery | `function` | `undefined` | Callback for user queries |
| onResult | `function` | `undefined` | Callback for answers |
| onError | `function` | `undefined` | Callback for server errors |
| bubblePopupTime| `Integer` | `20000` | Time after the help bubble shows up |
| tabs | `Array` | `undefined` | QuickTabs on the left side of Maja |

### JavaScript Tab options

| Name | Type | Default Value | Description |
|-----------|-----------|-----------|-----------|
| html | `string` | `undefined` | Html which is displayed in the tab button |
| link | `string` | `undefined` | A link to a different page |
| contentHtml | `string` | `undefined` | Html which gets appended in the chat window if the user clicks on the tab |
| toggle | `boolean` | `false` | If the chat window should slide in or not (this does not work with `link`) |

### JavaScript functions

| Name | Description |
|-----------|-----------|
| send(query) | Sends a query to majaAI |
| destroy() | Destroys the majaAI chat UI |

### Usage Example

```html
<!-- Bootstrap CSS -->
<link rel="stylesheet" href="//maxcdn.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css">

<!-- jQuery library -->
<script src="//ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>

<!-- Bootstrap library -->
<script src="//maxcdn.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.min.js"></script>

<!-- majaAI Chat JS -->
<script src="//cdn.biexcellence.com/majaai/js/chat.js"></script>
<script>
var majaAi = new MajaAi({
    apiKey: "123",
    welcomeText: "Hallo, Mein Name ist Maja, wie kann ich behilflich sein?",
    title: "majaAI rocks",
    teaserTitle: "majaAI",
    teaserImage: "//cdn.biexcellence.com/majaai/img/maja_head.png",
    placeholderText: "Deine Frage...",
    language: "de",
    languageRegion: "de-DE",
    theme: "mytheme.css",
    hidePoweredBy: true,
    visible: true,
    majaAvatar: "//cdn.biexcellence.com/majaai/img/MajaAI_AI_120px.png",
    onInit: function() { /* ... */ },
    onQuery: function(query) { /* ... */ },
    onResult: function(query, answers) { /* ... */ },
    onError: function(error) { /* ... */ },
    bubblePopupTime: 10000,
    tabs: [{
        html: '<i class="fas fa-question-circle"></i><span>Hilfe</span>',
        toggle: true,
        contentHtml: '<h3>Häufig gestellte Fragen</h3><ul><li><a href="#" class="majaai-question">Wo finde ich den nächsten Geldautomaten?</a></li></ul>'
    }, {
        html: '<i class="fas fa-question-circle"></i><span>Help</span>',
        link: 'http://biexcellence.com/help'
    }, {
        html: '<i class="fas fa-envelope"></i><span>Contact</span>',
        link: 'http://biexcellence.com/contact'
    }]
});

// send query to MajaAI
majaAi.send("Hello MajaAI");

// destroys the chat UI
majaAi.destroy();
</script>
```

## majaAI Action Examples

You can create custom actions for majaAI to return custom answers for user queries.
Before you start create a new Action on https://partner.maja.ai.


### majaAI Action using WebSockets

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
    if (!request.ActionCompleted)
    {
        Console.WriteLine($"Query: {request.Query}");

        await Task.Delay(1000);

        return new MajaAiActionResponse(new MajaAiSimpleAnswer("Hello World!"));
    }
    return null;
}
```

### majaAI Action using AspNet.Core Webhook

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
        if (!request.ActionCompleted)
        {
            Console.WriteLine($"Query: {request.Query}");

            await Task.Delay(1000);

            return new MajaAiActionResponse(new MajaAiSimpleAnswer("Hello World!"));
        }
        return null;
    }
}
```
