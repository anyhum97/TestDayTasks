using MapLib;
using MapLib.Map.Enums;
using MapLib.Map.Helpers;

namespace Tests
{
	/// <summary>
	/// Проверяем корректность работы класса MapManager
	/// </summary>
	public class MapManagerTests
	{
		[Fact]
		public void GenerateMap_ShouldInitializeTilesAndTerritories()
		{
			// Act
			MapManager.GenerateMap(50);
			
			// Assert
			var tile = MapManager.GetTile(0, 0);
			var tileType = MapManager.GetTileType(0, 0);
			var territoryId = MapManager.GetTerritoryId(0, 0);
			var info = MapManager.GetTerritoryInfo(territoryId);
			
			Assert.InRange((int)tileType, 0, 1); // TileType в диапазоне
			Assert.InRange(territoryId, 1, 50);  // TerritoryId в диапазоне
			Assert.NotNull(info);                // Есть информация о территории
			Assert.Equal(territoryId, info.Id);  // ID совпадает
		}
		
		[Fact]
		public void EncodeDecode_ShouldReturnOriginalValues()
		{
			// Arrange
			var tileType = TileType.Grass;
			var territoryId = 123;
			
			// Act
			var encoded = TileEncoder.Encode(tileType, territoryId);
			var decodedType = encoded.GetTileType();
			var decodedId = encoded.GetTerritoryId();
			
			// Assert
			Assert.Equal(tileType, decodedType);
			Assert.Equal(territoryId, decodedId);
		}
		
		[Fact]
		public void GetTerritoryInfo_ShouldThrowIfNotExists()
		{
			// Arrange
			MapManager.GenerateMap(5);
			
			// Act & Assert
			Assert.Throws<KeyNotFoundException>(() => MapManager.GetTerritoryInfo(9999));
		}
		
		[Fact]
		public void MultipleGenerations_ShouldNotThrowOrLeakState()
		{
			// Act
			MapManager.GenerateMap(10);
			MapManager.GenerateMap(20);
			MapManager.GenerateMap(5);
			
			// Assert
			var tileType = MapManager.GetTileType(10, 10);
			var terrId = MapManager.GetTerritoryId(10, 10);
			var info = MapManager.GetTerritoryInfo(terrId);
			
			Assert.InRange(terrId, 1, 5);
			Assert.NotNull(info);
		}
		
		[Fact]
		public void MapTiles_ShouldBeWithinValidRange()
		{
			// Arrange
			const int territoryCount = 30;
			MapManager.GenerateMap(territoryCount);
			
			// Act
			int width = 1000;
			int height = 1000;
			
			// Проверяем несколько случайных тайлов
			for(int y=0; y<height; y+=200)
			{
				for(int x=0; x<width; x+=200)
				{
					var type = MapManager.GetTileType(x, y);
					var id = MapManager.GetTerritoryId(x, y);
					
					Assert.InRange(id, 1, territoryCount);
					Assert.True(Enum.IsDefined(typeof(TileType), type));
				}
			}
		}
	}
}
