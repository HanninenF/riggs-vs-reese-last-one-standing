# Ollama

## Syfte

Här finns all kod som är särskild för Ollama. `OllamaChatClient` skickar HTTP-anrop, översätter meddelanden till och från JSON och använder tool-definitionerna i `OllamaToolDefinitions`.

## Känner till och anropas genom

Koden känner till Ollamas API och Core-kontraktet `ILanguageModelClient`. Den anropas av `GameTurnService` genom kontraktet, så servicen behöver inte veta vilken språkmodell som används.

## Känner inte till

Mappen känner inte till game state eller spelregler. En annan språkmodell kan senare få en egen mapp och implementation utan att Core behöver ändras.
