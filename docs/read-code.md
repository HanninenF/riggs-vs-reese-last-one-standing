# Så läser du koden

Börja inte med alla filer. Läs flödet i den ordning som programmet startas och data rör sig.

## 1. Börja i programmets root

Öppna:

`apps/RiggsVsReese/src/RiggsVsReese/Program.cs`

Det här är appens sammansättningspunkt. Läs raderna i ordning:

- `OllamaChatClient` skapas.
- `GameState` skapas.
- `GameTurnService` skapas med språkmodellsklienten och game state.
- `GameLoop` skapas med servicen.
- `gameLoop.RunAsync()` startar programmet.

`Program.cs` bestämmer alltså vilka implementationer som kopplas ihop, men innehåller inte själva spelflödet eller spelreglerna.

## 2. Läs terminalens flöde

Öppna:

`apps/RiggsVsReese/src/RiggsVsReese/GameLoop.cs`

Läs särskilt:

- `RunAsync()`
- `ReadPlayerCommand()`
- `WriteResponse()`

`GameLoop` är terminalens samordnare. Den:

- startar den första scenen
- skriver ut språkmodellens svar
- väntar på spelarens text
- skickar texten till `GameTurnService`
- fortsätter tills spelaren skriver `/exit` eller en tom rad

Den ska inte avgöra om en handling är tillåten i spelvärlden.

## 3. Läs användningsfallet för en speltur

Öppna:

`apps/RiggsVsReese/src/RiggsVsReese/Services/GameTurnService.cs`

Börja med dessa metoder:

- `StartAsync()` – startar den första scenen.
- `RespondAsync()` – tar emot spelarens kommando.
- `ContinueAsync()` – driver kommunikationen tills ett färdigt narrativt svar finns.
- `BuildMessagesForRequest()` – bygger meddelandelistan inför ett LLM-anrop.
- `BuildGameStateContext()` – läser aktuellt game state och skapar en state-snapshot.

Det här är application service-lagret. Servicen bestämmer ordningen på stegen, men den ska inte själv hitta på spelregler.

## 4. Följ den dynamiska state-snapshoten

När `GameTurnService` skickar en förfrågan till språkmodellen anropas först `BuildMessagesForRequest()`.

Den anropar `BuildGameStateContext()`, som läser från `GameState`:

- utrustat vapen
- laddat magasin
- antal skott
- patron i kammaren
- vapnets skick
- reservmagasin i inventory

Detta skickas som aktuell kontext till språkmodellen varje gång. Inventory ska därför inte beskrivas med en hårdkodad text i instruktionerna.

Mental modell:

```text
GameState
  → aktuell state-snapshot
  → LLM-anrop
  → föreslaget tool call eller narrativt svar
```

## 5. Läs språkmodellens tekniska adapter

Öppna:

`apps/RiggsVsReese/src/RiggsVsReese.Infrastructure/LanguageModel/Ollama/OllamaChatClient.cs`

Läs särskilt:

- `SendAsync()`
- `ToOllamaMessage()`
- `ParseMessage()`
- `ParseArguments()`

Här ser du hur Core-kontraktet översätts till HTTP och JSON. Adaptern:

- bygger requesten
- lägger till verktygsdefinitioner efter den första scenen
- skickar requesten till Ollama
- läser JSON-svaret
- omvandlar svaret till `LanguageModelMessage`

Den bestämmer inte om spelaren får skjuta, plocka upp eller använda ett föremål.

## 6. Läs tool-dispatchen i servicen

Gå tillbaka till `GameTurnService.cs` och läs:

- `ProcessToolCalls()`
- `ProcessToolCall()`
- `ProcessNarrativeCall()`
- `ProcessInspectionCall()`
- `ProcessInteractionCall()`

Här översätts språkmodellens strukturerade val till intern spel-logik:

- `respond_narratively` returnerar berättelsetext.
- `inspect_or_search` anropar en sök- eller undersökningsregel.
- `attempt_interaction` anropar en interaktionsregel.

Om ett verktyg behöver information från spelvärlden läggs resultatet till i meddelandehistoriken och språkmodellen får fortsätta en ny iteration.

## 7. Läs spelreglerna i Core

Öppna:

`apps/RiggsVsReese/src/RiggsVsReese.Core/Rules/WorldActionRules.cs`

Här avgörs vad som är tillåtet i spelvärlden. Reglerna ska vara deterministiska och fria från terminal, HTTP, JSON och språkmodell.

Nuvarande regler är embryonala:

- `InspectOrSearch()` returnerar att den riktiga världsmotorn ännu inte är implementerad.
- `AttemptInteraction()` kontrollerar om det utrustade vapnet eller ett magasin matchar spelarens mål.

När vapensystemet växer ska regler för att skjuta, ladda om, byta magasin och kontrollera ammunition ligga i Core-modellen eller rena Core-regler.

## 8. Läs game state och vapenmodellen

Börja med:

`apps/RiggsVsReese/src/RiggsVsReese.Core/Aggregates/GameSession/GameState.cs`

Sedan läser du:

`apps/RiggsVsReese/src/RiggsVsReese.Core/Items/Weapons/Weapon.cs`

`apps/RiggsVsReese/src/RiggsVsReese.Core/Items/Weapons/Magazine.cs`

`apps/RiggsVsReese/src/RiggsVsReese.Core/Items/Weapons/WeaponDefinition.cs`

`apps/RiggsVsReese/src/RiggsVsReese.Core/Items/Weapons/MagazineDefinition.cs`

`apps/RiggsVsReese/src/RiggsVsReese.Core/Items/Weapons/WeaponCatalog.cs`

Skilj på två sorters information:

- Definitioner beskriver fasta egenskaper, som namn, kapacitet, skada och räckvidd.
- Runtime-objekt beskriver matchens aktuella tillstånd, som antal skott, patron i kammaren och skick.

`GameState` är fortfarande prototypen till aggregatroten `GameSession`. Den ska på sikt vara den samlade sanningskällan för förändringar i matchen.

## Mental modell

Första scenen:

```text
Program.cs
  → GameLoop.RunAsync()
  → GameTurnService.StartAsync()
  → ContinueAsync()
  → BuildMessagesForRequest()
  → BuildGameStateContext()
  → OllamaChatClient.SendAsync()
  → GameLoop skriver ut svaret
```

En spelares handling:

```text
spelaren skriver text
  → GameLoop.ReadPlayerCommand()
  → GameTurnService.RespondAsync()
  → BuildGameStateContext()
  → Ollama föreslår ett tool call
  → GameTurnService dispatchar tool call
  → WorldActionRules kontrollerar handlingen
  → resultatet läggs till i meddelandehistoriken
  → Ollama formulerar narrativt svar
  → GameLoop skriver ut svaret
```

Det bästa sättet att förstå en ändring är att följa ett konkret användarflöde från terminalen till Core-regeln och sedan tillbaka till terminalen.

## När du undrar var något hör hemma

Fråga i denna ordning:

1. Är det terminalinmatning eller terminalutmatning? Läs eller ändra `GameLoop`.
2. Samordnar det flera steg i en speltur? Läs eller ändra `GameTurnService`.
3. Bestämmer det vad som är sant eller tillåtet i spelet? Lägg det i Core.
4. Pratar det HTTP, JSON eller med en extern språkmodell? Lägg det i Infrastructure.
5. Är det bara en kopplingsdetalj? Då hör det sannolikt hemma i `Program.cs`.

