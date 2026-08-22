# GameSession

## Syfte

Den här mappen representerar en pågående match. Nuvarande `GameState` innehåller spelaren och spelarens inventory och är början på den framtida aggregatroten `GameSession`.

## Känner till och tar hjälp av

`GameSession` ska känna till allt state som krävs för en match, exempelvis spelare, Reese, världstillstånd och slumpens seed. Den får ta hjälp av regler i `Rules` för att avgöra resultat.

## Känner inte till

Den känner inte till terminalen, Ollama eller JSON. Andra delar av programmet ska fråga eller be `GameSession` om förändringar i stället för att själva bestämma vad som händer.
