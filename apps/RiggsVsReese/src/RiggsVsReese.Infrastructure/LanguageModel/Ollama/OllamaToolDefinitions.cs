using RiggsVsReese.Core.Abstractions;

namespace RiggsVsReese.Infrastructure.LanguageModel.Ollama;

internal static class OllamaToolDefinitions
{
    internal static object[] All { get; } =
    [
        new
        {
            type = "function",
            function = new
            {
                name = LanguageModelToolNames.RespondNarratively,
                description = "Skriver ett fritt narrativt svar när ingen kontroll av spelvärlden behövs.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        text = new
                        {
                            type = "string",
                            description = "Det narrativa svaret till spelaren."
                        }
                    },
                    required = new[] { "text" }
                }
            }
        },
        new
        {
            type = "function",
            function = new
            {
                name = LanguageModelToolNames.InspectOrSearch,
                description = "Kontrollerar spelmotorn när spelaren undersöker eller söker efter något.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        query = new
                        {
                            type = "string",
                            description = "Vad spelaren letar efter eller undersöker."
                        }
                    },
                    required = new[] { "query" }
                }
            }
        },
        new
        {
            type = "function",
            function = new
            {
                name = LanguageModelToolNames.AttemptInteraction,
                description = "Kontrollerar en handling där spelaren försöker ta, använda, ladda eller påverka något.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new
                        {
                            type = "string",
                            description = "Handlingen, exempelvis load eller pick_up."
                        },
                        target = new
                        {
                            type = "string",
                            description = "Föremålet eller målet för handlingen."
                        }
                    },
                    required = new[] { "action", "target" }
                }
            }
        }
    ];
}
