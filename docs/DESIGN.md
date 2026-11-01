# Speldesign

## Grundidé

**Riggs vs. Reese: Last One Standing** ska upplevas som ett samtal med en spelledare.

Spelaren ska kunna skriva fritt på naturligt språk. Spelet ska inte presentera en lista med kommandon eller fråga `Vad gör du?` efter varje tur.

LLM:n beskriver världen, svarar på spelarens frågor och tolkar spelarens avsikter. C#-spelmotorn avgör regler, slump, konsekvenser och världens faktiska tillstånd.

## Första mötet med spelet

När en match startar ska spelet inte visa en instruktion som:

```text
Vad gör du?
```

I stället ska spelaren få en inledande scenbeskrivning, exempelvis:

```text
Riggs vs. Reese: Last One Standing

Du är Martin Riggs.

Du vaknar bakom disken i en övergiven bensinstation. Lysrören blinkar i taket
och regnet slår mot de smutsiga fönstren. Någon har nyligen varit här: en varm
kaffekopp står på golvet vid bakdörren.
```

Därefter ska spelaren själv kunna skriva exempelvis:

```text
Jag kollar mig omkring. Vad ser jag?
```

LLM:n svarar naturligt, men får inte anta att objekt eller personer finns utan att spelmotorn har kontrollerat det.

## Dold värld och upptäckt

LLM:n ska inte få en färdig lista över alla objekt, fordon, vapen eller personer som finns på platsen. Det skulle göra världen förutbestämd och få modellen att fylla i detaljer som spelmotorn aldrig har godkänt.

När spelaren frågar eller söker efter något ska LLM:n i stället be spelmotorn om ett resultat.

Exempel:

```text
Spelaren: "Jag kollar efter en bil."
LLM:n: föreslår en sökning efter ett körbart fordon
Spelmotorn: slumpar och avgör om ett fordon finns, var det finns och i vilket skick
LLM:n: beskriver resultatet
```

Samma princip gäller för föremål som gevär, ammunition, verktyg och nycklar. LLM:n får föreslå vad spelarens text betyder, men får inte skapa föremålet själv.

LLM:n kan skicka med en plausibilitetsbedömning, exempelvis `medium` för en bil eller `low` för ett gevär. Det är endast en hypotes eller metadata. Den får inte ersätta eller direkt styra spelmotorns slumpresultat. Spelmotorn använder egna regler och matchens seed för att avgöra utfallet.

Detta gör att spelaren kan försöka göra oväntade saker utan att världen måste innehålla en förbestämd dialogoption för varje möjlighet.

## När spelmotorn ska anropas

LLM:n ska inte använda tool calling för varje meddelande. Den får svara fritt när spelaren samtalar, tänker högt eller ber om en berättande fortsättning utifrån redan kända fakta.

LLM:n måste anropa spelmotorn när spelaren:

- söker efter något
- rör sig till en ny plats
- försöker ta, använda eller påverka något
- angriper, pratar med eller följer någon
- frågar om något som inte redan är känt

Exempel:

```text
"Jag börjar gå längs vägen."
→ travel

"Jag funderar på om Reese kan vara i närheten."
→ fri narrativ respons

"Jag frågar bartendern om Reese."
→ attempt_interaction
```

Första verktygen som exponeras för LLM:n är `respond_narratively` och `inspect_or_search`. Senare läggs `travel` och `attempt_interaction` till.

Efter varje spelartext måste LLM:n välja exakt ett strukturerat verktyg. `respond_narratively` används när ingen världskontroll behövs. `inspect_or_search` används när spelaren undersöker eller söker efter något.

Backend ska kontrollera varje tool call innan spelmotorn kör det. LLM:n får aldrig själv ändra game state eller hoppa över kontrollen genom att skriva vanlig fri text.

## Interaktionsloop

```text
Starta match
→ spelmotorn skapar första game state
→ LLM:n beskriver omgivningen utan att hitta på godkända fakta
→ spelaren skriver fritt
→ LLM:n tolkar spelarens avsikt
→ LLM:n frågar spelmotorn när något måste upptäckas eller prövas
→ spelmotorn validerar, slumpar och genomför eventuell handling
→ LLM:n beskriver vad spelaren upplever
→ spelaren skriver nästa meddelande
```

Spelaren ska kunna blanda frågor, observationer, dialog och handlingar:

- `Vad hör jag bakom dörren?`
- `Jag undersöker kaffekoppen.`
- `Jag ropar: Reese! Jag vet att du är här!`
- `Jag gömmer mig bakom hyllorna.`
- `Jag kollar efter en bil.`
- `Jag plockar upp geväret.`

## LLM:ns ansvar

LLM:n får:

