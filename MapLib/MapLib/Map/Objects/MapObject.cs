namespace MapLib.Map.Objects
{
	public class MapObject
	{
		public string Id { get; set; }
		
		public int X { get; set; }
		
		public int Y { get; set; }
		
		public int Width { get; set; }
		
		public int Height { get; set; }
		
		public string Type { get; set; } = "Generic";
		
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
