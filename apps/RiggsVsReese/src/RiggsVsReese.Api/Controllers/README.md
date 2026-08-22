# Controllers

## Möjligt framtida syfte

En controller är MVC:s klassbaserade sätt att ta emot HTTP-anrop. Den kan samla flera närliggande operationer, exempelvis anrop som gäller en match.

## Ska känna till och anropa

Controllern känner till HTTP-formatet och anropar en service för att utföra arbetet. Den känner inte till tekniska LLM-detaljer och innehåller inga spelregler.

## När mappen används

Controllers är ett alternativ till Minimal API-endpoints. Vi väljer ett av sätten om ett webb-API byggs och använder inte båda för samma operation utan ett tydligt skäl.
