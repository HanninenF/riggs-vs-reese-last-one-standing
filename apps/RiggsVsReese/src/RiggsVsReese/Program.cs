using RiggsVsReese;
using RiggsVsReese.Core.Aggregates.GameSession;
using RiggsVsReese.Infrastructure.LanguageModel.Ollama;
using RiggsVsReese.Services;

using var languageModel = new OllamaChatClient(
    new Uri("http://localhost:11434"),
    "gpt-oss:latest",
    TimeSpan.FromMinutes(10));

var gameState = new GameState();
var turnService = new GameTurnService(languageModel, gameState);
var gameLoop = new GameLoop(turnService);

await gameLoop.RunAsync();
