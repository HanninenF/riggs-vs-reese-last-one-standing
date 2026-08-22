# RiggsVsReese.UnitTests

## Framtida syfte

Unit tests kontrollerar en liten del kod i taget. Här ska vi främst testa `GameSession`, `GameState` och regler i Core med kända värden och kända förväntade resultat.

## Ska känna till och undvika

Testerna känner till Core men ska normalt inte starta terminalen, anropa Ollama eller läsa riktiga filer. Därför blir de snabba och kan tydligt visa vilken regel som är fel.
