# RiggsVsReese.Core

## Syfte

Core är spelets sanningskälla. Här finns game state och reglerna som avgör vad som finns, vad en karaktär får göra och vad resultatet blir.

## Känner till

Core känner bara till spelets egna modeller, regler och gränssnitt. Det känner inte till terminalen, Ollama, HTTP, JSON eller filsystemet.

## Anropas av

`GameTurnService` ber Core att kontrollera spelarens handlingar. Core svarar med ett regelresultat men skriver inte ut text och kontaktar inga externa system.
