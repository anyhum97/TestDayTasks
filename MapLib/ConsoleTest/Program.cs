using MapLib;
using MapLib.Map.Enums;
using MapLib.Map.Objects;
using System.Diagnostics;

namespace ConsoleTest
{
	public static class Program
	{
		public static void Main()
		{
			const int width = 1000;
			const int height = 1000;

			const int iterations = 10;

			var manager = new MapManager(width, height);
			
			var random = new Random();

			var stopwatch = new Stopwatch();

			stopwatch.Start();

			for(int i=0; i<iterations; ++i)
			{
				for(int x=0; x<width; ++x)
				{
					for(int y=0; y<height; ++y)
					{
						var type = (ushort)random.Next(ushort.MaxValue);

						var territoryId = (ushort)random.Next(ushort.MaxValue);

						var tile = new Tile((TileType)type, territoryId);

						manager.SetTile(x, y, tile);

						var get = manager.GetTile(x, y);

						if(tile != get)
						{
							throw new Exception();
						}
					}
				}
			}

			stopwatch.Stop();

			var elapsed = stopwatch.Elapsed;

			Console.WriteLine($"Elapsed {elapsed.TotalMilliseconds} ms");
			Console.ReadKey();
		}
	}
}
