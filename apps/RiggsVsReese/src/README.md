# Produktionskod

## Syfte

Här ligger all kod som används när spelet körs. Koden är uppdelad i tre delar:

- `RiggsVsReese` startar programmet, läser terminalen och samordnar en speltur.
- `RiggsVsReese.Core` innehåller game state och bestämmer vad som är tillåtet i spelet.
- `RiggsVsReese.Infrastructure` pratar med Ollama och andra tekniska system.

## Beroendegräns

Delarna hjälper varandra genom tydliga anrop. Core känner inte till terminalen eller Infrastructure, vilket gör spelreglerna lättare att förstå och testa.
