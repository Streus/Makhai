using Makhai.ComplexStats;
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
		private Dictionary<string, Status> statuses;

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
		public StatusEvent statusAdded;
		public StatusEvent statusRemoved;
		#endregion
		#endregion

		#region STATIC_METHODS

		/// <summary>
		/// Deals damage to the indicated victim from the indicated attacker.
		/// </summary>
		/// <param name="victim"></param>
		/// <param name="attacker"></param>
		/// <param name="damage"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static CombatSnapshot DealDamage(Entity victim, Entity attacker, float damage, DamageFlags flags = DamageFlags.None)
		{
			CombatSnapshot.Builder snapshotBuilder = new CombatSnapshot.Builder ()
			{
				Victim = victim,
				Attacker = attacker
			};

			if (damage <= 0f)
			{
				snapshotBuilder.HealthDamage = 0f;
				snapshotBuilder.ShieldDamage = 0f;
				snapshotBuilder.VictimDied = false;
				return snapshotBuilder.Build ();
			}

			if (victim == null)
				throw new NullReferenceException ("Cannot deal damage to null victim");

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

			snapshotBuilder.VictimDied = victimDied;

			CombatSnapshot snapshot = snapshotBuilder.Build ();

			victim.OnDamageTaken (snapshot);
			attacker?.OnDamageDealt (snapshot);

			return snapshot;
		}
		#endregion

		#region INSTANCE_METHODS

		public Entity()
		{
			statuses = new Dictionary<string, Status> ();
		}

		#region STATUS_HANDLING

		public void AddStatus(Status s)
		{
			if (s == null)
				throw new ArgumentNullException ("Cannot add null status.");

			Status existing;
			if (statuses.TryGetValue(s.Name, out existing))
			{
				existing.Stacks++;
			}
			else
			{
				statuses.Add (s.Name, s);
				s.OnApply (this);
				statusAdded?.Invoke (s);
			}
		}

		public bool RemoveStatus(Status s)
		{
			return RemoveStatus (s.Name);
		}

		public bool RemoveStatus(string name)
		{
			if (name == null || name == "")
				throw new ArgumentNullException ("Status name cannot be null or empty.");

			Status status;
			if (statuses.TryGetValue (name, out status))
			{
				statuses.Remove (name);
				statusRemoved?.Invoke (status);
				return true;
			}

			return false;
		}

		public void ClearStatuses()
		{
			foreach (Status s in statuses.Values)
			{
				s.Duration.Complete ();
				s.OnRevert (this);

				statusRemoved?.Invoke (s);
			}

			statuses.Clear ();
		}

		public bool HasStatus(Status s)
		{
			return HasStatus (s.Name);
		}

		public bool HasStatus(string name)
		{
			return statuses.ContainsKey (name);
		}

		public Dictionary<string, Status>.ValueCollection GetStatusList()
		{
			return statuses.Values;
		}
		#endregion

		#region EVENTS

		public void OnUpdate(Entity subject, float dTime)
		{
			if (ShieldRegenDelay > 0)
			{
				//decrement delay timer
				ShieldRegenDelay -= dTime;
				if (ShieldRegenDelay <= 0)
					OnShieldStartRecharge (this);
			}
			else
			{
				ShieldRegenDelay = 0;
				if (Shield < ShieldMax.GetValue())
				{
					//add to shield up to ShieldMax
					Shield += ShieldRegen.GetValue () * dTime;
					if (Shield >= ShieldMax.GetValue())
					{
						Shield = ShieldMax.GetValue ();
						OnShieldRecharged (this);
					}
				}
			}

			//update all statuses, taking note of those that complete their durations
			Queue<Status> expiredStatuses = new Queue<Status> ();
			foreach (Status s in statuses.Values)
			{
				s.OnUpdate (subject, dTime);
				if (s.Duration.IsCompleted())
					expiredStatuses.Enqueue (s);
			}

			//remove all statuses that have competed their durations
			while (expiredStatuses.Count > 0)
				RemoveStatus (expiredStatuses.Dequeue ());

			update?.Invoke (this, dTime);
		}

		public void OnDamageTaken(CombatSnapshot snapshot)
		{
			foreach (Status s in statuses.Values)
				s.OnDamageTaken (snapshot);

			damageTaken?.Invoke (snapshot);
		}

		public void OnDamageDealt(CombatSnapshot snapshot)
		{
			foreach (Status s in statuses.Values)
				s.OnDamageDealt (snapshot);

			damageDealt?.Invoke (snapshot);
		}

		public void OnHealed(Entity subject, float amount)
		{
			foreach (Status s in statuses.Values)
				s.OnHealed (subject, amount);

			healed?.Invoke (subject, amount);
		}

		public void OnShieldDepleted(Entity subject)
		{
			foreach (Status s in statuses.Values)
				s.OnShieldDepleted (subject);

			shieldDepleted?.Invoke (subject);
		}

		public void OnShieldStartRecharge(Entity subject)
		{
			foreach (Status s in statuses.Values)
				s.OnShieldStartRecharge (subject);

			shieldStartRecharge?.Invoke (subject);
		}

		public void OnShieldRecharged(Entity subject)
		{
			foreach (Status s in statuses.Values)
				s.OnShieldRecharged (subject);

			shieldRecharged?.Invoke (subject);
		}

		public bool OnDeath(Entity subject)
		{
			bool continueDeath = true;
			foreach (Status s in statuses.Values)
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

		public void OnStatusAdded(Status newStatus)
		{
			foreach (Status s in statuses.Values)
				s.OnStatusAdded (s);

			statusAdded?.Invoke (newStatus);
		}

		public void OnStatusRemoved(Status newStatus)
		{
			foreach (Status s in statuses.Values)
				s.OnStatusRemoved (newStatus);

			statusRemoved?.Invoke (newStatus);
		}
		#endregion
		#endregion

		#region INTERNAL_TYPES

		public delegate void BasicEvent(Entity subject);
		public delegate void ValueUpdateEvent(Entity subject, float amount);
		public delegate void DamageEvent(CombatSnapshot snapshot);
		public delegate void StatusEvent(Status s);
		#endregion
	}
}
