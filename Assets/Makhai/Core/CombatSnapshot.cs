namespace Makhai.Core
{
	public class CombatSnapshot
	{
		public Entity Victim { get; }
		public Entity Attacker { get; }
		public float HealthDamage { get; }
		public float ShieldDamage { get; }
		public bool VictimDied { get; }

		public CombatSnapshot(Builder builder)
		{
			Victim = builder.Victim;
			Attacker = builder.Attacker;
			HealthDamage = builder.HealthDamage;
			ShieldDamage = builder.ShieldDamage;
			VictimDied = builder.VictimDied;
		}

		public class Builder
		{
			public Entity Victim { get; set; } = null;
			public Entity Attacker { get; set; } = null;
			public float HealthDamage { get; set; } = 0f;
			public float ShieldDamage { get; set; } = 0f;
			public bool VictimDied { get; set; } = false;
		}
	}
}
