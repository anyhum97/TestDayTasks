using MapLib.Map.Objects;

using System.Runtime.CompilerServices;

namespace MapLib
{
	public class MapManager
	{
		private const int _defaultWidth = 1000;
		private const int _defaultHeight = 1000;

		private readonly int _width = _defaultWidth;

		private readonly int _height = _defaultHeight;

		private readonly Tile[] _tiles;

		private readonly ReaderWriterLockSlim _locker = new();

		public int Height => _height;

		public int Width => _width;

		public MapManager(int width = _defaultWidth, int height = _defaultHeight)
		{
			ValidateBounds(width, height);

			_width = width;
			_height = height;

			var count = _width * _height;

			_tiles = new Tile[count];
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
		/// Позволяет потокобезопасно получить тайл по координатам.
		/// </summary>
		public Tile GetTile(int x, int y)
		{
			_locker.EnterReadLock();

			try
			{
				var index = GetIndex(x, y);
			
				return _tiles[index];
			}
			finally
			{
				_locker.ExitReadLock();
			}
		}
		
		/// <summary>
		/// Позволяет потокобезопасно установить тайл по координатам.
		/// </summary>
		public void SetTile(int x, int y, Tile tile)
		{
			_locker.EnterWriteLock();
			
			try
			{
				var index = GetIndex(x, y);

				_tiles[index] = tile;
			}
			finally
			{
				_locker.ExitWriteLock();
			}
		}

		/// <summary>
		/// Потокобезопасно заполняет прямоугольную область тайлами.
		/// </summary>
		/// <param name="startX">Начальная координата X (включительно)</param>
		/// <param name="startY">Начальная координата Y (включительно)</param>
		/// <param name="width">Ширина области</param>
		/// <param name="height">Высота области</param>
		/// <param name="tile">Тайл для заполнения</param>
		public void FillArea(int startX, int startY, int width, int height, Tile tile)
		{
			_locker.EnterWriteLock();

			try
			{
				for(int y=startY; y<startY+height; ++y)
				{
					for(int x=startX; x<startX+width; ++x)
					{
						var index = GetIndex(x, y);

						_tiles[index] = tile;
					}
				}
			}
			finally
			{
				_locker.ExitWriteLock();
			}
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
			return y * _width + x;
		}
	}
}
