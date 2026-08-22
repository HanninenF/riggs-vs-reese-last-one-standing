# Abstractions

## Syfte

Här beskriver Core vilken hjälp spelet behöver från omvärlden utan att välja en teknisk lösning. Ett gränssnitt fungerar som ett kontrakt: det säger vad som ska kunna göras, men inte hur.

## Känner till

`ILanguageModelClient` säger exempelvis att spelet kan skicka meddelanden till en språkmodell. Core känner inte till att den nuvarande implementationen använder Ollama eller HTTP.

## Implementeras och kopplas ihop av

Infrastructure implementerar kontrakten och `Program.cs` kopplar ihop kontrakt med rätt implementation. Ett kontrakt för save/load läggs till när den funktionen byggs.
