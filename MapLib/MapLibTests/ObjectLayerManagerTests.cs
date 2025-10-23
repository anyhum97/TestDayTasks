using MapLib.Helpers;
using MapLib.Map;
using MapLib.Map.Objects;

namespace MapLib.Tests
{
	[TestFixture]
	public class ObjectLayerManagerTests
	{
		private InMemoryRedisClient _redis;
		private ObjectLayerManager _manager;
		private const int MapWidth = 1000;
		private const int MapHeight = 1000;

		[SetUp]
		public void Setup()
		{
			_redis = new InMemoryRedisClient();
			_manager = new ObjectLayerManager(_redis, MapWidth, MapHeight);
		}

		[Test]
		public async Task AddObjectAsync_ObjectStoredInRedis()
		{
			var obj = new MapObject { Id = "1", X = 100, Y = 200, Width = 10, Height = 10 };
			bool added = false;

			_manager.ObjectAdded += _ => added = true;

			await _manager.AddObjectAsync(obj);
			await _redis.SetObjectAsync(obj);

			var stored = await _redis.GetObjectAsync(obj.Id);

			Assert.NotNull(stored);
			Assert.IsTrue(added);
			Assert.AreEqual(obj.Id, stored!.Id);
			Assert.AreEqual(obj.X, stored.X);
		}

		[Test]
		public async Task GetByIdAsync_ReturnsSameObject()
		{
			var obj = new MapObject { Id = "2", X = 300, Y = 400, Width = 5, Height = 5 };
			await _redis.SetObjectAsync(obj);

			var fetched = await _manager.GetByIdAsync(obj.Id);

			Assert.NotNull(fetched);
			Assert.AreEqual(obj.Id, fetched!.Id);
			Assert.AreEqual(obj.X, fetched.X);
			Assert.AreEqual(obj.Y, fetched.Y);
		}

		[Test]
		public async Task RemoveObjectAsync_ObjectDeletedFromRedis()
		{
			var obj = new MapObject { Id = "3", X = 100, Y = 200, Width = 5, Height = 5 };
			await _redis.SetObjectAsync(obj);
			await _manager.AddObjectAsync(obj);

			bool removed = false;
			_manager.ObjectRemoved += _ => removed = true;

			await _manager.RemoveObjectAsync(obj);

			var fetched = await _redis.GetObjectAsync(obj.Id);

			Assert.IsTrue(removed);
			Assert.IsNull(fetched);
		}

		[Test]
		public async Task GetObjectsInAreaAsync_ReturnsObjectInsideArea()
		{
			var obj = new MapObject { Id = "4", X = 100, Y = 100, Width = 50, Height = 50 };
			await _redis.SetObjectAsync(obj);
			await _manager.AddObjectAsync(obj);

			var result = await _manager.GetObjectsInAreaAsync(90, 90, 100, 100);

			int count = 0;
			foreach(var o in result)
			{
				if(o.Id == "4")
					count++;
			}

			Assert.AreEqual(1, count);
		}

		[Test]
		public async Task GetObjectsInAreaAsync_ReturnsNothingOutsideArea()
		{
			var obj = new MapObject { Id = "5", X = 800, Y = 800, Width = 10, Height = 10 };
			await _redis.SetObjectAsync(obj);
			await _manager.AddObjectAsync(obj);

			var result = await _manager.GetObjectsInAreaAsync(0, 0, 100, 100);

			bool found = false;
			foreach(var o in result)
			{
				if(o.Id == "5")
					found = true;
			}

			Assert.IsFalse(found);
		}

		[Test]
		public void Intersects_FullAndPartialAndNone()
		{
			var obj = new MapObject { X = 100, Y = 100, Width = 50, Height = 50 };

			// Полное вхождение
			Assert.IsTrue(obj.Intersects(90, 90, 200, 200));

			// Частичное пересечение
			Assert.IsTrue(obj.Intersects(140, 140, 50, 50));

			// Нет пересечения
			Assert.IsFalse(obj.Intersects(500, 500, 50, 50));
		}

		[Test]
		public async Task ObjectUpdated_EventTriggered()
		{
			bool updated = false;
			_manager.ObjectUpdated += _ => updated = true;

			var obj = new MapObject { Id = "6", X = 10, Y = 10, Width = 10, Height = 10 };

			// Имитируем обновление
			await _manager.UpdateObjectAsync(obj);

			Assert.IsTrue(updated);
		}

		[Test]
		public async Task GetObjectsInAreaAsync_ReturnsAllObjectsInRegion()
		{
			var o1 = new MapObject { Id = "A", X = 50, Y = 50, Width = 10, Height = 10 };
			var o2 = new MapObject { Id = "B", X = 150, Y = 150, Width = 20, Height = 20 };
			var o3 = new MapObject { Id = "C", X = 900, Y = 900, Width = 30, Height = 30 };

			await _redis.SetObjectAsync(o1);
			await _redis.SetObjectAsync(o2);
			await _redis.SetObjectAsync(o3);

			await _manager.AddObjectAsync(o1);
			await _manager.AddObjectAsync(o2);
			await _manager.AddObjectAsync(o3);

			var result = await _manager.GetObjectsInAreaAsync(0, 0, 300, 300);

			List<MapObject> list = new List<MapObject>();
			foreach(var o in result)
			{
				list.Add(o);
			}

			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(list.Exists(x => x.Id == "A"));
			Assert.IsTrue(list.Exists(x => x.Id == "B"));
		}

		[Test]
		public async Task BoundaryCases_AreHandledCorrectly()
		{
			var obj = new MapObject { Id = "BND", X = 0, Y = 0, Width = 10, Height = 10 };
			await _redis.SetObjectAsync(obj);
			await _manager.AddObjectAsync(obj);

			var result = await _manager.GetObjectsInAreaAsync(0, 0, 10, 10);

			bool found = false;
			foreach(var o in result)
			{
				if(o.Id == "BND")
					found = true;
			}

			Assert.IsTrue(found);
		}
	}
}
