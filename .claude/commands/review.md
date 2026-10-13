---
description: Code review z dowodami i kategoriami (bug / ryzyko / styl / poza zakresem) — bez poprawek
argument-hint: <ścieżka, feature albo "całe repo">
---

Użyj subagenta `reviewer` do przeglądu: $ARGUMENTS

Wymagania:
- każda uwaga w formacie: Twierdzenie → Dowód (plik:linia) → Scenariusz → Jak udowodnić testem → Pewność → Kategoria,
- bugi z wysoką pewnością na górze, maksymalnie 8 uwag,
- sekcja „Czego nie jestem pewien",
- na końcu pytanie: którą uwagę biorę na warsztat i dlaczego — i STOP. Żadnych poprawek w kodzie.
