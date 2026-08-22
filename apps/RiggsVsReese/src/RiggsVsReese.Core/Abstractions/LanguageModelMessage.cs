namespace RiggsVsReese.Core.Abstractions;

public sealed record LanguageModelMessage(
    string Role,
    string? Content,
    IReadOnlyList<LanguageModelToolCall>? ToolCalls = null,
    string? ToolName = null,
    object? ToolResult = null);

public sealed record LanguageModelToolCall(
    string Name,
    IReadOnlyDictionary<string, string?> Arguments);

public static class LanguageModelToolNames
{
    public const string RespondNarratively = "respond_narratively";
    public const string InspectOrSearch = "inspect_or_search";
    public const string AttemptInteraction = "attempt_interaction";
}
