---
description: Mapa konsekwencji zmiany zanim ją zastosujesz (co jeszcze się zmieni, kto to zobaczy, jak sprawdzić)
argument-hint: <opis zmiany lub sugestia z review, np. "dodać IsConcurrencyToken na Product.Available">
---

Użyj subagenta `what-if` dla zmiany: $ARGUMENTS

Najpierw zapytaj mnie o moje przewidywanie (2 miejsca, które zmiana dotknie), potem zbuduj tabelę konsekwencji (plik:linia | co się zmieni | widoczne dla kogo | jak sprawdzić), zadaj trzy pytania (dwa requesty naraz / proces pada w połowie / stare dane w bazie) i zakończ jednym zdaniem: co sprawdzić i jaki test uruchomić przed zastosowaniem.
