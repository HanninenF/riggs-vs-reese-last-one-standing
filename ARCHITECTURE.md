# Arkitektur

## Working title

**Riggs vs. Reese: Last One Standing**

## Beslutade val

### Frontend

Terminalbaserad textklient.

Den första versionen ska visa spelstatus, ta emot spelarens textkommando och skriva ut resultatet.

### Backend

C# med .NET.

### Kodstruktur

Projektet använder tre runtime-projekt med tydliga ansvarsgränser. Användargränsen är för närvarande en terminal.

- `RiggsVsReese` är det körbara presentationsprojektet med `Program.cs`, terminalgräns och serviceorkestrering.
- `RiggsVsReese.Core` innehåller domänlogik, aggregat, regler, kontrakt och validering.
- `RiggsVsReese.Infrastructure` innehåller Ollama, HTTP, framtida JSON-persistens och andra tekniska implementationer.

Event sourcing, source generators, separata läs- och skrivmodeller och generiska repository-lager kopieras inte från referensprojektet. De löser problem som spelet ännu inte har.

#### Beroenderiktning

```text
RiggsVsReese ─→ RiggsVsReese.Infrastructure ─→ RiggsVsReese.Core

RiggsVsReese.Core ─────────→ inga yttre lager
```

Det körbara projektet refererar Infrastructure, Infrastructure refererar Core och Core refererar inte de yttre projekten. Core definierar de gränssnitt som behövs för omvärlden, Infrastructure implementerar dem och det körbara projektet kopplar ihop programmet.

#### Målbild

Den beslutade projektstrukturen är:

```text
apps/
  RiggsVsReese/
    RiggsVsReese.slnx
    src/
      RiggsVsReese/
        RiggsVsReese.csproj
        Program.cs
        GameLoop.cs
        Services/

      RiggsVsReese.Api/                  # reserverad framtida HTTP-host
        Endpoints/
        Controllers/

      RiggsVsReese.Core/
        RiggsVsReese.Core.csproj
        Aggregates/
          GameSession/
        Rules/
        Abstractions/
          Persistence/                   # framtida repository-kontrakt
        EventSourcing/                   # reserverad, inte beslutad

      RiggsVsReese.Infrastructure/
        RiggsVsReese.Infrastructure.csproj
        LanguageModel/
        Persistence/
          Repositories/                  # framtida implementationer

    tests/
      RiggsVsReese.UnitTests/
      RiggsVsReese.IntegrationTests/
```

De tre beslutade runtime-projekten och lösningsfilen är skapade. Framtida gränser får finnas som README-mappar, men blir inte projekt eller implementationer förrän deras aktiveringsvillkor är uppfyllda.

| Projekt eller mapp | Ansvar |
| --- | --- |
| `RiggsVsReese` | Körbar terminalapp, presentation och orkestrering |
| `RiggsVsReese/GameLoop.cs` | Tar emot spelarens text och presenterar spelets svar |
| `RiggsVsReese/Services` | Samordnar stegen i ett användningsfall |
| `RiggsVsReese.Core` | Game state, domänregler och kontrakt |
| `RiggsVsReese.Infrastructure` | Kommunikation med externa system och framtida persistens |
| `RiggsVsReese.UnitTests` | Snabba tester, främst av Core |
| `RiggsVsReese.IntegrationTests` | Tester av Ollama-, JSON-, filsystems- och hostintegrationer |

#### Reserverade framtidsgränser

| Plats | Status | Aktiveras när |
| --- | --- | --- |
| `Core/Abstractions/Persistence` | Reserverad | Save/load behöver ett `IGameSessionRepository`-kontrakt |
| `Infrastructure/Persistence/Repositories` | Reserverad | En konkret JSON- och filsystemsimplementation byggs |
| `RiggsVsReese.Api/Endpoints` | Reserverad | Spelet får ett HTTP-API; Minimal API är förstahandsvalet |
| `RiggsVsReese.Api/Controllers` | Alternativ | MVC väljs i stället för Minimal API; controllers och endpoints används inte parallellt utan skäl |
| `Core/EventSourcing` | Ej beslutad | Krav på full händelsehistorik, replay eller undo motiverar tekniken |

