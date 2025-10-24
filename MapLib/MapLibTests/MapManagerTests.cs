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
	}
}
