# RiggsVsReese

## Syfte

Det här är terminalappen som användaren faktiskt startar. Den tar emot spelarens text, visar spelets svar och samordnar en speltur.

## Känner till och anropar

`Program.cs` känner till de konkreta delar som måste skapas och kopplar ihop dem. Därefter lämnar den över till `GameLoop`, som bara känner till terminalens in- och utmatning och anropar `GameTurnService` när något ska göras.

## Hör inte hemma här

Projektet använder Core för state och regler samt Infrastructure för tekniska implementationer. Spelregler, Ollama-anrop och JSON-hantering ska inte skrivas direkt i terminalkoden.
