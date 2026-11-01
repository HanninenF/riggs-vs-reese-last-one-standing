# GameSession

## Syfte

Den här mappen representerar en pågående match. Nuvarande `GameState` innehåller spelaren och spelarens inventory och är början på den framtida aggregatroten `GameSession`.

## Vapen och magasin

Vapen och magasin ska behandlas som egna delar av spelmodellen, inte som lösa generiska items. Spelaren har ett utrustat vapen med ett laddat magasin, medan reservmagasin ligger i inventory.

Exempel på relationen:

```text
PlayerState
├── EquippedWeapon: Beretta 92F
│   └── LoadedMagazine: 15 skott
└── Inventory
    └── ExtraMagazine: 15 skott
```

Det gör det möjligt för aggregatet att skydda regler för att skjuta, ladda om, byta magasin och kontrollera aktuell ammunition. Ett generiskt item passar fortfarande för enkla föremål som inte har egen spelmekanik.

## Definitioner och runtime-tillstånd

Vapenmodellen i `Core/Items/Weapons` skiljer på vad ett vapen är och vad som händer med ett konkret exemplar under matchen:

- `WeaponDefinition` och `MagazineDefinition` innehåller fasta egenskaper.
- `Weapon` och `Magazine` innehåller matchens föränderliga tillstånd.
- `WeaponCatalog` är registret över vilka vapendefinitioner som finns.

Två Beretta 92F kan därför använda samma vapendefinition men ha olika skick, kammarläge eller magasin. Den konkreta `Weapon`-instansen äger sitt laddade `Magazine`, medan ett extra magasin ska vara ett separat konkret magasin i spelarens inventory.

## Känner till och tar hjälp av

`GameSession` ska känna till allt state som krävs för en match, exempelvis spelare, Reese, världstillstånd och slumpens seed. Den får ta hjälp av regler i `Rules` för att avgöra resultat.

Den ska också äga relationen mellan spelarens utrustade vapen, laddade magasin och lösa reservföremål. All state-förändring som påverkar dessa regler ska gå genom aggregatet eller rena Core-regler som aggregatet använder.

## Känner inte till

Den känner inte till terminalen, Ollama eller JSON. Andra delar av programmet ska fråga eller be `GameSession` om förändringar i stället för att själva bestämma vad som händer.