#### Vad som hör hemma var

| Del | Ansvar i spelet | Ska inte göra |
| --- | --- | --- |
| `RiggsVsReese.Core/Aggregates` | Äga game state och skydda regler som alltid måste gälla | Anropa Ollama, läsa konsolen eller skriva filer |
| `RiggsVsReese.Core/Rules` | Deterministiska beräkningar som strid, upptäckt, skada och seedad slump | Presentera text eller välja lagringsteknik |
| `RiggsVsReese.Core/Abstractions` | Små portar mot omvärlden, exempelvis berättare och sparning | Innehålla Ollama- eller JSON-detaljer |
| `RiggsVsReese.Infrastructure/LanguageModel` | HTTP, Ollama-format, tool-schema och serialisering av LLM-anrop | Bestämma om en spelhandling lyckas |
| `RiggsVsReese.Infrastructure/Persistence` | Spara och läsa en match som JSON | Ändra game state eller lägga till domänregler |
| `RiggsVsReese` | Läsa och skriva terminaltext samt starta spelet | Äga spelregler |
| `RiggsVsReese/Services` | Orkestrera ett helt användningsfall, exempelvis en speltur | Duplicera regler som hör hemma i aggregatet eller rules engine |

Mappar organiseras efter ansvar och därefter efter spelfunktion. Undvik allmänna uppsamlingsmappar där orelaterade typer hamnar enbart för att de heter `Models`, `Helpers` eller `Services`.

#### Begrepp och placeringsregler

**Aggregate** är en konsistens- och spargräns, inte bara ett annat ord för modell. För den lokala enspelarmatchen är `GameState` prototypen till aggregatroten `GameSession`. Den äger spelare, inventory, Reese, världen och matchens seed. All ändring som måste hålla spelets regler giltiga går genom metoder på aggregatet eller Core-regler som aggregatet använder. Delarna får inte egna repositories bara för att de är egna C#-klasser.

**Entity** har en identitet och kan ändras över tid, exempelvis en karaktär eller ett föremål med ett unikt id. Ett **value object** beskriver ett värde och jämförs genom sitt innehåll, exempelvis position, skada eller ett tärningsresultat. De är delar av aggregatet om de inte behöver en egen oberoende livscykel.

**Domain service eller rule** används när en ren domänoperation inte naturligt tillhör en enda entity, exempelvis att jämföra två aktörers stats och terräng vid upptäckt. Den ska vara deterministisk och fri från I/O. Skapa inte en service för vanlig state-förändring som bättre uttrycks av `GameSession.AttemptInteraction(...)`.

**Application service** orkestrerar ett användningsfall. `GameTurnService` hanterar språkmodellens tool calls, anropar Core-regler och håller ordning på en speltur. Den bestämmer ordningen men inte regelutfallen. Den ligger i det körbara projektets `Services`; inget separat Application-projekt införs.

**Interface eller port** beskriver vad det inre lagret behöver, inte vilken produkt som används. `ILanguageModelClient` är den nuvarande porten och `OllamaChatClient` dess Infrastructure-implementation. När sparning implementeras tillkommer `IGameSessionRepository`. Ett interface är motiverat för språkmodellen eftersom provider uttryckligen ska kunna bytas; interna klasser får vara konkreta tills en verklig gräns behövs.

**Repository** döljer hur ett aggregate hämtas och sparas. Första kontraktet ska vara specifikt för matchen, exempelvis `IGameSessionRepository`, med en implementation som `JsonGameSessionRepository`. Vi inför inte `IRepository<T>`, unit of work eller ett separat Data-projekt för en enda JSON-fil. Om JSON-formatet senare behöver skilja sig från domänmodellen används persistensmodeller endast i `Infrastructure/Persistence`.

**Endpoint** är en enskild HTTP-operation i Minimal API. **Controller** är MVC:s klassbaserade sätt att samla HTTP-actions. Referensprojektet använder främst en endpointfil per operation och tunna handlers. Terminalspelet har ingen HTTP-gräns, så det behöver varken endpoints eller controllers. Om ett webb-API läggs till senare väljer vi ett av mönstren; HTTP-lagret validerar och översätter anrop men delegerar spelreglerna till Core.

