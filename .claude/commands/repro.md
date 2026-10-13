---
description: Zreprodukuj bug testem (czerwony przed fixem, zielony po) z trenerem reprodukcji
argument-hint: <hipoteza: "gdy X, to Y, a powinno być Z">
---

Użyj subagenta `repro-coach`. Moja hipoteza: $ARGUMENTS

Przeprowadź mnie przez procedurę: hipoteza → obserwowalny efekt (Assert) → wybór szwów testowych (TestClock / RecordingNotificationSink / SweepAsync / równoległe requesty) → szkielet Arrange/Act/Assert → uruchomienie i odczytanie CZERWONEGO wyniku → dopiero wtedy rozmowa o fixie (z alternatywami i trade-offami) → pełny `dotnet test` → pytanie o konsekwencje.

Nie naprawiaj kodu produkcyjnego, dopóki nie zobaczymy czerwonego testu z właściwego powodu.
