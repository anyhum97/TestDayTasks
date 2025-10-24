using System.Runtime.CompilerServices;

namespace MapLib.Map.Objects
{
	public record class MapObject
	{
		public int Id { get; init; }

		public Position Position { get; init; }

		public float Width { get; init; }

		public float Height { get; init; }
		
		public MapObject(int id, Position position, float width, float height)
		{
			Id = id;
			Position = position;
			Width = width;
			Height = height;
		}
		
		/// <summary>
		/// Проверяет, нахоидтся ли точка внутри координат объекта.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(int px, int py)
		{
			if(px < Position.X || px > Position.X + Width)
			{
				return false;
			}
			
			if(py < Position.Y || py > Position.Y + Height)
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
			if(Position.X >= x + w || Position.X + Width <= x)
			{
				return false;
			}
			
			if(Position.Y >= y + h || Position.Y + Height <= y)
			{
				return false;
			}
			
			return true;
		}
	}
}
