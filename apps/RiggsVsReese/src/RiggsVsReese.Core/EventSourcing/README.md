# Event sourcing

## Möjligt framtida syfte

Event sourcing innebär att varje förändring sparas som en händelse, exempelvis `PlayerMoved` eller `ItemPickedUp`, i stället för att endast spara det senaste state-värdet. Händelserna kan sedan spelas upp för att återskapa matchen.

## Gräns

Mappen är reserverad men funktionen är inte beslutad. Den ska inte känna till terminalen eller en viss databas; teknisk lagring skulle ligga i Infrastructure.

## När den blir aktiv

Vi inför detta först om spelet behöver full historik, replay eller undo. Det finns därför inga commands, events eller event stores här ännu.
