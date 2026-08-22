# RiggsVsReese.Infrastructure

## Syfte

Infrastructure innehåller kod som pratar med världen utanför spelet, exempelvis Ollama, HTTP, JSON och senare filsystemet.

## Känner till och implementerar

Lagret känner till kontrakten i Core och implementerar dem med en konkret teknik. `OllamaChatClient` implementerar exempelvis `ILanguageModelClient` genom att göra HTTP-anrop till Ollama.

## Används av och känner inte till

`Program.cs` skapar Infrastructure-implementationerna och övrig kod använder dem genom Core-kontrakten. Infrastructure känner inte till terminalflödet och får aldrig bestämma spelregler eller ändra game state på egen hand.