- beskriva omgivningen och stämningen
- tolka fri text till en föreslagen fråga, sökning eller handling
- be spelmotorn kontrollera om något finns eller kan hända
- välja mellan fri narrativ respons och ett begränsat tool call
- skicka en plausibilitetsbedömning som hypotes, utan att bestämma resultatet
- skriva dialog och narrativa konsekvenser efter att motorn räknat ut resultatet
- bevara en naturlig samtalston

LLM:n får inte ensam:

- bestämma slumpresultat
- ändra hälsa, inventory eller stats
- skapa fakta som strider mot game state
- avgöra om en handling lyckas när reglerna kan avgöra det
- anta att ett sökt objekt finns utan att spelmotorn har kontrollerat det
- använda tool calling för att kringgå spelmotorns validering

## C#-motorns ansvar

Spelmotorn är spelets sanningskälla. Den ansvarar för:

- aktuellt game state
- spelarens inventory som en del av game state
- positioner och upptäckta platser
- dolda objekt och deras tillstånd
- stats, hälsa och inventory
- validering av handlingar
- regler, RNG och konsekvenser
- matchens utveckling över tid

Spelmotorn ska kunna svara på generella upptäcktsfrågor utan att varje möjligt objekt behöver vara förprogrammerat som en dialogoption. Den kan exempelvis bedöma om en sökning efter ett fordon, vapen eller verktyg är plausibel i den aktuella miljön och sedan använda RNG för att avgöra vad som faktiskt upptäcks.

## Designprincip

Spelet ska kännas fritt i språket men konsekvent i verkligheten.

**LLM:n beskriver världen. Spelmotorn bestämmer vad som är sant.**

Världen ska kunna ta oväntade riktningar, men varje upptäckt och varje konsekvens måste godkännas av spelmotorn.

## Game context och konsekvenser

LLM:n behöver få den del av game state som är relevant för spelarens aktuella situation, exempelvis:

- om spelaren lever
- hälsa och skador
- inventory
- aktuell plats
- kända personer och upptäckta fakta
- tid och pågående tillstånd

Detta är kända fakta, inte en lista över allt som finns i världen. Dolda objekt och oupptäckta personer måste fortfarande kontrolleras genom spelmotorn.

LLM:n får inte själv göra en konsekvens sann bara för att spelaren skriver den.

```text
"Jag dör."
→ tolka som en avsikt eller önskan
→ spelmotorn kontrollerar om död är möjlig enligt reglerna
→ LLM:n beskriver endast motorns resultat
```

Död får bara inträffa om spelmotorn returnerar ett giltigt dödligt resultat, exempelvis dödlig skada, ett regelgodkänt självmordsförsök eller annan definierad dödsorsak. En ensam narrativ formulering från LLM:n räcker aldrig.

Samma princip gäller för vapen, NPC:er, föremål, resor, skador och andra permanenta konsekvenser.

## Story director och framåtrörelse

Den fria dialogen behöver kompletteras med en story director. Annars riskerar spelet att bara svara på spelarens frågor utan att själv föra berättelsen framåt.

Story director ska arbeta med:

- aktuellt storyläge och spelarens mål
- tid och antal genomförda handlingar
- aktiva hot och pågående trådar
- Reese position, planer och nästa möjliga drag
- ledtrådar och möjliga story beats
- dramatisk spänning

Berättelsen ska inte vara ett fast manus. Den ska i stället bestå av mål, tryck och möjliga händelser som kan uppstå ur spelvärlden.

Exempel:

```text
Riggs undersöker bensinstationen
→ tiden går
→ Reese får möjlighet att förflytta sig eller lämna ett spår
→ story director väljer eller föreslår nästa relevanta beat
→ spelmotorn kontrollerar om beatet kan inträffa
→ LLM:n beskriver händelsen
```

LLM:n kan föreslå att nästa scen innehåller ett avlägset skott, färska däckspår eller en oväntad överlevare. Spelmotorn måste fortfarande avgöra om händelsen är möjlig och vilken konkret form den får.

Story director ska inte avbryta varje liten spelarhandling. Den ska aktiveras när en relevant trigger uppstår, exempelvis efter en viss tid, när spelaren lämnar en plats eller när Reese gör ett drag.

## Kyle Reeses självständiga simulering

Kyle Reese ska simuleras som en egen aktör i världen. Han ska inte flyttas godtyckligt av LLM:n eller bara skapas när berättelsen behöver honom.

Reeses state ska minst innehålla:

- aktuell position
- hälsa, stats och inventory
- aktuellt mål
- möjliga mål och prioriteringar
- vad han vet om Riggs
- senaste kända position för Riggs
- misstanke, ljud- och spårinformation
- nästa planerade handling

Vid relevanta tidssteg väljer spelmotorn Reeses nästa handling från möjliga alternativ, exempelvis:

- förflytta sig
- gömma sig
- undersöka ett spår
- söka utrustning
- bevaka en plats
- lägga ett bakhåll
- följa efter Riggs
- lämna området

