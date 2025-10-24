using System.Diagnostics;

using MapLib.Map;
using MapLib.Map.Enums;
using MapLib.Map.Objects;
using MapLib.Helpers;

namespace ConsoleTest
{
	public static class Program
	{
		private const int _defaultWidth = 1000;
		private const int _defaultHeight = 1000;

		private const int _defaultIterations = 10;

		public delegate void BenchmarkWork(ConcurrentMapManager manager, int width, int height, int iterations);

		public static void Main()
		{
			//Benchmark(Test1, 1000, 1000, 5);

			var redis = new InMemoryRedisClient();

			var manager = new MapManager(redis);
		}

		private static void Benchmark(BenchmarkWork work, int width = _defaultWidth, int height = _defaultHeight, int iterations = _defaultIterations)
		{
			var manager = new ConcurrentMapManager(width, height);

			var stopwatch = new Stopwatch();

			work.Invoke(manager, width, height, iterations); // Прогреваем

			stopwatch.Start();

			work.Invoke(manager, width, height, iterations);

			stopwatch.Stop();

			var elapsed = stopwatch.Elapsed;

			Console.WriteLine($"Elapsed {elapsed.TotalMilliseconds} ms");
			Console.ReadKey();
		}

		private static void Test1(ConcurrentMapManager manager, int width, int height, int iterations)
		{
			var random = new Random();

			for(int i=0; i<iterations; ++i)
			{
				for(int x=0; x<width; ++x)
				{
					for(int y=0; y<height; ++y)
					{
						var type = (ushort)random.Next(ushort.MaxValue);
			
						var territoryId = (ushort)random.Next(ushort.MaxValue);
			
						var set = new Tile((TileType)type, territoryId);
			
						manager.SetTile(x, y, set);
			
						var get = manager.GetTile(x, y);
			
						if(set != get)
						{
							throw new Exception();
						}
					}
				}
			}
		}

		private static void Test2(ConcurrentMapManager manager, int width, int height, int iterations)
		{
			var random = new Random();

			var hash = 0; // Чтобы компилятор не оптимизировал ненужное чтение

			for(int i=0; i<iterations; ++i)
			{
				for(int x=0; x<width; ++x)
				{
					for(int y=0; y<height; ++y)
					{
						var type = (ushort)random.Next(ushort.MaxValue);
						
						var territoryId = (ushort)random.Next(ushort.MaxValue);
						
						var set = new Tile((TileType)type, territoryId);
						
						manager.SetTile(x, y, set);
						
						var get = manager.GetTile(x, y);
						
						for(int j=0; j<100; ++j)
						{
							// Иммитируем частое чтение

							hash += manager.GetTile(random.Next(width), random.Next(height)).TerritoryId;
						}

						if(set != get)
						{
							throw new Exception();
						}
					}
				}
			}

			Console.WriteLine(hash);
		}
	}
}
