using System;

namespace Makhai.Core
{
	[Flags]
	public enum Faction
	{
		None = 0x0,
		Neutral = 0x1,
		Player = 0x2,
		Enemy = 0x4,
		All = ~0x0
	}
}
