# Aggregates

## Syfte

Ett aggregate samlar state som hör ihop och ansvarar för att det alltid förblir giltigt. I spelet är en hel pågående match den naturliga första gränsen.

## Känner till och tar hjälp av

Aggregatet känner till sina egna delar, exempelvis spelaren och inventory, och kan ta hjälp av rena Core-regler. Det känner inte till terminalen, språkmodellen eller hur matchen sparas.

## Används av

Övrig kod ska be aggregatet utföra en förändring i stället för att ändra dess inre state fritt.
