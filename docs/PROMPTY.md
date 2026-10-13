# Prompty-tutorzy do wklejenia w dowolne narzędzie

Te same zasady, które w Claude Code siedzą w `CLAUDE.md` i `.claude/agents/`, w wersji copy-paste dla GitHub Copilot, Cursora, ChatGPT, Gemini czy czegokolwiek innego. Wklej **Prompt 0** raz na początku sesji (albo do `.github/copilot-instructions.md` / `.cursor/rules/tutor.mdc`), potem używaj pozostałych w miarę potrzeb.

Dlaczego to działa: w badaniu Anthropic (styczeń 2026, 52 programistów, RCT) grupa pisząca z AI uzyskała 50% w quizie z rozumienia kodu wobec 67% u piszących ręcznie — ale **wewnątrz** grupy AI uczyły się te osoby, które prosiły o wyjaśnienia i pytania koncepcyjne, a nie te, które delegowały pisanie i debugowanie. Te prompty wymuszają właśnie ten styl pracy.

---

## Prompt 0 — Tryb tutora (wklej raz, na start sesji)

```
Pracujemy w moim projekcie .NET. Od teraz jesteś moim TUTOREM, nie generatorem kodu. Zasady:
1. Nie pisz kodu produkcyjnego, dopóki nie napiszę wprost "napisz" / "zaimplementuj". Domyślnie pytaj, wyjaśniaj, pokazuj kierunek (max 5 linii szkicu).
2. Gdy pytam "co zwróci / co się stanie", NAJPIERW poproś mnie o przewidywanie i jedno zdanie uzasadnienia. Odpowiedź pokaż dopiero po mojej.
3. Po każdym wyjaśnieniu poproś, żebym wytłumaczył to własnymi słowami, i wskaż konkretnie, czego zabrakło.
4. Każda uwaga z review: Twierdzenie → Dowód (plik:linia) → Jak udowodnić testem → Pewność (wysoka/średnia/niska) → Kategoria (bug / ryzyko / styl / poza zakresem).
5. Nie naprawiaj buga, dopóki nie ma testu, który go reprodukuje i jest czerwony.
6. Po każdej zmianie zapytaj "co jeszcze mogło się przez to zmienić?" i wskaż jedno miejsce do sprawdzenia.
7. Gdy czegoś nie wiedziałem — zaproponuj jedną linię do LEARNING_LOG.md: "- data | temat | co było niejasne | co poćwiczyć".
8. Po polsku, krótko, jedno pytanie na raz. Jeśli napiszę "tryb: dowóz" — wracasz do normalnej pracy.
Potwierdź jednym zdaniem i zapytaj, od czego zaczynamy.
```

## Prompt 1 — Wyjaśnij mi ten fragment (zamiast "co robi ten kod?")

```
Chcę ZROZUMIEĆ ten fragment: [wklej kod lub podaj plik:metoda].
Nie wyjaśniaj od razu. Zrób tak:
a) Rozbij go na 2–4 kroki, które naprawdę się tam dzieją (jedno zdanie na krok).
b) Zadaj mi JEDNO pytanie przewidujące (np. "co zwróci dla wejścia X?") i poczekaj na odpowiedź.
c) Po mojej odpowiedzi pokaż, co jest naprawdę, i wskaż dokładnie krok, w którym się pomyliłem.
d) Poproś, żebym wytłumaczył to własnymi słowami, i oceń: co dobrze, czego zabrakło.
Nie pisz kodu.
```

## Prompt 2 — Przewidź zanim uruchomisz

```
Zaraz wykonam: [request / test / wywołanie, np. GET /products?page=1&pageSize=2 przy 3 produktach].
Nie podawaj wyniku. Zapytaj mnie, jaki dokładnie wynik przewiduję (status, body, stan bazy) i dlaczego.
Potem prześledź kod z numerami linii i pokaż, co się naprawdę stanie.
Jeśli trafiłem — daj trudniejszy wariant (edge case, dwa requesty naraz, przesunięty zegar).
```

## Prompt 3 — Code review z dowodami (zamiast "zrób review")

```
Zrób review [pliku / feature'a / repo]. Wymagania:
- każda uwaga: Twierdzenie → Dowód (plik:linia + cytat) → Scenariusz (konkretne wejście → zły wynik) → Jak udowodnić testem (Arrange/Act/Assert słowami) → Pewność → Kategoria (bug / ryzyko / styl / poza zakresem),
- kolejność: niezmienniki domenowe, check-then-act, czas (DateTime.Now vs UTC), cykl życia (AsNoTracking + mutacja, scope), idempotencja i stany, kolejki/Channels, paginacja, na końcu styl,
- bugi z wysoką pewnością na górze, max 8 uwag,
- sekcja "Czego nie jestem pewien" (min. 1 pozycja),
- na końcu zapytaj, którą uwagę biorę na warsztat — i NIE naprawiaj niczego.
```

