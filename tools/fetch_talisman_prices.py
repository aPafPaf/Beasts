#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Выгрузка цен талисманов с официального trade-сайта Path of Exile (3.29+).

Запускается вручную, когда нужно обновить данные. Результат — JSON, который потом
читает плагин Beasts (ExileAPI). Зависимостей нет, только стандартная библиотека.

Примеры:
    python tools/fetch_talisman_prices.py
    python tools/fetch_talisman_prices.py --league Covens
    python tools/fetch_talisman_prices.py --only "Farric Talisman,Squid Talisman"
    python tools/fetch_talisman_prices.py --uniques --top 5
    python tools/fetch_talisman_prices.py --resume            # докачать прерванный прогон

POESESSID нужен редко: поиск/фетч работают и без логина, но с ним лимиты выше.
Если сайт отвечает 403 (Cloudflare) — добавьте POESESSID и/или cf_clearance:
    set POESESSID=...            (cmd)
    $env:POESESSID="..."         (PowerShell)
или положите их в poe_trade_config.json рядом со скриптом.
"""

from __future__ import annotations

import argparse
import json
import os
import statistics
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from collections import deque
from datetime import datetime, timedelta, timezone

TRADE_HOST = "https://www.pathofexile.com"
DEFAULT_UA = "BeastsTalismanPrices/1.0 (ExileAPI plugin helper; contact: set me)"
CONFIG_NAME = "poe_trade_config.json"
SCHEMA = 1

# base, level, enchant, beast display name (как в BeastsDatabase), metadata path зверя
TALISMANS: list[tuple[str, int, str, str | None, str | None]] = [
    ("Chieftain Talisman", 1, "16% increased Area of Effect", "Farric Chieftain", "Metadata/Monsters/LeagueBestiary/BestiaryMonkeyChiefBlood"),
    ("Chimeral Talisman", 1, "30% increased Projectile Speed", "Saqawine Chimeral", "Metadata/Monsters/LeagueBestiary/IguanaBestiary"),
    ("Devourer Talisman", 1, "Damage Penetrates 15% Fire Resistance", "Fenumal Devourer", "Metadata/Monsters/LeagueBestiary/RootSpiderBestiary_"),
    ("Gargantuan Talisman", 1, "15% increased maximum Life", "Farric Gargantuan", "Metadata/Monsters/LeagueBestiary/BeastCaveBestiary"),
    ("Goatman Talisman", 1, "Hits ignore Enemy Physical Damage Reduction", "Farric Goatman", "Metadata/Monsters/LeagueBestiary/GoatmanLeapSlamBestiary"),
    ("Goliath Talisman", 1, "Projectiles Pierce 3 additional Targets", "Farric Goliath", "Metadata/Monsters/LeagueBestiary/BestiarySpiker"),
    ("Plagued Arachnid Talisman", 1, "35% increased Effect of Withered", "Fenumal Plagued Arachnid", "Metadata/Monsters/LeagueBestiary/SpiderPlagueBestiary"),
    ("Rhoa Talisman", 1, "Gain 15% of Maximum Life as Extra Armour", "Saqawine Rhoa", "Metadata/Monsters/LeagueBestiary/RhoaBestiary"),
    ("Sand Spitter Talisman", 1, "12% increased Movement Speed", "Craicic Sand Spitter", "Metadata/Monsters/LeagueBestiary/SandSpitterBestiary"),
    ("Savage Crab Talisman", 1, "Damage Penetrates 15% Cold Resistance", "Craicic Savage Crab", "Metadata/Monsters/LeagueBestiary/CrabParasiteLargeBestiary_"),
    ("Shield Crab Talisman", 1, "30% increased Global Defences", "Craicic Shield Crab", "Metadata/Monsters/LeagueBestiary/ShieldCrabBestiary"),
    ("Squid Talisman", 1, "30% increased maximum Mana", "Craicic Squid", "Metadata/Monsters/LeagueBestiary/SeaWitchSpawnBestiary"),
    ("Watcher Talisman", 1, "Damage Penetrates 15% Lightning Resistance", "Craicic Watcher", "Metadata/Monsters/LeagueBestiary/SquidBestiary"),
    ("Wolf Alpha Talisman", 1, "+40% to Global Critical Strike Multiplier", "Farric Wolf Alpha", "Metadata/Monsters/LeagueBestiary/WolfBestiary"),
    ("Ape Talisman", 28, "+1 to Minimum Endurance, Frenzy and Power Charges", "Farric Ape", "Metadata/Monsters/LeagueBestiary/MonkeyBloodBestiary"),
    ("Black Widow Talisman", 28, "Utility Flasks gain 2 Charges every 3 seconds", "Fenumal Widow", "Metadata/Monsters/LeagueBestiary/Spider5Bestiary"),
    ("Blood Viper Talisman", 28, "20% increased Cooldown Recovery Rate", "Saqawine Blood Viper", "Metadata/Monsters/LeagueBestiary/SnakeBestiary2"),
    ("Carrion Queen Talisman", 28, "+1 to maximum number of Spectres", "Fenumal Queen", "Metadata/Monsters/LeagueBestiary/InsectSpawnerBestiary"),
    ("Cobra Talisman", 28, "+1 to Maximum Frenzy Charges", "Saqawine Cobra", "Metadata/Monsters/LeagueBestiary/SnakeBestiary1"),
    ("Flame Hellion Talisman", 28, "+4% to maximum Fire Resistance", "Farric Flame Hellion Alpha", "Metadata/Monsters/LeagueBestiary/HellionBestiary"),
    ("Frost Hellion Talisman", 28, "+4% to maximum Cold Resistance", "Farric Frost Hellion Alpha", "Metadata/Monsters/LeagueBestiary/HellionBestiary2"),
    ("Lynx Talisman", 28, "+4% to maximum Lightning Resistance", "Farric Lynx Alpha", "Metadata/Monsters/LeagueBestiary/LynxBestiary"),
    ("Magma Hound Talisman", 28, "Unaffected by Ignite", "Farric Magma Hound", "Metadata/Monsters/LeagueBestiary/HoundBestiary"),
    ("Pitbull Talisman", 28, "Warcries Exert 1 additional Attack", "Farric Pit Hound", "Metadata/Monsters/LeagueBestiary/PitbullBestiary"),
    ("Retch Talisman", 28, "+1 to Maximum Power Charges", "Saqawine Retch", "Metadata/Monsters/LeagueBestiary/KiwethBestiary"),
    ("Scorpion Talisman", 28, "+20% to Damage over Time Multiplier", "Fenumal Scorpion", "Metadata/Monsters/LeagueBestiary/BlackScorpionBestiary"),
    ("Scrabbler Talisman", 28, "+2 to Level of all Herald Skill Gems", "Fenumal Scrabbler", "Metadata/Monsters/LeagueBestiary/SandLeaperBestiary"),
    ("Taurus Talisman", 28, "+1 to Maximum Endurance Charges", "Farric Taurus", "Metadata/Monsters/LeagueBestiary/BestiaryBull"),
    ("Ursa Talisman", 28, "30% increased Effect of your Marks", "Farric Ursa", "Metadata/Monsters/LeagueBestiary/DropBearBestiary"),
    ("Croaker Talisman", 36, "+1 to Level of all Skill Gems", "Craicic Croaker", "Metadata/Monsters/LeagueBestiary/GemFrogBestiary"),
    ("Great Maw Talisman", 36, "15% increased Attributes", "Craicic Maw", "Metadata/Monsters/LeagueBestiary/FrogBestiary"),
    ("Hybrid Arachnid Talisman", 36, "Minions have +30% to Damage over Time Multiplier", "Fenumal Hybrid Arachnid", "Metadata/Monsters/LeagueBestiary/SpiderPlatedBestiary"),
    ("Octopus Talisman", 36, "20% chance to Freeze Enemies for 1 second when they Hit you", "Craicic Vassal", "Metadata/Monsters/LeagueBestiary/ParasiticSquidBestiary"),
    ("Rhex Talisman", 36, "100% of Cold and Lightning Damage from Hits taken as Fire Damage", "Saqawine Rhex", "Metadata/Monsters/LeagueBestiary/Avians/MarakethBirdBestiary"),
    ("Spider Crab Talisman", 36, "+15% to Quality of all Skill Gems", "Craicic Spider Crab", "Metadata/Monsters/LeagueBestiary/CrabSpiderBestiary"),
    ("Tiger Talisman", 36, "8% increased Action Speed", "Farric Tiger Alpha", "Metadata/Monsters/LeagueBestiary/TigerBestiary"),
    ("Vulture Talisman", 36, "Skills fire an additional Projectile", "Saqawine Vulture", "Metadata/Monsters/LeagueBestiary/VultureBestiary"),
    ("Craicic Talisman", 44, "100% increased Aspect of the Crab Buff Effect", "Craiceann, First of the Deep", "Metadata/Monsters/LeagueBestiary/NessaCrabBestiarySpiritBoss"),
    ("Farric Talisman", 44, "100% increased Aspect of the Cat Buff Effect", "Farrul, First of the Plains", "Metadata/Monsters/LeagueBestiary/TigerBestiarySpiritBoss"),
    ("Fenumal Talisman", 44, "100% increased Aspect of the Spider Buff Effect", "Fenumus, First of the Night", "Metadata/Monsters/LeagueBestiary/SpiderPlatedBestiarySpiritBoss"),
    ("Saqawine Talisman", 44, "100% increased Aspect of the Avian Buff Effect", "Saqawal, First of the Sky", "Metadata/Monsters/LeagueBestiary/MarakethBirdSpiritBoss"),
    # legacy bases: не выпадают с 3.29, берём только по флагу --include-legacy
    ("Black Maw Talisman", 1, "Has 1 Socket", None, None),
    ("Greatwolf Talisman", 1, "44% increased Damage per Moon Rite completed", None, None),
]

UNIQUE_TALISMANS: list[tuple[str, str]] = [
    ("Rigwald's Curse", "Wolf Alpha Talisman"),
    ("Natural Hierarchy", "Rhex Talisman"),
    ("The Blightwell", "Shield Crab Talisman"),
    ("Eyes of the Greatwolf", "Greatwolf Talisman"),
    ("Night's Hold", "Black Maw Talisman"),
]


def now_iso() -> str:
    return datetime.now(timezone.utc).isoformat(timespec="seconds")


def log(msg: str) -> None:
    print(msg, flush=True)


class RateLimiter:
    """
    Раздельные скользящие окна под каждое правило сервера
    (x-rate-limit-<rule>: max:window:penalty, состояние — x-rate-limit-<rule>-state).
    Каждый запрос учитывается во всех правилах, но окна не смешиваются.
    """

    def __init__(self, safety: float = 2.0):
        self.rules: dict[tuple[int, int], deque[float]] = {}
        self.safety = safety

    def note(self) -> None:
        stamp = time.time()
        for window_log in self.rules.values():
            window_log.append(stamp)

    def sync(self, headers) -> None:
        names = headers.get("x-rate-limit-rules")
        if not names:
            return
        now = time.time()
        for name in names.split(","):
            name = name.strip().lower()
            limit_hdr = headers.get(f"x-rate-limit-{name}")
            if not limit_hdr:
                continue
            state_hdr = (headers.get(f"x-rate-limit-{name}-state") or "").split(",")
            for idx, part in enumerate(limit_hdr.split(",")):
                nums = part.split(":")
                if len(nums) < 2:
                    continue
                try:
                    max_hits, window = int(nums[0]), int(nums[1])
                except ValueError:
                    continue
                if max_hits <= 0 or window <= 0:
                    continue
                window_log = self.rules.setdefault((max_hits, window), deque())
                try:
                    used = int(state_hdr[idx].split(":")[0])
                except (IndexError, ValueError):
                    used = 0
                # сервер видит и чужую нагрузку по нашему IP — добиваем до его счётчика
                local = sum(1 for t in window_log if now - t < window + self.safety)
                for _ in range(max(0, used - local)):
                    window_log.append(now)

        for (_, window), window_log in self.rules.items():
            while window_log and now - window_log[0] > window + self.safety:
                window_log.popleft()

    def wait(self) -> None:
        if not self.rules:
            time.sleep(0.2)
            return
        while True:
            now = time.time()
            sleep_for = 0.0
            for (max_hits, window), window_log in self.rules.items():
                hits = [t for t in window_log if now - t < window + self.safety]
                if len(hits) >= max_hits:
                    sleep_for = max(sleep_for, hits[0] + window + self.safety - now)
            if sleep_for <= 0:
                return
            if sleep_for > 3:
                log(f"    rate limit: ждём {sleep_for:.0f}s")
            time.sleep(min(sleep_for + 0.05, 300))


class TradeClient:
    def __init__(self, league: str, poesessid: str | None, cf_clearance: str | None,
                 ua: str, delay: float):
        self.league = league
        self.league_q = urllib.parse.quote(league)
        self.poesessid = poesessid
        self.cf_clearance = cf_clearance
        self.ua = ua
        self.delay = max(0.0, delay)
        self.limiter = RateLimiter()

    def _cookies(self) -> str:
        parts = []
        if self.poesessid:
            parts.append(f"POESESSID={self.poesessid}")
        if self.cf_clearance:
            parts.append(f"cf_clearance={self.cf_clearance}")
        return "; ".join(parts)

    def request(self, method: str, path: str, payload=None, retries: int = 4):
        url = TRADE_HOST + path
        body = None if payload is None else json.dumps(payload).encode("utf-8")

        for attempt in range(1, retries + 1):
            self.limiter.wait()
            req = urllib.request.Request(url, data=body, method=method)
            req.add_header("User-Agent", self.ua)
            req.add_header("Accept", "application/json")
            req.add_header("Accept-Encoding", "identity")
            if body is not None:
                req.add_header("Content-Type", "application/json")
            cookies = self._cookies()
            if cookies:
                req.add_header("Cookie", cookies)

            try:
                with urllib.request.urlopen(req, timeout=30) as resp:
                    self.limiter.note()
                    self.limiter.sync(resp.headers)
                    raw = resp.read().decode("utf-8", "replace")
                if self.delay:
                    time.sleep(self.delay)
                return json.loads(raw)

            except urllib.error.HTTPError as exc:
                self.limiter.note()
                self.limiter.sync(exc.headers)
                text = exc.read().decode("utf-8", "replace")[:300]
                if exc.code == 429:
                    retry_after = exc.headers.get("retry-after")
                    wait = float(retry_after) if retry_after else 60.0
                    log(f"    429 Too Many Requests, пауза {wait:.0f}s")
                    time.sleep(wait)
                    continue
                if exc.code in (403, 502, 503, 520, 522) and attempt < retries:
                    log(f"    HTTP {exc.code}, повтор через {8 * attempt}s")
                    time.sleep(8 * attempt)
                    continue
                hint = ""
                if exc.code == 403 and not self.poesessid:
                    hint = " (похоже на Cloudflare: задайте POESESSID/cf_clearance в poe_trade_config.json)"
                raise RuntimeError(f"HTTP {exc.code}: {text}{hint}") from None

            except (urllib.error.URLError, TimeoutError, json.JSONDecodeError) as exc:
                if attempt < retries:
                    log(f"    сеть/JSON: {exc}, повтор через {5 * attempt}s")
                    time.sleep(5 * attempt)
                    continue
                raise RuntimeError(f"network error: {exc}") from None

        raise RuntimeError("исчерпаны попытки запроса")


def detect_league(client: TradeClient) -> str:
    data = client.request("GET", "/api/trade/data/leagues")
    ids = [entry.get("id", "") for entry in data.get("result", [])]
    skip = ("standard", "hardcore", "ruthless", "ssf", "event")
    for league_id in ids:
        low = league_id.lower()
        if any(word in low for word in skip):
            continue
        if league_id:
            return league_id
    return ids[0] if ids else "Standard"


def http_get_json(url: str, ua: str) -> dict:
    req = urllib.request.Request(url, method="GET")
    req.add_header("User-Agent", ua)
    req.add_header("Accept", "application/json")
    req.add_header("Accept-Encoding", "identity")
    with urllib.request.urlopen(req, timeout=30) as resp:
        return json.loads(resp.read().decode("utf-8", "replace"))


def get_currency_rates(client: TradeClient) -> dict[str, float]:
    """
    Курсы к хаосу. Основной источник — poe.ninja exchange overview
    (core.rates[x] = сколько единиц x за 1 chaos, поэтому курс = 1 / rate).
    Если poe.ninja недоступен — хаос остаётся единственным курсом, а лоты
    в других валютах сохранятся как есть (amount + currency).
    """
    rates: dict[str, float] = {"chaos": 1.0}
    league = client.league
    slugs = [
        league,
        league.replace(" ", ""),
        league.lower().replace(" ", ""),
        league.lower().replace(" ", "-"),
    ]

    payload = None
    for slug in dict.fromkeys(slugs):
        url = ("https://poe.ninja/poe1/api/economy/exchange/current/overview"
               f"?league={urllib.parse.quote(slug)}&type=Currency")
        try:
            payload = http_get_json(url, client.ua)
        except (urllib.error.URLError, urllib.error.HTTPError, TimeoutError, json.JSONDecodeError):
            continue
        if payload.get("core", {}).get("rates"):
            break
        payload = None

    if not payload:
        log("  poe.ninja недоступен — конвертация только chaos")
        return rates

    core = payload.get("core") or {}
    if core.get("primary") != "chaos":
        return rates

    # lines[].primaryValue — сколько хаоса стоит 1 единица валюты
    for line in payload.get("lines") or []:
        currency_id = line.get("id")
        primary = line.get("primaryValue")
        if currency_id and primary:
            try:
                rates[currency_id] = round(float(primary), 4)
            except (TypeError, ValueError):
                continue

    # core.rates — обратный курс (сколько единиц за 1 chaos), добиваем недостающее
    for currency_id, value in (core.get("rates") or {}).items():
        if not value or currency_id in rates:
            continue
        try:
            rates[currency_id] = round(1.0 / float(value), 4)
        except (TypeError, ZeroDivisionError):
            continue
    return rates


def to_chaos(amount: float, currency: str, rates: dict[str, float]) -> float | None:
    rate = rates.get(currency)
    if rate is None:
        return None
    return amount * rate


def request_listings(client: TradeClient, base: str, name: str | None,
                     offline: bool, top: int) -> tuple[int, list[dict]]:
    query: dict = {
        "query": {
            "status": {"option": "any" if offline else "available"},
            "stats": [{"type": "and", "filters": []}],
            "filters": {
                "type_filters": {"filters": {"category": {"option": "accessory.amulet"}}},
            },
        },
        "sort": {"price": "asc"},
    }
    query["query"]["type"] = base
    if name:
        query["query"]["name"] = name

    search = client.request("POST", f"/api/trade/search/{client.league_q}", query)
    if "error" in search:
        raise RuntimeError(f"search error: {search['error'].get('message')}")

    query_id = search.get("id")
    results = search.get("result") or []
    total = int(search.get("total") or 0)
    if not query_id or not results:
        return total, []

    listings: list[dict] = []
    wanted = results[:max(1, top)]
    for start in range(0, len(wanted), 10):
        chunk = wanted[start:start + 10]
        if not chunk:
            break
        fetched = client.request(
            "GET", f"/api/trade/fetch/{','.join(chunk)}?query={query_id}"
        )
        for entry in fetched.get("result") or []:
            if entry:
                listings.append(entry)
    return total, listings


RARITY_NAMES = {0: "normal", 1: "magic", 2: "rare", 3: "unique", 4: "relic", 5: "gem", 6: "currency", 7: "divination"}


def summarize(entry: dict, rates: dict[str, float]) -> dict:
    item = entry.get("item") or {}
    listing = entry.get("listing") or {}
    price = listing.get("price") or {}
    account = listing.get("account") or {}
    online = account.get("online") or {}
    amount = float(price.get("amount") or 0)
    currency = price.get("currency") or "unknown"
    chaos = to_chaos(amount, currency, rates) if amount else None
    return {
        "price": round(amount, 2),
        "currency": currency,
        "chaos": round(chaos, 2) if chaos is not None else None,
        "rarity": RARITY_NAMES.get(item.get("frameType"), "unknown"),
        "ilvl": item.get("ilvl"),
        "corrupted": bool(item.get("corrupted")),
        "indexed": listing.get("indexed"),
        "account": account.get("name"),
        "ign": account.get("lastCharacterName"),
        "status": online.get("status", "online" if online else "offline"),
    }


def build_entry(base: str, level: int, enchant: str, beast: str | None,
                beast_path: str | None, name: str | None,
                client: TradeClient, rates: dict[str, float],
                offline: bool, top: int) -> dict:
    stamp = now_iso()
    record = {
        "base": base,
        "level": level,
        "enchant": enchant,
        "beast": beast,
        "beast_path": beast_path,
        "total": 0,
        "listings_count": 0,
        "min_chaos": None,
        "median_chaos": None,
        "max_chaos": None,
        "currency": "chaos",
        "by_rarity": {},
        "listings": [],
        "fetched_at": stamp,
        "error": None,
    }
    if name:
        record["name"] = name

    try:
        total, raw_listings = request_listings(client, base, name, offline, top)
    except RuntimeError as exc:
        record["error"] = str(exc)
        return record

    listings = [summarize(entry, rates) for entry in raw_listings]
    prices = sorted(l["chaos"] for l in listings if l["chaos"])
    record["total"] = total
    record["listings_count"] = len(listings)
    record["listings"] = listings

    by_rarity: dict[str, dict] = {}
    for rarity in ("normal", "magic", "rare", "unique"):
        rarity_prices = sorted(l["chaos"] for l in listings
                               if l["rarity"] == rarity and l["chaos"])
        if rarity_prices:
            by_rarity[rarity] = {
                "count": len(rarity_prices),
                "min_chaos": round(rarity_prices[0], 2),
                "median_chaos": round(statistics.median(rarity_prices), 2),
            }
    record["by_rarity"] = by_rarity

    if prices:
        record["min_chaos"] = round(prices[0], 2)
        record["max_chaos"] = round(prices[-1], 2)
        record["median_chaos"] = round(statistics.median(prices), 2)
    return record


def load_config(path: str) -> dict:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as handle:
            return json.load(handle)
    except (OSError, json.JSONDecodeError) as exc:
        log(f"config: не смог прочитать {path}: {exc}")
        return {}


def load_previous(path: str) -> dict[str, dict]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as handle:
            data = json.load(handle)
    except (OSError, json.JSONDecodeError):
        return {}
    previous: dict[str, dict] = {}
    for entry in data.get("talismans") or []:
        if isinstance(entry, dict) and entry.get("base"):
            previous[entry["base"]] = entry
    for entry in data.get("unique_talismans") or []:
        if isinstance(entry, dict) and entry.get("name"):
            previous[f"unique:{entry['name']}"] = entry
    return previous


def is_fresh(entry: dict, max_age_min: int) -> bool:
    if max_age_min <= 0 or entry.get("error") or not entry.get("median_chaos"):
        return False
    try:
        stamp = datetime.fromisoformat(entry["fetched_at"])
    except (KeyError, ValueError):
        return False
    return datetime.now(timezone.utc) - stamp < timedelta(minutes=max_age_min)


def save(path: str, payload: dict) -> None:
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as handle:
        json.dump(payload, handle, ensure_ascii=False, indent=2)
        handle.write("\n")
    os.replace(tmp, path)


def fmt(value) -> str:
    return "—" if value is None else f"{value:.2f}c"


def format_rates(rates: dict[str, float]) -> str:
    keys = ("chaos", "divine", "exalted", "alch")
    parts = [f"{key}={rates[key]}" for key in keys if key in rates]
    return ", ".join(parts) + f" (всего валют: {len(rates)})"


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Цены талисманов с pathofexile.com/trade → JSON для плагина Beasts",
    )
    parser.add_argument("--league", help="лига как в trade API (по умолчанию — текущая)")
    parser.add_argument("--out", default="talisman_prices.json", help="куда писать JSON")
    parser.add_argument("--poesessid", help="cookie POESESSID (иначе env POESESSID / config)")
    parser.add_argument("--cf-clearance", help="cookie cf_clearance при Cloudflare")
    parser.add_argument("--config", default=CONFIG_NAME, help="файл конфига рядом со скриптом")
    parser.add_argument("--top", type=int, default=3,
                        help="сколько самых дешёвых лотов брать на базу (по умолчанию 3)")
    parser.add_argument("--offline", action="store_true", help="учитывать оффлайн-лоты (status=any)")
    parser.add_argument("--uniques", action="store_true", help="добавить юника-талисманы")
    parser.add_argument("--include-legacy", action="store_true", help="включить Black Maw / Greatwolf")
    parser.add_argument("--only", help="через запятую: только эти базы (для теста)")
    parser.add_argument("--resume", action="store_true", help="переиспользовать свежие записи из --out")
    parser.add_argument("--max-age", type=int, default=60, help="минут, сколько запись считается свежей")
    parser.add_argument("--delay", type=float, default=0.0, help="доп. пауза после каждого запроса, сек")
    parser.add_argument("--ua", help="свой User-Agent (по умолчанию из config/DEFAULT_UA)")
    parser.add_argument("--loop", action="store_true",
                        help="ленивый режим: по одному талисману за --interval, бесконечно")
    parser.add_argument("--interval", type=float, default=20.0,
                        help="пауза между талисманами в --loop, сек (по умолчанию 20)")
    return parser.parse_args(argv)


def run_loop(args, client: TradeClient, selected: list[tuple], rates: dict[str, float],
             payload: dict, previous: dict[str, dict]) -> int:
    """Ленивый режим: один талисман за тик, файл перезаписывается после каждого."""
    records: dict[str, dict] = {t[0]: previous[t[0]] for t in selected if t[0] in previous}
    payload["talismans"] = [records[t[0]] for t in selected if t[0] in records]

    if args.uniques:
        payload["unique_talismans"] = [previous[f"unique:{name}"]
                                       for name, _ in UNIQUE_TALISMANS
                                       if f"unique:{name}" in previous]

    log(f"Ленивый режим: 1 талисман каждые {args.interval:.0f}s, "
        f"полный круг ~{len(selected) * (args.interval + 20) / 60:.0f} мин. Ctrl+C — стоп.")
    last_rates = time.time()
    cycle = 0
    try:
        while True:
            cycle += 1
            started = time.time()
            for index, (base, level, enchant, beast, beast_path) in enumerate(selected, start=1):
                if time.time() - last_rates > 30 * 60:
                    try:
                        rates = get_currency_rates(client)
                        payload["currency_rates"] = rates
                        last_rates = time.time()
                        log("  курсы валют обновлены: " + format_rates(rates))
                    except Exception as exc:  # noqa: BLE001 - курс не критичен
                        log(f"  курсы валют: {exc}")

                tick = time.time()
                record = build_entry(base, level, enchant, beast, beast_path, None,
                                     client, rates, args.offline, args.top)
                records[base] = record
                payload["currency_rates"] = rates
                payload["generated_at"] = now_iso()
                payload["talismans"] = [records[t[0]] for t in selected if t[0] in records]
                save(args.out, payload)

                state = "ошибка: " + record["error"] if record["error"] else (
                    f"median={fmt(record['median_chaos'])} (лотов {record['total']})")
                log(f"  {datetime.now().strftime('%H:%M:%S')} круг {cycle} "
                    f"[{index}/{len(selected)}] {base}: {state}")

                time.sleep(max(0.0, args.interval - (time.time() - tick)))

            if args.uniques:
                for name, base in UNIQUE_TALISMANS:
                    record = build_entry(base, 0, "", None, None, name,
                                         client, rates, args.offline, args.top)
                    payload["unique_talismans"] = [
                        r for r in payload["unique_talismans"] if r.get("name") != name
                    ] + [record]
                    save(args.out, payload)
                    log(f"  {datetime.now().strftime('%H:%M:%S')} {name}: "
                        + ("ошибка: " + record["error"] if record["error"]
                           else f"median={fmt(record['median_chaos'])}"))
                    time.sleep(args.interval)

            log(f"Круг {cycle} завершён за {(time.time() - started) / 60:.1f} мин.")

    except KeyboardInterrupt:
        log("")
        log("Остановлено пользователем, сохраняю файл.")
        save(args.out, payload)
        return 0


def main(argv: list[str]) -> int:
    try:
        sys.stdout.reconfigure(encoding="utf-8")  # type: ignore[attr-defined]
    except (AttributeError, ValueError):
        pass

    args = parse_args(argv)
    config = load_config(args.config)

    poesessid = args.poesessid or os.environ.get("POESESSID") or config.get("poesessid")
    cf_clearance = args.cf_clearance or os.environ.get("CF_CLEARANCE") or config.get("cf_clearance")
    ua = args.ua or config.get("user_agent") or DEFAULT_UA
    league = args.league or config.get("league")

    bootstrap = TradeClient(league or "Standard", poesessid, cf_clearance, ua, args.delay)
    if not league:
        league = detect_league(bootstrap)
        log(f"Лига: {league}")
    client = TradeClient(league, poesessid, cf_clearance, ua, args.delay)
    if not poesessid:
        log("POESESSID не задан — работаем анонимно (лимиты по IP, возможен 403 от Cloudflare)")

    log("Получаю курсы валют (divine/exalted → chaos)...")
    rates = get_currency_rates(client)
    log("  курсы: " + format_rates(rates))

    selected = TALISMANS
    if args.only:
        wanted = {part.strip().lower() for part in args.only.split(",") if part.strip()}
        selected = [t for t in TALISMANS if t[0].lower() in wanted]
        if not selected:
            log("--only: ничего не совпало")
            return 2
    elif not args.include_legacy:
        selected = [t for t in TALISMANS if t[3] is not None]

    previous = load_previous(args.out) if args.resume else {}
    payload = {
        "schema": SCHEMA,
        "generated_at": now_iso(),
        "league": league,
        "status": "any" if args.offline else "available",
        "currency_rates": rates,
        "talismans": [],
        "unique_talismans": [],
    }

    if args.loop:
        if not previous:
            previous = load_previous(args.out)
        return run_loop(args, client, selected, rates, payload, previous)

    total = len(selected)
    started = time.time()
    refreshed = 0
    from_cache = 0
    errors = 0

    def progress(index: int, label: str, done_total: int, t0: float) -> str:
        done = index - 1
        elapsed = time.time() - t0
        eta = (elapsed / done) * (done_total - done) if done else 0.0
        return (f"[{index:>2}/{done_total}] {done / done_total * 100:5.1f}%  "
                f"ETA {eta / 60:4.1f}м  {label}")

    log(f"Талисманов в очереди: {total} (по {args.top} самых дешёвых лотов)")
    for index, (base, level, enchant, beast, beast_path) in enumerate(selected, start=1):
        cached = previous.get(base)
        if args.resume and cached and is_fresh(cached, args.max_age):
            payload["talismans"].append(cached)
            from_cache += 1
            log(progress(index, f"{base}: из кэша {fmt(cached.get('median_chaos'))}", total, started))
            save(args.out, payload)
            continue

        log(progress(index, f"{base} ...", total, started))
        tick = time.time()
        record = build_entry(base, level, enchant, beast, beast_path, None,
                             client, rates, args.offline, args.top)
        payload["talismans"].append(record)
        spent = time.time() - tick
        if record["error"]:
            errors += 1
            log(f"      ошибка: {record['error']}  ({spent:.1f}s)")
        else:
            refreshed += 1
            log(f"      median={fmt(record['median_chaos'])}  "
                f"лотов={record['listings_count']}/{record['total']}  "
                f"({fmt(record['min_chaos'])}..{fmt(record['max_chaos'])})  {spent:.1f}s")
        save(args.out, payload)

    if args.uniques:
        u_total = len(UNIQUE_TALISMANS)
        log(f"Юника-талисманы: {u_total}")
        for index, (name, base) in enumerate(UNIQUE_TALISMANS, start=1):
            key = f"unique:{name}"
            cached = previous.get(key)
            if args.resume and cached and is_fresh(cached, args.max_age):
                payload["unique_talismans"].append(cached)
                from_cache += 1
                save(args.out, payload)
                continue
            log(progress(index, f"{name} ({base}) ...", u_total, started))
            tick = time.time()
            record = build_entry(base, 0, "", None, None, name,
                                 client, rates, args.offline, args.top)
            payload["unique_talismans"].append(record)
            spent = time.time() - tick
            if record["error"]:
                errors += 1
                log(f"      ошибка: {record['error']}  ({spent:.1f}s)")
            else:
                refreshed += 1
                log(f"      median={fmt(record['median_chaos'])}  "
                    f"лотов={record['listings_count']}/{record['total']}  {spent:.1f}s")
            save(args.out, payload)

    payload["generated_at"] = now_iso()
    save(args.out, payload)

    priced = [t for t in payload["talismans"] if t.get("median_chaos")]
    log("")
    log(f"Готово за {(time.time() - started) / 60:.1f} мин: {args.out}  (лига {league})")
    log(f"  обновлено: {refreshed}, из кэша: {from_cache}, ошибок: {errors}, "
        f"с ценой: {len(priced)}/{len(payload['talismans'])}, "
        f"курс divine={rates.get('divine', '?')}c")
    for record in sorted(priced, key=lambda r: r["median_chaos"], reverse=True)[:10]:
        log(f"  {record['base']:<28} {fmt(record['median_chaos']):>9}  "
            f"(лотов {record['total']})")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
