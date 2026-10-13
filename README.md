# StockReserve

Mała, ale nietrywialna aplikacja .NET do rezerwowania stanów magazynowych — materiał do webinaru
**„AI pomaga Ci pisać kod. A czy pomaga Ci się uczyć?"** (Szkoła dotNETa × nerds.family).

Aplikacja jest celowo „napisana z AI": działa, testy są zielone, a mimo to ma kilka bugów, które
wychodzą dopiero przy współbieżności, upływie czasu albo powtórzonym requeście. Zadanie: znaleźć je
z pomocą AI, **zrozumieć**, udowodnić testem i naprawić.

## Co jest w środku

| Element | Gdzie | Po co |
|---|---|---|
| Minimal API: produkty, rezerwacje, confirm/cancel | `src/StockReserve.Api/Features` | zwykły CRUD z logiką domenową (`OnHand`, `Available`, TTL) |
| `ExpiryWorker` + `ExpirySweeper` | `src/StockReserve.Api/Workers` | `BackgroundService` na `PeriodicTimer` + `TimeProvider`, zwalnia przeterminowane rezerwacje |
| `NotificationWorker` + `Channel<ReservationConfirmed>` | `src/StockReserve.Api/Workers`, `Program.cs` | asynchroniczna kolejka w procesie (`System.Threading.Channels`) |
| `IFraudScreening`, `INotificationSink` | `src/StockReserve.Api/Infrastructure` | udawane zależności zewnętrzne z opóźnieniem — szwy do testów |
| EF Core + SQLite | `src/StockReserve.Api/Data` | prawdziwa baza, prawdziwe blokady, zero mocków |
| Testy integracyjne | `tests/StockReserve.Tests` | xUnit + `WebApplicationFactory`, własny plik SQLite na test, `TestClock`, `RecordingNotificationSink` |
| Tutorzy AI | `CLAUDE.md`, `.claude/agents`, `.claude/commands`, `docs/PROMPTY.md` | tryb tutora, review z dowodami, trener reprodukcji, mapa konsekwencji, quiz |

## Uruchomienie

```bash
dotnet build
dotnet test
dotnet run --project src/StockReserve.Api      # http://localhost:5080
```

Requesty do klikania: `src/StockReserve.Api/StockReserve.http` (Rider / VS Code REST Client).
W trybie Development baza jest seedowana trzema produktami, a TTL rezerwacji to 1 minuta.

Wymagany .NET SDK 10. Jeśli masz tylko 9: zmień `net10.0` → `net9.0` w obu `.csproj` i `10.0.*` → `9.0.*`.

## API

| Metoda | Ścieżka | Opis |
|---|---|---|
| `GET` | `/products?page=1&pageSize=20` | lista produktów (stronicowana) |
| `GET` | `/products/{id}` | produkt |
| `POST` | `/products` | `{ sku, name, onHand }` |
| `POST` | `/reservations` | `{ productId, quantity, customerEmail }` → 201 / 404 / 409 / 403 |
| `GET` | `/reservations/{id}` | rezerwacja |
| `POST` | `/reservations/{id}/confirm` | potwierdza i zdejmuje `OnHand` |
| `POST` | `/reservations/{id}/cancel` | anuluje i oddaje `Available` |
| `POST` | `/admin/sweep` | uruchamia sweep przeterminowanych rezerwacji od ręki |
| `GET` | `/health` | |

Niezmiennik domeny: `0 <= Available <= OnHand`. Rezerwacja zdejmuje `Available`, confirm zdejmuje `OnHand`,
cancel i wygaśnięcie oddają `Available`.

## Jak się tym uczyć

1. `dotnet test` — wszystko zielone. Zielone testy ≠ poprawny kod.
2. Uruchom aplikację i **zanim** wyślesz `GET /products`, zapisz, co zwróci.
3. W Claude Code: `/review całe repo` → wybierz jedną uwagę → `/repro <hipoteza>` → czerwony test → fix → zielony → `/co-jesli <twoja zmiana>`.
4. `./demo/race.sh` — 20 równoległych rezerwacji jedynej sztuki. Policz dwieście-jedynki.
5. `/quiz Features/Reservations/ReservationService.cs` — sprawdź, czy rozumiesz, co naprawiłeś.

Gałąź `solutions` zawiera testy reprodukujące każdy bug i poprawki — ale najpierw spróbuj sam.