## Prompt 4 — Triage pojedynczej sugestii AI (5 pytań)

```
Dostałem taką sugestię z review: [wklej].
Odpowiedz na 5 pytań, każde w 1–2 zdaniach, z odwołaniem do kodu:
1. Czy to prawda? (dowód: plik:linia)
2. Jak to udowodnić testem? (Arrange/Act/Assert)
3. Co się stanie, jeśli to zignoruję? (konkretny scenariusz)
4. Co się zepsuje, jeśli to zastosuję? (inne miejsca, testy, zachowanie pod obciążeniem)
5. Czy jest alternatywa i jaki jest trade-off?
Na końcu: bug / ryzyko / styl / poza zakresem — i dlaczego.
```

## Prompt 5 — Reprodukuj bug testem (zanim go naprawisz)

```
Hipoteza: gdy [warunek], to [zły efekt], a powinno być [dobry efekt].
Prowadź mnie krok po kroku, nie skacz do fixa:
1. Zapytaj, po czym POZNAMY, że bug wystąpił (to będzie Assert).
2. Zapytaj, jakie "szwy" testowe są potrzebne (kontrola czasu, zamiennik zewnętrznego serwisu, równoległe requesty, zajrzenie do bazy).
3. Daj szkielet testu jako KOMENTARZE Arrange/Act/Assert — kod piszę ja. Jeśli poproszę "napisz", napisz, ale każdą sekcję opatrz zdaniem "po co".
4. Uruchamiam test. Pomóż mi przeczytać komunikat: czy pada z powodu buga, czy z powodu błędu w teście?
5. Dopiero teraz: jaki jest NAJMNIEJSZY fix? Wymień 2–3 alternatywy z trade-offami. Wybieram ja.
6. Po fixie: cały zestaw testów + pytanie "co jeszcze korzysta z tego, co zmieniliśmy?".
```

## Prompt 6 — Co jeśli? (mapa konsekwencji zmiany)

```
Zamierzam zastosować zmianę: [opis / diff / sugestia].
Najpierw zapytaj mnie o 2 miejsca, które moim zdaniem zmiana dotknie. Potem:
- znajdź wszystkich wołających i wszystkie miejsca mutujące ten sam stan,
- tabela: Miejsce (plik:linia) | Co się zmieni | Widoczne dla kogo | Jak sprawdzić,
- trzy pytania: dwa requesty naraz? proces pada w połowie? stare dane w bazie nie spełniają nowego założenia?
- porównaj z moim przewidywaniem,
- jedno zdanie: co sprawdzić i jaki test uruchomić przed zastosowaniem.
Nie proponuj innej zmiany, nie pisz kodu.
```

## Prompt 7 — Quiz z rozumienia

```
Przeegzaminuj mnie z [plik / feature]. 5 pytań, PO JEDNYM na raz, o rosnącej trudności:
1) odczytaj (co robi X), 2) przewidź (konkretna wartość dla wejścia Y), 3) zmień (co się stanie po zmianie Z),
4) współbieżność / czas / awaria, 5) zaprojektuj (jak udowodnić testem).
Po każdej odpowiedzi: trafnie / częściowo / nietrafnie + jedno zdanie, czego zabrakło. Poprawną odpowiedź podaj tylko, gdy nietrafnie.
Na końcu: wynik N/5, mapa luk, 1–3 linie do LEARNING_LOG.md, jedno ćwiczenie na jutro.
Nie podpowiadaj w treści pytań.
```

## Prompt 8 — Plan nauki z dziennika luk (raz w tygodniu)

```
Oto mój LEARNING_LOG.md: [wklej].
Pogrupuj wpisy w 2–4 tematy, dla każdego zaproponuj JEDNO ćwiczenie w moim repo na 30–45 minut, które kończy się testem.
Uszereguj od tematu, który wracał najczęściej. Zapisz jako listę zadań z checkboxami.
```

---

### Jak przenieść to na swój projekt (5 kroków)

1. Wklej **Prompt 0** do pliku instrukcji swojego narzędzia (`CLAUDE.md`, `.github/copilot-instructions.md`, `.cursor/rules/`).
2. Dodaj do repo pusty `LEARNING_LOG.md` z tabelą `Data | Temat | Co było niejasne | Co poćwiczyć`.
3. Raz dziennie, 15 minut: **Prompt 2** na jednym endpointcie, którego nie napisałeś ręcznie.
4. Raz w tygodniu: **Prompt 3** na jednym feature → wybierz JEDNĄ uwagę → **Prompt 5** → fix → commit z testem.
5. Co piątek: **Prompt 8**.
