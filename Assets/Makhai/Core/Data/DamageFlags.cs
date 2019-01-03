using System;

namespace Makhai.Core.Data
{
	[Flags]
	public enum DamageFlags
	{
		None = 0x0,

		/// <summary>
		/// Damage should ignore shield value, and be applied directly to health.
		/// </summary>
		IgnoreShield = 0x1,

		/// <summary>
		/// Damage will hit shields, and continue through to health if damage is leftover.
		/// </summary>
		PierceShield = 0x2
	}
}
