using System;
using System.Collections.Generic;

namespace Makhai.Core
{
	[Serializable]
    public class Entity : IEntityEventUser
    {
		#region STATIC_VARS

		public const float COMBAT_TIMER_MAX = 5f;
		#endregion

		#region INSTANCE_VARS
		private HashSet<Status> statuses;

		private float combatTimer;
		public bool InCombat
		{
			get
			{
				return combatTimer > 0f;
			}
		}

		private Faction faction;
		public Faction Affiliation { get { return faction; } }

		#region STATS

		public float Health { get; private set; }
		public BoundedStat HealthMax { get; set; }
		public Stat HealthRegen { get; set; }

		private int invincibleCount;
		public bool Invincible
		{
			get { return invincibleCount > 0; }
		}

		public BoundedStat Movespeed { get; set; }

		public float Shield { get; private set; }
		public BoundedStat ShieldMax { get; set; }
		public Stat ShieldRegen { get; set; }
		public float ShieldRegenDelay { get; set; }
		public BoundedStat ShieldRegenDelayMax { get; set; }
		#endregion

		#region EVENTS

		public ValueUpdateEvent update;
		public DamageEvent damageTaken;
		public DamageEvent damageDealt;
		public ValueUpdateEvent healed;
		public BasicEvent shieldDepleted;
		public BasicEvent shieldStartRecharge;
		public BasicEvent shieldRecharged;
		public BasicEvent death;
		#endregion
		#endregion

		#region STATIC_METHODS

		public static bool DealDamage(Entity victim, Entity attacker, float damage, DamageFlags flags = DamageFlags.None)
		{
			if (damage <= 0f)
				return false;

			if (victim == null)
				throw new NullReferenceException ("Cannot deal damage to null victim");

			CombatSnapshot.Builder snapshotBuilder = new CombatSnapshot.Builder ()
			{
				Victim = victim,
				Attacker = attacker
			};

			victim.combatTimer = COMBAT_TIMER_MAX;
			if (attacker != null)
				attacker.combatTimer = COMBAT_TIMER_MAX;

			bool hitShield = false;
			bool victimDied = false;

			if (!victim.Invincible)
			{
				//not ignoring shield, hit it
				if (victim.Shield > 0f && (DamageFlags.IgnoreShield & flags) != DamageFlags.IgnoreShield)
				{
					hitShield = true;
					float preDmgShield = victim.Shield;
					victim.Shield -= damage;
					if (victim.Shield < 0f)
					{
						damage -= preDmgShield;
						victim.Shield = 0f;
						victim.OnShieldDepleted (victim);
						snapshotBuilder.ShieldDamage = preDmgShield;
					}
					else
					{
						snapshotBuilder.ShieldDamage = damage;
					}

					victim.ShieldRegenDelay = victim.ShieldRegenDelayMax.GetValue ();
				}

				//either the shield wasn't hit, or pierce is set
				if (!hitShield || (DamageFlags.PierceShield & flags) == DamageFlags.PierceShield)
				{
					victim.Health -= damage;
					if (victim.Health < 0f)
					{
						victim.Health = 0f;
						victimDied = victim.OnDeath (victim);
					}
					snapshotBuilder.HealthDamage = damage;
				}
			}

			//TODO some event calls
			snapshotBuilder.VictimDied = victimDied;


			return victimDied;
		}
		#endregion

		#region INSTANCE_METHODS

		public Entity()
		{
			statuses = new HashSet<Status> ();
		}

		#region STATUS_HANDLING

		#endregion

		#region EVENTS

		public void OnUpdate(Entity subject, float dTime)
		{
			List<Status> expiredStatuses = new List<Status> ();
			foreach (Status s in statuses)
			{
				s.OnUpdate (subject, dTime);
				if (s.IsCompleted)
					expiredStatuses.Add (s);
			}

			update?.Invoke (this, dTime);
		}

		public void OnDamageTaken(CombatSnapshot snapshot)
		{
			foreach (Status s in statuses)
				s.OnDamageTaken (snapshot);

			damageTaken?.Invoke (snapshot);
		}

		public void OnDamageDealt(CombatSnapshot snapshot)
		{
			foreach (Status s in statuses)
				s.OnDamageDealt (snapshot);

			damageDealt?.Invoke (snapshot);
		}

		public void OnHealed(Entity subject, float amount)
		{
			foreach (Status s in statuses)
				s.OnHealed (subject, amount);

			healed?.Invoke (subject, amount);
		}

		public void OnShieldDepleted(Entity subject)
		{
			foreach (Status s in statuses)
				s.OnShieldDepleted (subject);

			shieldDepleted?.Invoke (subject);
		}

		public void OnShieldStartRecharge(Entity subject)
		{
			foreach (Status s in statuses)
				s.OnShieldStartRecharge (subject);

			shieldStartRecharge?.Invoke (subject);
		}

		public void OnShieldRecharged(Entity subject)
		{
			foreach (Status s in statuses)
				s.OnShieldRecharged (subject);

			shieldRecharged?.Invoke (subject);
		}

		public bool OnDeath(Entity subject)
		{
			bool continueDeath = true;
			foreach (Status s in statuses)
			{
				if (!s.OnDeath (subject))
					continueDeath = false;
			}

			if (continueDeath)
			{
				death?.Invoke (subject);
				return true;
			}
			return false;
		}
		#endregion
		#endregion

		#region INTERNAL_TYPES

		public delegate void BasicEvent(Entity subject);
		public delegate void ValueUpdateEvent(Entity subject, float amount);
		public delegate void DamageEvent(CombatSnapshot snapshot);
		#endregion
	}
}
