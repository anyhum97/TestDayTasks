using MapLib.Map;
using MapLib.Map.Objects;
using MapLib.Interfaces;
using MapLib.Map.Enums;
using MapLib.Helpers;

namespace MapLib.Tests
{
	[TestFixture]
	public class MapManagerTests
	{
		private IRedisClient _redis;

		[SetUp]
		public void Setup()
		{
			_redis = new RedisGeoClient("localhost:6379");
		}

		[Test]
		public void DefaultConstructorTest()
		{
			const int width = 20;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			Assert.AreEqual(width, manager.Width);
			Assert.AreEqual(height, manager.Height);

			var tile = manager.GetTile(width - 1, height-1);
		}

		[Test]
		public void WithTilesConstructorTest()
		{
			const int width = 10;
			const int height = 10;

			const int count = width * height;

			var tiles = new Tile[count];

			var manager1 = new MapManager(_redis, tiles, width, height);

			var manager2 = new MapManager(_redis, tiles.ToList(), width, height);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles, width + 1, height);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles, width, height + 1);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles, 0, height);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles, width, 0);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles.ToList(), width + 1, height);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles.ToList(), width, height + 1);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles.ToList(), 0, height);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var manager2 = new MapManager(_redis, tiles.ToList(), width, 0);
			});
		}
		
		[Test]
		public void GetTileTest()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			var tile1 = manager.GetTile(0, 0);
			var tile2 = manager.GetTile(width-1, 0);
			var tile3 = manager.GetTile(0, height-1);
			var tile4 = manager.GetTile(width-1, height-1);

			Assert.AreNotEqual(0, tile1.TerritoryId);

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				var tile = manager.GetTile(-1, 0);
			});

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				var tile = manager.GetTile(0, -1);
			});

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				var tile = manager.GetTile(width, 0);
			});

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				var tile = manager.GetTile(0, height);
			});
		}

		[Test]
		public void SetTileTest()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			var tile = new Tile(TileType.Mountain, 1);

			manager.SetTile(0, 0, tile);
			manager.SetTile(width-1, 0, tile);
			manager.SetTile(0, height-1, tile);
			manager.SetTile(width-1, height-1, tile);

			var get1 = manager.GetTile(0, 0);
			var get2 = manager.GetTile(width-1, 0);
			var get3 = manager.GetTile(0, height-1);
			var get4 = manager.GetTile(width-1, height-1);

			Assert.AreEqual(tile, get1);
			Assert.AreEqual(tile, get2);
			Assert.AreEqual(tile, get3);
			Assert.AreEqual(tile, get4);

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				manager.SetTile(width, 0, tile);
			});

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				manager.SetTile(0, height, tile);
			});

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				manager.SetTile(-1, 0, tile);
			});

			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				manager.SetTile(0, -1, tile);
			});
		}

		[Test]
		public void FillAreaTest()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			var tile = new Tile(TileType.Plain, 5);

			manager.FillArea(2, 2, 3, 3, tile);

			for(int y=2; y<5; ++y)
			{
				for(int x=2; x<5; ++x)
				{
					var t = manager.GetTile(x, y);

					Assert.AreEqual(tile, t);
				}
			}
		}

		[Test]
		public void CanPlaceInAreaTest()
		{
			const int width = 5;
			const int height = 5;

			var manager = new MapManager(_redis, width: width, height: height);

			var tile = new Tile(TileType.Mountain, 1);

			manager.FillArea(0, 0, width, height, tile);

			// После заполнения "горами" нельзя разместить
			Assert.IsFalse(manager.CanPlaceInArea(0, 0, width, height));
		}

		[Test]
		public void TryAddAndRemoveObject_Success()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			var obj = new MapObject(1, new Position(3, 3), 1, 1);

			manager.TryRemoveObject(obj.Id);

			var added = manager.TryAddObject(obj);

			Assert.IsTrue(added);

			var removed = manager.TryRemoveObject(obj.Id);

			Assert.IsTrue(removed);
		}

		[Test]
		public void FirstOrDefaultByPosition_ReturnsObject()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			var obj = new MapObject(3, new Position(4, 4), 1, 1);

			manager.TryRemoveObject(obj.Id);

			manager.TryAddObject(obj);

			var result = manager.FirstOrDefaultByPosition(obj.Position, 10);

			Assert.IsNotNull(result);
			Assert.AreEqual(obj.Id, result.Id);

			manager.TryRemoveObject(obj.Id);
		}

		[Test]
		public void TryGetTerritoryInfoById_ReturnsValue()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			var first = manager.TryGetTerritoryInfoById(1, out var info);

			Assert.IsTrue(first);
			Assert.NotNull(info);
			Assert.AreEqual(1, info.Id);
		}

		[Test]
		public void IsTileInTerritory_CorrectlyDetectsMembership()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			manager.TryGetTerritoryInfoById(1, out var info);

			Assert.IsTrue(manager.IsTileInTerritory(0, 0, info));
		}

		[Test]
		public void GetAllTerritoriesInArea_ReturnsNonEmptyList()
		{
			const int width = 10;
			const int height = 10;

			var manager = new MapManager(_redis, width: width, height: height);

			var result = manager.GetAllTerritoriesInArea(0, 0, width, height);

			Assert.IsNotEmpty(result);
		}

		[Test]
		public void GetAllObjectsInArea_ReturnsObjectsWithinRadius()
		{
			const int width = 1000;
			const int height = 1000;

			var manager = new MapManager(_redis, width: width, height: height);

			// Объект в центре области
			var obj1 = new MapObject(10, new Position(500, 500), 5, 5);
			manager.TryAddObject(obj1);

			// Объект далеко — не должен попасть
			var obj2 = new MapObject(11, new Position(900, 900), 5, 5);
			manager.TryAddObject(obj2);

			var result = manager.GetAllObjectsInArea(500, 500, 100);

			Assert.That(result, Is.Not.Empty);
			Assert.That(result.Exists(o => o.Id == obj1.Id));
			Assert.That(result.Exists(o => o.Id == obj2.Id), Is.False);
		}

		#if DEBUG

		[Test]
		public void IntersectsArea_Throws_WhenInvalidSize()
		{
			const int width = 1000;
			const int height = 1000;

			var manager = new MapManager(_redis, width: width, height: height);

			var obj = new MapObject(3, new Position(10, 10), 2, 2);

			manager.TryAddObject(obj);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				manager.IntersectsArea(obj.Id, 10, 10, 0, 0);
			});
		}

		#endif
	}
}
