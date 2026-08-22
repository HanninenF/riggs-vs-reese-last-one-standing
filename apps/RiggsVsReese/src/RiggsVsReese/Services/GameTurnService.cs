using RiggsVsReese.Core.Abstractions;
using RiggsVsReese.Core.Aggregates.GameSession;
using RiggsVsReese.Core.Rules;

namespace RiggsVsReese.Services;

internal sealed class GameTurnService
{
    private readonly GameState _gameState;
    private readonly ILanguageModelClient _languageModel;
    private readonly List<LanguageModelMessage> _messages =
    [
        new("system", "Du är spelledare i ett fritt textbaserat RPG. " +
                      "Svara på svenska och skriv levande men koncist. " +
                      "Detta är spelets första scen. Skriv endast en vanlig berättande scenbeskrivning. " +
                      "Använd inga verktyg och skriv inte JSON i detta första svar."),
        new("user", "Starta spelet med en provisorisk scen där Martin Riggs vaknar på en övergiven bensinstation.")
    ];

    private bool _firstRequest = true;

    internal GameTurnService(ILanguageModelClient languageModel, GameState gameState)
    {
        _languageModel = languageModel;
        _gameState = gameState;
    }

    internal Task<string?> StartAsync(CancellationToken cancellationToken = default)
    {
        return ContinueAsync(cancellationToken);
    }

    internal Task<string?> RespondAsync(
        string playerCommand,
        CancellationToken cancellationToken = default)
    {
        _messages.Add(new LanguageModelMessage("user", playerCommand));
        return ContinueAsync(cancellationToken);
    }

    private async Task<string?> ContinueAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var message = await _languageModel.SendAsync(
                _messages,
                requireToolCall: !_firstRequest,
                cancellationToken);
            _firstRequest = false;

            if (message.ToolCalls is not { Count: > 0 } calls)
            {
                AddToolInstructionsAfterOpeningScene();
                return message.Content;
            }

            _messages.Add(message);
            var toolError = false;
            var needsContinuation = false;
            string? narrativeText = null;

            foreach (var call in calls)
            {
                if (call.Name == LanguageModelToolNames.RespondNarratively)
                {
                    if (!call.Arguments.TryGetValue("text", out narrativeText))
                    {
                        toolError = true;
                        AddToolError(call.Name, "Ogiltiga argument: respond_narratively kräver fältet text.");
                    }
                }
                else if (call.Name == LanguageModelToolNames.InspectOrSearch)
                {
                    if (!call.Arguments.TryGetValue("query", out var query))
                    {
                        toolError = true;
                        AddToolError(call.Name, "Ogiltiga argument: inspect_or_search kräver fältet query.");
                        continue;
                    }

                    var result = WorldActionRules.InspectOrSearch(query);
                    _messages.Add(new LanguageModelMessage("tool", null, ToolName: call.Name, ToolResult: result));
                    needsContinuation = true;
                }
                else if (call.Name == LanguageModelToolNames.AttemptInteraction)
                {
                    if (!call.Arguments.TryGetValue("action", out var action) ||
                        !call.Arguments.TryGetValue("target", out var target))
                    {
                        toolError = true;
                        AddToolError(call.Name, "Ogiltiga argument: attempt_interaction kräver fälten action och target.");
                        continue;
                    }

                    var result = WorldActionRules.AttemptInteraction(action, target, _gameState);
                    _messages.Add(new LanguageModelMessage("tool", null, ToolName: call.Name, ToolResult: result));
                    needsContinuation = true;
                }
            }

            if (toolError || needsContinuation)
                continue;

            return narrativeText;
        }
    }

    private void AddToolInstructionsAfterOpeningScene()
    {
        if (_messages.Count != 2)
            return;

        _messages.Add(new LanguageModelMessage(
            "system",
            "Från och med nästa spelartext måste du välja exakt ett verktyg. " +
            "Använd respond_narratively för samtal, tankar och fri berättelse. " +
            "Använd inspect_or_search när spelaren undersöker, söker efter eller frågar om något som inte är känt. " +
            "Använd attempt_interaction när spelaren försöker ta, använda, ladda eller påverka något. " +
            "Spelarens inventory är just nu tomt. " +
            "Hitta aldrig på konkreta objekt eller fakta utan att först använda inspect_or_search."));
    }

    private void AddToolError(string toolName, string error)
    {
        _messages.Add(new LanguageModelMessage("tool", error, ToolName: toolName));
    }
}
