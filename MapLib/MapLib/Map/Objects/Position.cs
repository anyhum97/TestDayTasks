using MemoryPack;
using System.Runtime.InteropServices;

namespace MapLib.Map.Objects
{
	[MemoryPackable]
	[StructLayout(LayoutKind.Explicit)]
	public readonly partial struct Position(float x, float y)
	{
		[FieldOffset(0)]
		public readonly float X = x;
		
		[FieldOffset(4)]
		public readonly float Y = y;
	}
}
