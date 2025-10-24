using MapLib.Map;
using MapLib.Map.Objects;
using MapLib.Helpers;

using MapLib.Map.Enums;

namespace MapLib.Tests
{
	[TestFixture]
	public class MapManagerTests
	{
		private MapManager _mapManager;
		private InMemoryRedisClient _redis;

		[SetUp]
		public void Setup()
		{
			_redis = new InMemoryRedisClient();
			_mapManager = new MapManager(_redis, 100, 100); // компактная карта для тестов
		}

		[Test]
		public void Object_AddAndRetrieveById_WorksCorrectly()
		{
			var obj = new MapObject(1, new Position(10, 20), 5, 5);
			var added = _mapManager.TryAddObject(obj);

			Assert.IsTrue(added, "Object should be added");
			Assert.IsTrue(_redis.FirstOrDefaultByGeoPoint(GeoConverter.ToGeo(obj.Position, _mapManager.Width, _mapManager.Height)).HasValue);
			var retrieved = _mapManager.FirstOrDefaultByPosition(obj.Position);
			Assert.AreEqual(obj, retrieved);
		}

		[Test]
		public void Object_Remove_WorksCorrectly()
		{
			var obj = new MapObject(2, new Position(15, 25), 5, 5);
			_mapManager.TryAddObject(obj);

			var removed = _mapManager.TryRemoveObject(obj.Id);
			Assert.IsTrue(removed, "Object should be removed");

			Assert.IsFalse(_redis.FirstOrDefaultByGeoPoint(GeoConverter.ToGeo(obj.Position, _mapManager.Width, _mapManager.Height)).HasValue);
			Assert.IsNull(_mapManager.FirstOrDefaultByPosition(obj.Position));
		}

		[Test]
		public void Object_Events_AreInvoked()
		{
			var obj = new MapObject(3, new Position(5, 5), 3, 3);
			MapObject? addedObj = null;
			int? removedId = null;

			_mapManager.ObjectAdded += o => addedObj = o;
			_mapManager.ObjectRemoved += id => removedId = id;

			_mapManager.TryAddObject(obj);
			Task.Delay(10).Wait(); // ждём асинхронный вызов события
			Assert.AreEqual(obj, addedObj);

			_mapManager.TryRemoveObject(obj.Id);
			Task.Delay(10).Wait();
			Assert.AreEqual(obj.Id, removedId);
		}

		[Test]
		public void IntersectsArea_FullPartialNone()
		{
			var obj = new MapObject(4, new Position(10, 10), 10, 10);
			_mapManager.TryAddObject(obj);

			// Полное вхождение
			Assert.IsTrue(_mapManager.IntersectsArea(obj.Id, 10, 10, 10, 10));

			// Частичное пересечение
			Assert.IsTrue(_mapManager.IntersectsArea(obj.Id, 15, 15, 10, 10));

			// Нет пересечения
			Assert.IsFalse(_mapManager.IntersectsArea(obj.Id, 0, 0, 5, 5));
		}

