# LanguageModel

## Syfte

Den här mappen innehåller adaptrar för språkmodeller. En adapter översätter mellan spelets enkla kontrakt och det format som en viss språkmodell kräver.

## Känner till och anropas av

Adaptern känner till `ILanguageModelClient` och språkmodellens tekniska protokoll. Den tar emot neutrala meddelanden från `GameTurnService`, gör provideranropet och lämnar tillbaka ett neutralt svar.

## Känner inte till

Den känner inte till spelreglernas innebörd. Ett tool call är bara ett förslag som `GameTurnService` skickar vidare till Core för kontroll.
