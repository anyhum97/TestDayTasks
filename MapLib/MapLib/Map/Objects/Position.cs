using System.Runtime.InteropServices;

namespace MapLib.Map.Objects
{
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct Position(float x, float y)
	{
		[FieldOffset(0)]
		public readonly float X = x;
		
		[FieldOffset(4)]
		public readonly float Y = y;
	}
}
