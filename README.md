# Практическая работа № 4 (СРВ): канал передачи данных с временными ограничениями

Консольное приложение на C# (.NET 8). По набору сообщений и параметрам канала
рассчитывает время передачи, задержку доставки, время доставки при приоритетном
арбитраже, джиттер и проверяет выполнение дедлайнов.

## Структура

- `src/` — исходный код (`Models.cs`, `Calculations.cs`, `DataLoader.cs`, `Program.cs`)
- `data/` — входные данные варианта (`channel.csv`, `messages.csv`)


## Запуск

```bash
dotnet run
dotnet run -- data/messages.csv data/channel.csv
```

## Формат данных

`data/channel.csv`: `bitrate_bps`, `propagation_delay_ms`, `processing_delay_ms`.

`data/messages.csv`: `name,priority,size_bits,period_ms,deadline_ms`
(приоритет 1 — высший; пустой `deadline_ms` = период).

## Ветки

- `main` — стабильная версия, только готовые релизы
- `develop` — интеграционная ветка
- `feature/*` — разработка отдельных частей
- `docs/*` — документация и отчёт
