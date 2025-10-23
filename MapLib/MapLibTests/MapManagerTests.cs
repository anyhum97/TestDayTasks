using MapLib.Map;
using MapLib.Map.Objects;
using MapLib.Map.Enums;

namespace MapLib.Tests
{
	[TestFixture]
	public class MapManagerTests
	{
		[Test]
		public void Constructor_Default_CreatesEmptyMap()
		{
			var map = new MapManager(10, 20);
			
			Assert.AreEqual(10, map.Width);
			Assert.AreEqual(20, map.Height);

			// Все тайлы должны быть по умолчанию (TileType = 0, TerritoryId = 0)
			for (int y = 0; y < map.Height; y++)
				for (int x = 0; x < map.Width; x++)
					Assert.AreEqual(default(Tile), map.GetTile(x, y));
		}

		[Test]
		public void SetTile_And_GetTile_WorksCorrectly()
		{
			var map = new MapManager(5, 5);
			var tile = new Tile(TileType.Mountain, 42);

			map.SetTile(2, 3, tile);

			Assert.AreEqual(tile, map.GetTile(2, 3));
		}

		[Test]
		public void FillArea_FillsCorrectly()
		{
			var map = new MapManager(4, 4);
			var tile = new Tile(TileType.Mountain, 1);

			map.FillArea(1, 1, 2, 2, tile);

			for (int y = 0; y < map.Height; y++)
			{
				for (int x = 0; x < map.Width; x++)
				{
					if (x >= 1 && x <= 2 && y >= 1 && y <= 2)
						Assert.AreEqual(tile, map.GetTile(x, y));
					else
						Assert.AreEqual(default(Tile), map.GetTile(x, y));
				}
			}
		}

		[Test]
		public void CanPlaceInArea_ReturnsFalse_WhenMountainPresent()
		{
			var map = new MapManager(3, 3);
			var mountain = new Tile(TileType.Mountain, 0);
			map.SetTile(1, 1, mountain);

			bool canPlace = map.CanPlaceInArea(0, 0, 3, 3);
			Assert.IsFalse(canPlace);
		}

		[Test]
		public void CanPlaceInArea_ReturnsTrue_WhenNoMountain()
		{
			var map = new MapManager(2, 2);
			var tile = new Tile(TileType.Plain, 0);
			map.FillArea(0, 0, 2, 2, tile);

			Assert.IsTrue(map.CanPlaceInArea(0, 0, 2, 2));
		}

		[Test]
		public void Constructor_WithArray_Throws_WhenSizeIncorrect()
		{
			var tiles = new Tile[4]; // 2x2 expected
			Assert.Throws<ArgumentOutOfRangeException>(() => new MapManager(tiles, 3, 3));
		}

		[Test]
		public void Constructor_WithCollection_Throws_WhenSizeIncorrect()
		{
			var tiles = Enumerable.Repeat(new Tile(TileType.Mountain, 0), 4);
			Assert.Throws<ArgumentOutOfRangeException>(() => new MapManager(tiles, 3, 3));
		}

		[Test]
		public void GetTile_Throws_WhenOutOfBounds()
		{
			var map = new MapManager(2, 2);
			Assert.Throws<IndexOutOfRangeException>(() => map.GetTile(-1, 0));
			Assert.Throws<IndexOutOfRangeException>(() => map.GetTile(0, 2));
		}

		#if DEBUG

		[Test]
		public void SetTile_Throws_WhenOutOfBounds()
		{
			var map = new MapManager(2, 2);
			var tile = new Tile(TileType.Mountain, 0);

			Assert.Throws<IndexOutOfRangeException>(() => map.SetTile(2, 0, tile));
			Assert.Throws<IndexOutOfRangeException>(() => map.SetTile(0, -1, tile));
		}

		#endif
	}
}
