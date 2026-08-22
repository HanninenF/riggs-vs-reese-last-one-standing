# Persistence abstractions

## Framtida syfte

Den här mappen är för framtida kontrakt som beskriver hur en match kan sparas och läsas tillbaka. Ett möjligt kontrakt är `IGameSessionRepository`.

## Ska känna till

Kontraktet ska bara känna till `GameSession` och operationer som spara och hämta. Det ska inte känna till JSON, filnamn eller mappar på datorn.

## Ska implementeras av

En implementation i Infrastructure kommer senare att följa kontraktet och göra det faktiska arbetet mot filsystemet.
