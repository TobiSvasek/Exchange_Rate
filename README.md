# Exchange Rate Sprint

Toto je konzolová aplikace v C#, která získává data o produktech z databáze SQL Server a zapisuje je do souboru CSV. Soubor CSV obsahuje názvy produktů, ceny v USD a ceny v CZK. Směnný kurz z USD na CZK je načítán z webových stránek České národní banky.

## Začínáme

Pro spuštění této aplikace budete potřebovat:

- .NET 5.0 nebo novější
- Přístup k databázi SQL Server

## Použití

Aplikaci spustíte následujícím příkazem:

```bash
dotnet run
```
Ve výchozím nastavení aplikace používá aktuální datum pro načtení směnného kurzu a pojmenování souboru CSV. Můžete zadat jiné datum tím, že ho předáte jako argument ve formátu dd.MM.yyyy.

## Autor

- Tobiáš Svášek - (https://github.com/TobiSvasek)
- [1143@student.itgymnazium.cz](mailto:1143@student.itgymnazium.cz)