**DTO/request/response** är dataformat vid en yttre gräns. Ollamas JSON-objekt och ett framtida HTTP-anrops requestmodeller är därför inte domänmodeller, även om de råkar ha liknande fält.

#### Flöde för en speltur

```text
GameLoop
  → GameTurnService
  → ILanguageModelClient (OllamaChatClient)
  → GameState + Core-regler
  → ILanguageModelClient formulerar resultatet
  → GameLoop skriver resultatet
  → framtida IGameSessionRepository sparar om state ändrades
```

LLM-adaptern föreslår avsikt och formulerar text. `GameSession` och Core-reglerna är fortfarande sanningskällan för vad som faktiskt händer.

#### Nuvarande ansvarsfördelning

Den första strukturella extraktionen är genomförd:

- `Program.cs` skapar implementationer, kopplar ihop dem och startar `GameLoop`.
- `GameLoop` äger terminalens in- och utmatning.
- `GameTurnService` äger konversationsflöde och tool-dispatch.
- `OllamaChatClient` och `OllamaToolDefinitions` äger HTTP- och JSON-formatet för Ollama.
- `GameState` ligger under Core-aggregatet `GameSession` och `WorldActionRules` innehåller de första reglerna.

Nästa domänsteg är att utveckla `GameState` till aggregatroten `GameSession` och skriva deterministiska tester. Repository-kontrakt och JSON-implementation införs först tillsammans med faktisk save/load.

#### Regel för nya klasser

Placera ny kod genom att fråga i denna ordning:

1. Avgör koden vad som är sant eller tillåtet i spelvärlden? Lägg den i Core.
2. Pratar koden med Ollama, HTTP, JSON eller filsystemet? Lägg den i Infrastructure bakom en Core-port när en sådan gräns behövs.
3. Läser eller skriver koden terminalen? Lägg den i `RiggsVsReese`.
4. Samordnar koden flera steg i en speltur? Lägg den i en liten application service under `RiggsVsReese/Services`.
5. Finns inget faktiskt behov av en ny klass eller abstraktion? Låt koden ligga kvar tills nästa verifierbara extraktion.

### Game state

C#-objekt under körning, med JSON-fil för sparning.

Ingen databas i den första versionen.

Game state ska innehålla modeller för spelaren och spelarens inventory. Regler och tool calls ska läsa och uppdatera dessa modeller genom spelmotorn.

### Rules engine

En enkel, deterministisk rules engine skriven i vanlig C#-kod.

Rules engine ansvarar för att validera handlingar, avgöra resultat, beräkna skada och uppdatera game state. LLM:n får inte bestämma regelresultat.

### RNG

En seedad PRNG i backend.

Varje match får ett seed, vilket gör resultat möjliga att återskapa och reglerna enklare att testa. Kryptografisk slump kan övervägas senare för onlinespel.

### LLM-integration

LLM:n används som tolkare och berättare.

Den tolkar spelarens naturliga språk till en föreslagen handling och beskriver därefter resultatet som berättelse eller dialog. LLM:n får inte slå tärningar, ändra stats direkt eller bestämma regelresultat.

LLM-integrationen ska kopplas in tidigt eftersom fri språkförståelse och improvisation är centrala delar av spelet. Den ska ändå läggas bakom ett tydligt gränssnitt.

### LLM-provider

Ollama används lokalt under utveckling för att undvika API-kostnader och hålla data på den egna datorn. Den första testmodellen är `gpt-oss:latest`.

LLM-integrationen ska kapslas bakom ett enkelt C#-gränssnitt så att OpenAI senare kan läggas till som ett valbart alternativ utan att ändra spelmotorn.

### Structured output

LLM:n ska använda structured output och tool calling för föreslagna upptäckter, förflyttningar och handlingar. Efter varje spelartext kräver backend ett strukturerat val mellan narrativ respons och ett världsanrop. Tool calls ska ha ett strikt JSON-schema, exempelvis `action`, `target` och `arguments`.

C#-backend validerar först formatet och kontrollerar därefter om handlingen är tillåten i aktuell game state. JSON-schema ersätter inte rules engine och får inte användas för att bestämma regelresultat.

## Ej beslutade delar
