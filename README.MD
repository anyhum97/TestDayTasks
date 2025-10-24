# MapLib

MapLib — высокопроизводительный модуль для работы с тайловыми картами, объектами и регионами, с поддержкой Redis и MagicOnion для многопользовательской работы в реальном времени.  

---

## Подключение к Redis
localhost:6379

## Основные возможности

### Тайловая карта
- Создание карты и хранение всех типов тайлов.
- Доступ и изменение тайла по координатам за **O(1)**.
- Проверка выхода за границы карты.
- Эффективное использование памяти (данные занимают ≤ 8 МБ).
- Методы:
  - `GetTile(int x, int y)` — получение тайла.
  - `SetTile(int x, int y, Tile tile)` — установка тайла.
  - `FillArea(int startX, int startY, int width, int height, Tile tile)` — заполнение области.
  - `CanPlaceInArea(int startX, int startY, int width, int height)` — проверка возможности размещения объекта.

### Управление объектами
- Добавление и удаление объектов с уведомлением подписчиков:
  - События: `ObjectAdded`, `ObjectRemoved`, `ObjectUpdated`.
- Хранение объектов в Redis с географическими координатами:
  - `TryAddObject(MapObject obj)` — добавление объекта.
  - `TryRemoveObject(int id)` — удаление объекта.
  - `FirstOrDefaultByPosition(Position position, double radius)` — поиск объекта по координатам.
  - `GetAllObjectsInArea(int x, int y, int radius)` — получение всех объектов в области.
- Проверка вхождения объекта в область:
  - `IntersectsArea(int id, int x, int y, int width, int height)`.

### Работа с регионами (территориями)
- Генерация регионов одинаковой площади.
- Быстрое получение ID региона по тайлу и метаданных региона (O(1)).
- Методы:
  - `TryGetTerritoryInfoById(ushort id, out TerritoryInfo? info)` — получение данных региона.
  - `IsTileInTerritory(int tileX, int tileY, TerritoryInfo territoryInfo)` — проверка принадлежности тайла региону.
  - `GetAllTerritoriesInArea(int startX, int startY, int width, int height)` — получение всех регионов в области.

### Redis
- Хранение объектов с использованием географического индекса.
- Методы:
  - `TryAddGeoPoint(int key, GeoPoint point)`
  - `TryRemoveGeoPoint(int key)`
  - `FirstOrDefaultByGeoPoint(GeoPoint point, double radius)`
  - `GetAllObjectsInRadius(GeoPoint point, double radius)`

### MagicOnion сервер
- Сервис для получения объектов и регионов в указанной области:
  - `GetObjectsInArea(GetObjectsInAreaRequest request)`
  - `GetRegionsInArea(GetRegionsInAreaRequest request)`
- Подписка на события объектов:
  - `SubscribeObjectEvents()` возвращает поток событий добавления, обновления и удаления объектов.
- Сериализация и десериализация через **MemoryPack**.
- Многопоточная обработка запросов и уведомлений.

---

## Требования
- .NET 8.0
- Redis (для хранения объектов)
- MagicOnion (для gRPC/реального времени)
- MemoryPack (для сериализации)

---

## Примеры использования

### Создание карты
```csharp
var redisClient = new RedisGeoClient("localhost:6379");
var mapManager = new MapManager(redisClient, width: 1000, height: 1000);
var tile = mapManager.GetTile(10, 10);
mapManager.SetTile(10, 10, new Tile(TileType.Grass, 1));
mapManager.FillArea(0, 0, 5, 5, new Tile(TileType.Water, 2));
```

### Работа с объектами
```csharp
var obj = new MapObject { Id = 1, Position = new Position(5, 5), Width = 2, Height = 2 };
mapManager.TryAddObject(obj);

var found = mapManager.FirstOrDefaultByPosition(new Position(5,5), 1);

mapManager.TryRemoveObject(1);
```

### Работа с регионами
```csharp
if(mapManager.TryGetTerritoryInfoById(1, out var territory))
{
	Console.WriteLine(territory?.Name);
}

var regions = mapManager.GetAllTerritoriesInArea(0,0,10,10);
```

### Подписка на события объектов
```csharp
mapManager.ObjectAdded += obj => Console.WriteLine($"Added: {obj.Id}");
mapManager.ObjectUpdated += obj => Console.WriteLine($"Updated: {obj.Id}");
mapManager.ObjectRemoved += id => Console.WriteLine($"Removed: {id}");
```


## Тестирование

Все операции покрыты юнит-тестами.

Проверены пограничные случаи:

Пустая область

Объекты на границе карты

Одновременные обновления объектов

Полное/частичное/отсутствие пересечения объектов


## Производительность

Получение/установка тайла: O(1)

Получение региона по тайлу: O(1)

Работа с объектами через Redis: быстрый поиск по координатам

Память карты ≤ 8 МБ (при стандартном размере 1000x1000 тайлов)

