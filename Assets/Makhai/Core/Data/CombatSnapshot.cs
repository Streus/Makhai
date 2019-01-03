namespace Makhai.Core.Data
{
	/// <summary>
	/// Represents a damage interaction between an attacker and a victim.
	/// </summary>
	public class CombatSnapshot
	{
		public Entity Victim { get; }
		public Entity Attacker { get; }
		public float HealthDamage { get; }
		public float ShieldDamage { get; }
		public float TotalDamage { get { return HealthDamage + ShieldDamage; } }
		public DamageFlags DmgFlags { get; }
		public bool VictimDied { get; }

		public CombatSnapshot(Builder builder)
		{
			Victim = builder.Victim;
			Attacker = builder.Attacker;
			HealthDamage = builder.HealthDamage;
			ShieldDamage = builder.ShieldDamage;
			DmgFlags = builder.DmgFlags;
			VictimDied = builder.VictimDied;
		}

		/// <summary>
		/// Builds an immutable CombatSnapshot object.
		/// </summary>
		public class Builder
		{
			public Entity Victim { get; set; } = null;
			public Entity Attacker { get; set; } = null;
			public float HealthDamage { get; set; } = 0f;
			public float ShieldDamage { get; set; } = 0f;
			public DamageFlags DmgFlags { get; set; } = DamageFlags.None;
			public bool VictimDied { get; set; } = false;

			public CombatSnapshot Build()
			{
				return new CombatSnapshot (this);
			}
		}
	}
}
