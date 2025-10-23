using MapLib.Map.Enums;
using MapLib.Map.Helpers;

namespace Tests
{
	/// Проверяем корректнотсь работы класса TileEncoder
	/// Нужно проверить, что конвертация работает правильно для всех значений
	public class TileEncoderTests
	{
		[Fact]
		public void EncodeDecode_SimpleConsistency()
		{
			// Простая проверка

			int territoryId = 42;
			TileType type = TileType.Mountain;

			long encoded = TileEncoder.Encode(type, territoryId);

			TileType decodedType1 = TileEncoder.GetTileType(encoded);
			int decodedTerritoryId1 = TileEncoder.GetTerritoryId(encoded);

			Assert.Equal(type, decodedType1);
			Assert.Equal(territoryId, decodedTerritoryId1);
		}

		//[Fact]
		//public void EncodeDecode_CheckConsistency()
		//{
		//	// Тут реализуем более серьёзную проверку
		//}
	}
}