		[Test]
		public void GetAllObjectsInArea_ReturnsCorrectObjects()
		{
			var obj1 = new MapObject(5, new Position(0, 0), 5, 5);
			var obj2 = new MapObject(6, new Position(10, 10), 5, 5);
			_mapManager.TryAddObject(obj1);
			_mapManager.TryAddObject(obj2);

			var list = _mapManager.GetAllObjectsInArea(0, 0, 15, 15);
			Assert.Contains(obj1, list);
			Assert.Contains(obj2, list);
			Assert.AreEqual(2, list.Count);

			// Ограниченная область
			list = _mapManager.GetAllObjectsInArea(0, 0, 5, 5);
			Assert.Contains(obj1, list);
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void Tile_SetAndGet_WorksCorrectly()
		{
			var tile = new Tile(TileType.Plain, 1);
			_mapManager.SetTile(0, 0, tile);
			var t = _mapManager.GetTile(0, 0);
			Assert.AreEqual(tile, t);
		}

		[Test]
		public void FillArea_FillsCorrectly()
		{
			var tile = new Tile(TileType.Plain, 2);
			_mapManager.FillArea(0, 0, 2, 2, tile);
			for (int y = 0; y < 2; y++)
				for (int x = 0; x < 2; x++)
					Assert.AreEqual(tile, _mapManager.GetTile(x, y));
		}

		[Test]
		public void CanPlaceInArea_RespectsTileType()
		{
			var grass = new Tile(TileType.Plain, 0);
			var mountain = new Tile(TileType.Mountain, 0);

			_mapManager.FillArea(0, 0, 2, 2, grass);
			Assert.IsTrue(_mapManager.CanPlaceInArea(0, 0, 2, 2));

			_mapManager.SetTile(1, 1, mountain);
			Assert.IsFalse(_mapManager.CanPlaceInArea(0, 0, 2, 2));
		}

		[Test]
		public void GetTile_OutOfBounds_Throws()
		{
			Assert.Throws<IndexOutOfRangeException>(() => _mapManager.GetTile(-1, 0));
			Assert.Throws<IndexOutOfRangeException>(() => _mapManager.GetTile(0, -1));
			Assert.Throws<IndexOutOfRangeException>(() => _mapManager.GetTile(_mapManager.Width, 0));
			Assert.Throws<IndexOutOfRangeException>(() => _mapManager.GetTile(0, _mapManager.Height));
		}

		[Test]
		public void Object_Contains_PointInsideOutsideEdges()
		{
			var obj = new MapObject(1, new Position(2, 2), 4, 4);
			_mapManager.TryAddObject(obj);

			// Внутри объекта
			Assert.IsTrue(obj.Contains(3, 3));

			// На краю объекта
			Assert.IsTrue(obj.Contains(2, 2));
			Assert.IsTrue(obj.Contains(6, 6));

			// Снаружи объекта
			Assert.IsFalse(obj.Contains(1, 1));
			Assert.IsFalse(obj.Contains(7, 7));
		}

		[Test]
		public void Object_IntersectsArea_PartialEdges()
		{
			var obj = new MapObject(2, new Position(3, 3), 4, 4);
			_mapManager.TryAddObject(obj);

			// Полное вхождение
			Assert.IsTrue(obj.Intersects(3, 3, 4, 4));

			// Частичное пересечение слева
			Assert.IsTrue(obj.Intersects(1, 4, 3, 2));

			// Частичное пересечение справа
			Assert.IsTrue(obj.Intersects(6, 3, 4, 4));

			// Полностью снаружи
			Assert.IsFalse(obj.Intersects(0, 0, 2, 2));
			Assert.IsFalse(obj.Intersects(8, 8, 2, 2));
		}

		[Test]
		public void FirstOrDefaultByPosition_ReturnsCorrectObjectOrNull()
		{
			var obj = new MapObject(3, new Position(1, 1), 2, 2);
			_mapManager.TryAddObject(obj);

			var insidePos = new Position(1, 1);
			var outsidePos = new Position(0, 0);

			var found = _mapManager.FirstOrDefaultByPosition(insidePos);
			Assert.AreEqual(obj, found);

			var notFound = _mapManager.FirstOrDefaultByPosition(outsidePos);
			Assert.IsNull(notFound);
		}

		[Test]
		public void GetAllObjectsInArea_NoObjects_ReturnsEmpty()
		{
			var list = _mapManager.GetAllObjectsInArea(0, 0, 5, 5);
			Assert.IsNotNull(list);
			Assert.IsEmpty(list);
		}

		[Test]
		public void Tile_CanPlaceHere_ReturnsCorrectly()
		{
			var plain = new Tile(TileType.Plain, 0);
			var mountain = new Tile(TileType.Mountain, 0);

			Assert.IsTrue(plain.CanPlaceHere());
			Assert.IsFalse(mountain.CanPlaceHere());
		}

		[Test]
		public void GetTile_SetTile_OtherTilesUnchanged()
		{
			var t1 = new Tile(TileType.Plain, 1);
			var t2 = new Tile(TileType.Mountain, 2);

			_mapManager.SetTile(0, 0, t1);
			_mapManager.SetTile(1, 1, t2);

			Assert.AreEqual(t1, _mapManager.GetTile(0, 0));
			Assert.AreEqual(t2, _mapManager.GetTile(1, 1));

			// остальные тайлы должны быть дефолтными
			for (int y = 0; y < _mapManager.Height; y++)
				for (int x = 0; x < _mapManager.Width; x++)
				{
					if ((x == 0 && y == 0) || (x == 1 && y == 1)) continue;
					var tile = _mapManager.GetTile(x, y);
					Assert.AreEqual(default(Tile), tile);
				}
		}

		[Test]
		public void FillArea_PartialEdges_FillsCorrectly()
		{
			var tile = new Tile(TileType.Plain, 9);
			_mapManager.FillArea(8, 8, 3, 3, tile); // выходит за границы карты (10x10)
			
			// Должно заполнить только в пределах карты
			Assert.AreEqual(tile, _mapManager.GetTile(8, 8));
			Assert.AreEqual(tile, _mapManager.GetTile(9, 8));
			Assert.AreEqual(tile, _mapManager.GetTile(8, 9));
			Assert.AreEqual(tile, _mapManager.GetTile(9, 9));
		}

		[Test]
		public void CanPlaceInArea_EmptyAndBlockedTiles()
		{
			var plain = new Tile(TileType.Plain, 0);
			var mountain = new Tile(TileType.Mountain, 0);

			_mapManager.FillArea(0, 0, 2, 2, plain);
			Assert.IsTrue(_mapManager.CanPlaceInArea(0, 0, 2, 2));

			_mapManager.SetTile(1, 1, mountain);
			Assert.IsFalse(_mapManager.CanPlaceInArea(0, 0, 2, 2));
		}
	}
}
