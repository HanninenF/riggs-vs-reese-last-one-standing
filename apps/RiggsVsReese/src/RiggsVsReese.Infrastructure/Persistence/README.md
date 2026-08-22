# Persistence

## Framtida syfte

Persistence betyder att data överlever efter att programmet stängts. Den här mappen ska senare innehålla den tekniska koden för att spara och läsa en match med JSON och filsystemet.

## Ska känna till och göra

Koden kommer att känna till Core-kontraktet för lagring och det konkreta filformatet. Den tar emot en `GameSession`, översätter den till lagringsformat och skriver filen.

## Ska inte göra

Den får inte ändra matchens regler eller hitta på nytt state. Save/load är inte implementerat ännu, så mappen innehåller bara dokumentation.
