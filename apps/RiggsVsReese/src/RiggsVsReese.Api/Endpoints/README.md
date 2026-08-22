# Endpoints

## Framtida syfte

En endpoint är ingången för ett enda HTTP-anrop, exempelvis ”starta match” eller ”skicka spelarkommando”. Den läser anropet, kontrollerar att formatet är giltigt och omvandlar resultatet till ett HTTP-svar.

## Ska känna till och anropa

Endpointen känner till HTTP-formatet och den service som utför användningsfallet. Den anropar servicen för arbetet och får därigenom hjälp av Core.

## Ska inte känna till

Endpointen känner inte till Ollamas JSON-format och innehåller inga spelregler. Mappen används bara om Minimal API väljs för ett framtida webb-API.
