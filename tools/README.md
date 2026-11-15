# tools / fetch_talisman_prices.py

Ручной сборщик цен талисманов с официального trade-сайта PoE (3.29+).
Пишет `talisman_prices.json`, который потом читает плагин Beasts (ExileAPI).

Только стандартная библиотека Python 3.10+, зависимостей нет.

## Запуск

```powershell
# из папки плагина
python tools\fetch_talisman_prices.py

# конкретная лига и свой файл
python tools\fetch_talisman_prices.py --league Covens --out tools\talisman_prices.json

# быстрый тест на двух базах
python tools\fetch_talisman_prices.py --only "Farric Talisman,Squid Talisman"

# полный прогон: 5 самых дешёвых лотов на базу + уникальные талисманы
python tools\fetch_talisman_prices.py --top 5 --uniques

# ленивый режим: по одному талисману каждые 20 секунд, бесконечно
python tools\fetch_talisman_prices.py --loop --interval 20
```

Лига по умолчанию определяется автоматически через `/api/trade/data/leagues`
(берётся первый «челлендж»-лиг, не Standard/Hardcore/Ruthless).

Во время прогона печатается прогресс с ETA:

```
Талисманов в очереди: 41 (по 3 самых дешёвых лотов)
[ 5/41]  9.8%  ETA 11.6м  Watcher Talisman ...
      median=0.14c  лотов=3/630  (0.01c..1.00c)  1.9s
...
Готово за 12.4 мин: tools/talisman_prices.json  (лига Covens)
  обновлено: 41, из кэша: 0, ошибок: 0, с ценой: 41/41, курс divine=108.8c
```

## Параметры

| Флаг | Смысл |
|---|---|
| `--league` | имя лиги как в trade API (`Covens`, `Curse of the Allflame`) |
| `--out` | путь к JSON (по умолчанию `talisman_prices.json`) |
| `--only` | список баз через запятую — только они (для тестов) |
| `--top` | сколько самых дешёвых лотов брать на базу (по умолчанию **3**) |
| `--offline` | учитывать оффлайн-лоты (`status=any`) |
| `--uniques` | добавить секцию `unique_talismans` |
| `--include-legacy` | включить `Black Maw` / `Greatwolf` (не дропаются с 3.29) |
| `--resume` | переиспользовать свежие записи из `--out`, докачать прерванный прогон |
| `--max-age` | сколько минут запись считается свежей для `--resume` (по умолчанию 60) |
| `--delay` | доп. пауза после каждого запроса (сек) |
| `--loop` | ленивый режим: по одному талисману за `--interval`, бесконечно |
| `--interval` | пауза между талисманами в `--loop`, сек (по умолчанию 20) |
| `--poesessid`, `--cf-clearance`, `--ua` | переопределить cookies/UA из конфига |

## Конфиг и авторизация

Скопируйте `poe_trade_config.example.json` в `poe_trade_config.json` и заполните:

```json
{
  "league": "Covens",
  "poesessid": "ваш POESESSID",
  "cf_clearance": "",
  "user_agent": "BeastsTalismanPrices/1.0 (contact: your@mail)"
}
```

- **POESESSID не обязателен**: поиск и фетч работают анонимно (лимиты по IP).
  Нужен, если сайт отвечает `403` (Cloudflare) — тогда же может понадобиться `cf_clearance`.
- Cookies берутся из config-файла или переменных окружения `POESESSID` / `CF_CLEARANCE`.
- POESESSID: браузер → DevTools → Application → Cookies → `https://www.pathofexile.com`.
- UA лучше не оставлять дефолтным: GGG просит идентифицировать приложение.

## Лимиты и время

Лимиты берутся из ответов сервера (`x-rate-limit-rules`, `x-rate-limit-<rule>`,
`x-rate-limit-<rule>-state`) и соблюдаются раздельно по каждому правилу.
Анонимно это примерно `5/10s, 15/60s, 30/300s, 600/6h` по IP.

Один талисман = 2 запроса (search + fetch). Полный прогон 41 базы ≈ 82 запроса
≈ 7–14 минут. Скрипт сохраняет JSON после каждой записи, так что прерывание не
страшно: перезапуск с `--resume` продолжит с места остановки.

`429` обрабатывается автоматически (пауза по `Retry-After`), `403/502/5xx` — ретраи.

## Формат JSON

```json
{
  "schema": 1,
  "generated_at": "2026-11-15T10:50:20+00:00",
  "league": "Covens",
  "status": "available",
  "currency_rates": { "chaos": 1.0, "divine": 108.83 },
  "talismans": [
    {
      "base": "Great Maw Talisman",
      "level": 36,
      "enchant": "15% increased Attributes",
      "beast": "Craicic Maw",
      "beast_path": "Metadata/Monsters/LeagueBestiary/FrogBestiary",
      "total": 55,
      "listings_count": 3,
      "min_chaos": 2.0,
      "median_chaos": 10.0,
      "max_chaos": 12.0,
      "by_rarity": {
        "rare": { "count": 9, "min_chaos": 2.0, "median_chaos": 10.0 }
      },
      "listings": [
        {
          "price": 10.0, "currency": "chaos", "chaos": 10.0,
          "rarity": "rare", "ilvl": 78, "corrupted": false,
          "indexed": "2026-11-15T09:01:34Z",
          "account": "Name#1234", "ign": "CharName", "status": "online"
        }
      ],
      "fetched_at": "2026-11-15T10:50:18+00:00",
      "error": null
    }
  ],
  "unique_talismans": []
}
```

Поля, на которые опирается плагин:

- `beast_path` — прямой ключ к `Beast.Path` в `BeastsDatabase`, ничего матчить по имени не надо;
- `median_chaos` / `min_chaos` — цена в хаосе (медиана/пол по самым дешёвым лотам);
- `total` — сколько всего лотов выставлено (ликвидность);
- `by_rarity` — разбивка по редкости (у обычных/магических часто пол 1c, у юников своя цена);
- `fetched_at` — когда снимали, чтобы плагин мог ругаться на устаревшие данные;
- `error` — текст ошибки, если базу не удалось получить (запись при этом остаётся в файле).

## Ограничения, о которых стоит помнить

- Берутся **самые дешёвые онлайн-лоты** (как в APT), а не средняя цена рынка:
  для редких талисманов это фактически «пол» цены, а не стоимость хорошего ролла.
  По умолчанию берутся 3 лота; `--top 5` даёт более устойчивую медиану.
- Цены в divine/exalted/прочих валютах (alch, fusing, mirror, …) конвертируются в хаос
  по курсу poe.ninja (`/poe1/api/economy/exchange/current/overview`, `lines[].primaryValue`).
  Если poe.ninja недоступен, конвертации нет и в лотах остаются `price` + `currency`.
- Юника-талисманы (`--uniques`) ищутся по `name` + `type`; некоторых юников
  в лиге может не быть — тогда `total = 0`, `error = null`.
- Скрипт ничего не отправляет в игру и не логинится: только чтение публичного API.
