using System.Runtime.CompilerServices;

namespace MapLib.Map.Objects
{
	public record class MapObject
	{
		public int Id { get; init; }

		public float X { get; init; }

		public float Y { get; init; }

		public float Width { get; init; }

		public float Height { get; init; }
		
		public MapObject(int id, float x, float y, float width, float height)
		{
			Id = id;
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		
		/// <summary>
		/// Проверяет, нахоидтся ли точка внутри координат объекта.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(int px, int py)
		{
			if(px < X || px > X + Width)
			{
				return false;
			}
			
			if(py < Y || py > Y + Height)
			{
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Проверяет, лежит ли объект внутри указанной области.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Intersects(int x, int y, int w, int h)
		{
			if(X >= x + w || X + Width <= x)
			{
				return false;
			}
			
			if(Y >= y + h || Y + Height <= y)
			{
				return false;
			}
			
			return true;
		}
	}
}
