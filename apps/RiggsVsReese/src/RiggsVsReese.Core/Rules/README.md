# Rules

## Syfte

Här ligger regler som räknar ut vad en handling leder till. `WorldActionRules` gör just nu den första kontrollen för sökning och interaction; senare kan mappen innehålla regler för upptäckt, strid, skada och seedad slump.

## Känner till och returnerar

Reglerna känner till Core-modeller som `GameState` och får läsa det state som behövs för beräkningen. De returnerar ett tydligt resultat till anroparen.

## Känner inte till

Reglerna känner inte till terminalen, Ollama, HTTP eller filsystemet. Samma input och samma seed ska ge samma resultat.
