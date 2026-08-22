# RiggsVsReese.IntegrationTests

## Framtida syfte

Integration tests kontrollerar att flera delar fungerar tillsammans. Här ska vi exempelvis testa HTTP-kommunikation med Ollama, JSON-översättning, framtida filsystemslagring och att den körbara appen kopplar ihop rätt implementationer.

## Ska känna till och avgränsas från

Testerna känner till de riktiga tekniska gränserna och kan därför vara långsammare eller kräva att ett externt system är igång. De ska inte användas för varje liten spelregel; sådana tester hör hemma i UnitTests.
