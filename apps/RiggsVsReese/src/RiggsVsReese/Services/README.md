# Services

## Syfte

Services ansvarar för att samordna ett helt arbetsflöde. `GameTurnService` håller ordning på meddelandehistoriken och ser till att rätt steg sker i rätt ordning under en speltur.

## Känner till och tar hjälp av

`GameTurnService` känner till Core-kontrakt, `GameState` och Core-regler. Den tar hjälp av `ILanguageModelClient` för att tolka eller formulera text och av `WorldActionRules` för att kontrollera vad som faktiskt är tillåtet i spelet.

## Känner inte till

Servicen känner inte till att språkmodellen är Ollama och arbetar inte direkt med HTTP eller JSON. Den läser inte heller från terminalen; `GameLoop` skickar in spelarens kommando och visar sedan servicens svar.

## Skillnad mot Core

En service bestämmer alltså ordningen mellan flera delar, medan Core fortfarande bestämmer regelutfallet.
