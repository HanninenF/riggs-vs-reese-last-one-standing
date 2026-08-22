# Persistence repositories

## Framtida syfte

Ett repository ger resten av spelet enkla operationer som ”spara match” och ”hämta match” och gömmer hur lagringen fungerar.

## Ska känna till och implementera

En framtida `JsonGameSessionRepository` ska känna till Core-kontraktet, JSON-formatet och filsystemet. Den implementerar kontraktet från Core och anropas genom det, inte genom sin konkreta klasstyp.

## Ska inte känna till

Repositoryt känner inte till terminalen eller hur en speltur fungerar. Ingen implementation finns ännu eftersom save/load inte har byggts.