Valet påverkas av Reeses stats, mål, tillgänglig information och situation. Seedad RNG används för variation, men får inte ge Reese information han inte rimligen kan ha.

Reese ska inte vara allvetande. Spelmotorn måste hålla isär:

- Reeses faktiska position
- vad Reese tror om Riggs position
- vad Riggs vet om Reese
- vad spelaren faktiskt kan observera

## Förhållandet mellan Riggs och Reese

Spelmotorn beräknar deras relation i världen genom exempelvis:

- avstånd och färdväg
- aktuell plats eller område
- siktlinje och miljö
- ljud och upptäckta spår
- tid sedan senaste observation
- respektive stats och skador

Det gör att Reese kan vara nära utan att Riggs vet det, eller långt bort men följa Riggs spår. När deras tillstånd skapar en observerbar händelse får LLM:n beskriva den, men den får inte själv bestämma att de har sett varandra.

## Simuleringsloop

```text
Riggs gör en handling
→ tiden uppdateras
→ Reese väljer och utför en handling enligt sina stats och mål
→ spelmotorn uppdaterar deras positioner och kunskap
→ story director kontrollerar relevanta triggers
→ LLM:n beskriver endast det Riggs kan uppleva
```

## Slutmål: emergent konfrontation

Spelets mål är att föra Riggs och Reese genom en öppen jakt tills deras positioner och handlingar leder till en konfrontation. Konfrontationen ska inte starta automatiskt bara för att matchen har pågått en viss tid.

De vet inte varandras exakta positioner. Båda vet endast att en mycket farlig, beväpnad motståndare jagar dem.

### Exempelmiljö

Övergivet industriområde på kvällen med lagerbyggnader, containrar, fordon och öppna ytor.

### Karaktärsprofiler

Riggs:

- extrem precision
- mycket bra närstrid
- hög risktolerans
- normal utrustning för karaktären

Reese:

- extrem stridserfarenhet
- bättre fälttaktik
- stark stealth och improvisation
- normal utrustning för karaktären

Ingen karaktär får specialutrustning enbart för att motverka den andra.

### Konfrontationens faser

När deras positioner blir relevanta för varandra ska encounter-systemet kunna gå igenom separata faser:

1. **Positionering** – taktik, stealth, terräng och slump avgör vem som får bättre utgångsläge.
2. **Upptäckt** – separat kontroll för vem som upptäcker vem först.
3. **Första angrepp** – den som får initiativ kan attackera från sitt faktiska läge och skydd.
4. **Träff och försvar** – attack jämförs mot reaktion, skydd och andra relevanta stats.
5. **Träffområde** – separat slump avgör exempelvis torso, arm eller ben.
6. **Skadans allvar** – separat slump avgör om träffen är ytlig, allvarlig eller dödlig.
7. **Fortsatt encounter** – om offret överlever fortsätter simuleringen med uppdaterad hälsa, position och utrustning.

Varje fas ska använda separata, loggade RNG-resultat. LLM:n får inte välja ett resultat för att göra scenen mer dramatisk.

### Ackumulerade fördelar

Fördelar från jakten ska följa med in i encounter-systemet, exempelvis:

- bättre position
- överraskning
- skydd
- information om terrängen
- kända spår
- skador och förlorad utrustning
- initiativ

Det innebär att en konfrontation kan avgöras innan en klassisk eldstrid börjar, men också att ett bra första läge inte garanterar seger. Ett överlevt skott kan fortfarande leda till en längre strid där Riggs precision och närstrid blir avgörande.

### Exempel på önskad simulering

En möjlig match kan börja med att Riggs söker långa siktlinjer medan Reese använder byggnader och stealth. Reese kan vinna positioneringen, upptäcka Riggs först och få första skottet från dold position. Träffområde och skadegrad slås separat. Om skadan inte är dödlig fortsätter matchen med Riggs nya tillstånd.

Det viktiga är att detta är ett möjligt utfall, inte ett manus. Samma regler och ett annat seed ska kunna ge en helt annan konfrontation.

Inventory ska aldrig avgöras av LLM:n eller hårdkodas direkt i ett verktyg. `attempt_interaction` ska läsa spelarens aktuella inventory från game state och uppdatera det endast när rules engine godkänner handlingen.

## Första versionens avgränsning

LLM-integrationen ska finnas med redan i den första vertikala prototypen, eftersom den fria dialogen är en central del av spelet. Vi håller däremot resten av systemet minimalt.

Första prototypen ska innehålla:

1. En provisorisk öppningsscen.
2. Ollama som lokal LLM-provider.
3. LLM-genererad scenbeskrivning.
4. Fri spelartext i terminalen.
5. LLM-svar utifrån scenens kända kontext.
6. En första enkel fråga till spelmotorn när spelaren söker efter något.

Strid, avancerad inventory, NPC-system och full rules engine läggs till stegvis efter att denna dialog- och upptäcktsloop fungerar.
