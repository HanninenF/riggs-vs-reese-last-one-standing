# Så hänger `GameLoop` och `GameTurnService` ihop

De här två klasserna arbetar tillsammans, men de har olika ansvar:

- `GameLoop` pratar med terminalen.
- `GameTurnService` genomför en speltur.

Läs dem därför som ett par. `GameLoop` startar och upprepar flödet, medan `GameTurnService` utför arbetet som behövs mellan spelarens text och det färdiga svaret.

## Ansvarsfördelning

### `GameLoop`

Fil:

`apps/RiggsVsReese/src/RiggsVsReese/GameLoop.cs`

`GameLoop` känner till:

- terminalens inmatning
- terminalens utmatning
- `GameTurnService`

`GameLoop` känner inte till:

- hur LLM-anropet fungerar
- vilka tool calls som finns
- hur inventory eller vapen fungerar
- om en spelhandling är tillåten

### `GameTurnService`

Fil:

`apps/RiggsVsReese/src/RiggsVsReese/Services/GameTurnService.cs`

`GameTurnService` känner till:

- `GameState`
- språkmodellens kontrakt
- meddelandehistoriken
- tool calls och deras argument
- vilka Core-regler som ska anropas

Den skriver inte direkt till terminalen. Den returnerar ett svar till `GameLoop`.

## Första gången programmet körs

`Program.cs` skapar en `GameTurnService` och skickar den till `GameLoop`. Sedan anropas `GameLoop.RunAsync()`.

Första flödet ser ut så här:

```text
Program.cs
  → GameLoop.RunAsync()
  → GameTurnService.StartAsync()
  → GameTurnService.ContinueAsync()
  → BuildMessagesForRequest()
  → BuildGameStateContext()
  → ILanguageModelClient.SendAsync()
  → svar tillbaka
  → GameLoop.WriteResponse()
```

### Vad gör `StartAsync()`?

`StartAsync()` är en tunn startmetod. Den innehåller inte en separat speltur, utan skickar arbetet vidare till `ContinueAsync()`.

Det gör att både den första scenen och vanliga spelarhandlingar kan använda samma fortsättningslogik.

## `GameLoop.RunAsync()` steg för steg

`RunAsync()` gör två saker i ordning:

1. Hämtar och skriver ut den första scenen.
2. Läser spelarens kommandon i en loop.

Förenklat:

```text
StartAsync()
  → WriteResponse()

så länge ReadPlayerCommand() returnerar text:
  → RespondAsync(command)
  → WriteResponse()
```

Om `ReadPlayerCommand()` returnerar `null` avslutas loopen. Det händer när spelaren:

- skriver `/exit`
- skriver en tom rad
- bara skriver blanksteg

`GameLoop` avgör alltså när programmet ska fortsätta eller avslutas, men inte vad spelarens kommando betyder.

## Vad händer när spelaren skriver text?

Anta att spelaren skriver:

```text
Jag undersöker bensinstationen.
```

Flödet blir:

```text
Console.ReadLine()
  → GameLoop.ReadPlayerCommand()
  → GameTurnService.RespondAsync(command)
  → _messages.Add(user message)
  → ContinueAsync()
```

`RespondAsync()` lägger först spelarens text i meddelandehistoriken. Därefter börjar samma fortsättningslogik som användes för den första scenen.

## Vad gör `ContinueAsync()`?

`ContinueAsync()` är en loop eftersom en speltur ibland kräver flera LLM-anrop.

Ett LLM-anrop kan till exempel säga:

```text
Jag behöver först undersöka vad som finns på platsen.
```

I stället för att direkt skriva ett färdigt svar kan modellen välja tool call `inspect_or_search`. Då måste programmet:

1. läsa tool call
2. kontrollera argumenten
3. anropa Core-regeln
4. lägga regelresultatet i meddelandehistoriken
5. fråga LLM:n igen

Därför ser den förenklade logiken ut så här:

