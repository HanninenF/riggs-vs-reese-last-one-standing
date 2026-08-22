using System.Net.Http.Json;
using System.Text.Json;
using RiggsVsReese.Core.Abstractions;

namespace RiggsVsReese.Infrastructure.LanguageModel.Ollama;

public sealed class OllamaChatClient : ILanguageModelClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaChatClient(Uri baseAddress, string model, TimeSpan timeout)
    {
        _model = model;
        _httpClient = new HttpClient
        {
            BaseAddress = baseAddress,
            Timeout = timeout
        };
    }

    public async Task<LanguageModelMessage> SendAsync(
        IReadOnlyList<LanguageModelMessage> messages,
        bool requireToolCall,
        CancellationToken cancellationToken = default)
    {
        var request = new Dictionary<string, object?>
        {
            ["model"] = _model,
            ["messages"] = messages.Select(ToOllamaMessage).ToArray(),
            ["stream"] = false
        };

        if (requireToolCall)
        {
            request["tools"] = OllamaToolDefinitions.All;
            request["tool_choice"] = "required";
        }

        using var response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ollama svarade {(int)response.StatusCode}: {responseBody}");

        using var json = JsonDocument.Parse(responseBody);
        return ParseMessage(json.RootElement.GetProperty("message"));
    }

    public void Dispose() => _httpClient.Dispose();

    private static Dictionary<string, object?> ToOllamaMessage(LanguageModelMessage message)
    {
        var result = new Dictionary<string, object?>
        {
            ["role"] = message.Role,
            ["content"] = message.ToolResult is null
                ? message.Content
                : JsonSerializer.Serialize(message.ToolResult, JsonSerializerOptions.Web)
        };

        if (message.ToolCalls is { Count: > 0 })
        {
            result["tool_calls"] = message.ToolCalls.Select(call => new
            {
                function = new
                {
                    name = call.Name,
                    arguments = call.Arguments
                }
            }).ToArray();
        }

        if (message.ToolName is not null)
            result["tool_name"] = message.ToolName;

        return result;
    }

    private static LanguageModelMessage ParseMessage(JsonElement message)
    {
        var role = message.GetProperty("role").GetString() ?? "assistant";
        var content = message.TryGetProperty("content", out var contentElement)
            ? contentElement.GetString()
            : null;
        List<LanguageModelToolCall>? toolCalls = null;

        if (message.TryGetProperty("tool_calls", out var calls) && calls.GetArrayLength() > 0)
        {
            toolCalls = [];
            foreach (var call in calls.EnumerateArray())
            {
                var function = call.GetProperty("function");
                var name = function.GetProperty("name").GetString()
                    ?? throw new JsonException("Ollama returnerade ett tool call utan namn.");
                toolCalls.Add(new LanguageModelToolCall(
                    name,
                    ParseArguments(function.GetProperty("arguments"))));
            }
        }

        return new LanguageModelMessage(role, content, toolCalls);
    }

    private static IReadOnlyDictionary<string, string?> ParseArguments(JsonElement arguments)
    {
        if (arguments.ValueKind == JsonValueKind.String)
        {
            using var json = JsonDocument.Parse(arguments.GetString() ?? "{}");
            return ParseArgumentObject(json.RootElement);
        }

        return ParseArgumentObject(arguments);
    }

    private static IReadOnlyDictionary<string, string?> ParseArgumentObject(JsonElement arguments)
    {
        if (arguments.ValueKind != JsonValueKind.Object)
            return new Dictionary<string, string?>();

        return arguments.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString()
                : property.Value.GetRawText());
    }
}
