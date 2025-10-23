using MapLib.Map.Objects;
using System.Runtime.CompilerServices;

namespace MapLib.Map
{
	/// <summary>
	/// Класс для работы с тайловой картой в однопоточной среде.
	/// </summary>
	public class MapManager
	{
		private const int _defaultWidth = 1000;
		private const int _defaultHeight = 1000;

		private readonly int _width = _defaultWidth;
		private readonly int _height = _defaultHeight;

		private readonly Tile[] _tiles;

		public int Height => _height;

		public int Width => _width;

		public MapManager(int width = _defaultWidth, int height = _defaultHeight)
		{
			ValidateBounds(width, height);

			_width = width;
			_height = height;

			_tiles = new Tile[_width * _height];
		}

		public MapManager(Tile[] tiles, int width, int height)
		{
			ValidateBounds(width, height);
			ValidateArray(tiles, width, height);

			_width = width;
			_height = height;

			_tiles = tiles;
		}

		public MapManager(IEnumerable<Tile> tiles, int width, int height)
		{
			ValidateBounds(width, height);
			ValidateCollection(tiles, width, height);

			_width = width;
			_height = height;

			_tiles = tiles.ToArray();
		}

		/// <summary>
		/// Получает тайл по координатам.
		/// </summary>
		public Tile GetTile(int x, int y)
		{
			var index = GetIndex(x, y);

			return _tiles[index];
		}

		/// <summary>
		/// Устанавливает тайл по координатам.
		/// </summary>
		public void SetTile(int x, int y, Tile tile)
		{
			var index = GetIndex(x, y);

			_tiles[index] = tile;
		}

		/// <summary>
		/// Заполняет прямоугольную область тайлами.
		/// </summary>
		public void FillArea(int startX, int startY, int width, int height, Tile tile)
		{
			int rowEnd = startY + height;
			int colEnd = startX + width;

			for(int y=startY; y<rowEnd; ++y)
			{
				int index = y * _width + startX;

				for(int x=startX; x<colEnd; ++x, ++index)
				{
					_tiles[index] = tile;
				}
			}
		}

		/// <summary>
		/// Проверяет возможность размещения объекта в прямоугольной области.
		/// </summary>
		public bool CanPlaceInArea(int startX, int startY, int width, int height)
		{
			// Оптимизация: считаем линейный индекс сразу

			int rowEnd = startY + height;
			int colEnd = startX + width;

			for (int y=startY; y<rowEnd; ++y)
			{
				int index = y * _width + startX;

				for(int x=startX; x<colEnd; ++x, ++index)
				{
					if(!_tiles[index].CanPlaceHere())
					{
						return false;
					}
				}
			}

			return true;
		}

		private static void ValidateBounds(int width, int height)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(width, 1, nameof(width));
			ArgumentOutOfRangeException.ThrowIfLessThan(height, 1, nameof(height));
		}

		private static void ValidateArray(Tile[] tiles, int width, int height)
		{
			ArgumentNullException.ThrowIfNull(tiles);

			var length = tiles.Length;

			var expected = width * height;

			if(length != expected)
			{
				throw new ArgumentOutOfRangeException("Invalid array lenght");
			}
		}

		private static void ValidateCollection(IEnumerable<Tile> tiles, int width, int height)
		{
			ArgumentNullException.ThrowIfNull(tiles);

			var count = tiles.Count();

			var expected = width * height;

			if(count != expected)
			{
				throw new ArgumentOutOfRangeException("Invalid collection size");
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetIndex(int x, int y)
		{
			#if DEBUG
			
			// Проверяем диапазон значений только в DEBUG сборке.

			if(x < 0 || x >= _width)
			{
				throw new IndexOutOfRangeException();
			}

			if(y < 0 || y >= _height)
			{
				throw new IndexOutOfRangeException();
			}

			#endif

			return y * _width + x;
		}
	}
}