```text
skicka meddelanden till LLM
  ↓
fick vi tool calls?
  ├── nej → returnera narrativt svar
  └── ja
        ↓
      bearbeta tool calls
        ↓
      lägg resultaten i meddelandehistoriken
        ↓
      fråga LLM igen
```

## Första anropet och senare anrop

Servicen använder `_firstRequest` för att skilja på första scenen och senare spelturer.

### Första anropet

Första anropet får inte kräva ett tool call. Modellen ska skriva en vanlig öppningsscen.

```text
requireToolCall = false
```

Efter öppningsscenen läggs instruktionen om framtida verktyg till i meddelandehistoriken.

### Senare anrop

Efter den första scenen måste modellen välja exakt ett verktyg:

```text
requireToolCall = true
```

Det hindrar modellen från att hitta på spelvärldens resultat utan att först gå via spelreglerna.

## Meddelandehistoriken och aktuell state

`_messages` innehåller den långsiktiga konversationen:

- systeminstruktioner
- spelarens text
- modellens svar
- tool calls
- tool-resultat

Inför varje LLM-anrop skapas dessutom en aktuell state-snapshot genom:

```text
BuildMessagesForRequest()
  → BuildGameStateContext()
```

Snapshoten läser aktuellt state, exempelvis:

- utrustat vapen
- laddat magasin
- antal skott
- patron i kammaren
- vapnets skick
- reservmagasin

Den aktuella snapshoten skickas med i anropet men läggs inte permanent i `_messages`. Därför kan inventory och ammunition ändras utan att gammal information ligger kvar som sanning.

## Tool calls och deras vägar

Efter att modellen returnerat tool calls skickas de vidare så här:

```text
ProcessToolCalls()
  → ProcessToolCall()
      ├── respond_narratively
      │     → ProcessNarrativeCall()
      ├── inspect_or_search
      │     → ProcessInspectionCall()
      │     → WorldActionRules.InspectOrSearch()
      └── attempt_interaction
            → ProcessInteractionCall()
            → WorldActionRules.AttemptInteraction()
```

### `respond_narratively`

Detta är modellens färdiga berättande svar. Servicen kontrollerar att argumentet `text` finns och returnerar sedan texten till `GameLoop`.

### `inspect_or_search`

Servicen kontrollerar att argumentet `query` finns. Därefter anropas `WorldActionRules.InspectOrSearch()` och resultatet läggs till som ett tool-resultat.

Eftersom LLM:n behöver se resultatet fortsätter `ContinueAsync()` med ett nytt LLM-anrop.

### `attempt_interaction`

Servicen kontrollerar att både `action` och `target` finns. Därefter anropas `WorldActionRules.AttemptInteraction()` med aktuellt `GameState`.

Core avgör om handlingen är tillåten. Servicen avgör bara att rätt regel ska anropas och skickar tillbaka resultatet till modellen.

## Exempel: ett undersökningskommando

```text
Spelaren skriver kommando
  → GameLoop läser texten
  → GameTurnService.RespondAsync()
  → spelartexten läggs i _messages
  → aktuell GameState-snapshot byggs
  → LLM väljer inspect_or_search
  → ProcessInspectionCall()
  → WorldActionRules.InspectOrSearch()
  → tool-resultat läggs i _messages
  → ContinueAsync() gör nästa LLM-anrop
  → LLM skriver berättande svar
  → GameTurnService returnerar texten
  → GameLoop skriver texten i terminalen
```

## Vad ska ändras var?

När du arbetar med dessa klasser kan du använda följande tumregel:

- Ändra `GameLoop` när terminalens in- eller utmatning ändras.
- Ändra `GameTurnService` när ordningen på en speltur eller tool-dispatchen ändras.
- Ändra `GameState` eller Core-regler när en spelregel ändras.
- Ändra `OllamaChatClient` när HTTP- eller JSON-formatet ändras.

Om en regel börjar hamna i `GameLoop` eller i `GameTurnService` är det ofta ett tecken på att den borde flyttas till Core.

