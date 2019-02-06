using Makhai.ComplexStats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Makhai.Core.Data
{
	[Serializable]
    public class Entity : MonoBehaviour, IEntityEventUser
    {
		#region STATIC_VARS

		public const float COMBAT_TIMER_MAX = 5f;
		#endregion

		#region INSTANCE_VARS
		[SerializeField]
		private Dictionary<string, Status> statuses;

		[NonSerialized]
		private float combatTimer;
		public bool InCombat
		{
			get
			{
				return combatTimer > 0f;
			}
		}

		/// <summary>
		/// Used to determine bullet hits/ignores.
		/// </summary>
		public Faction Affiliation { get { return faction; } }
		[SerializeField]
		private Faction faction;

		#region STATS

		// Health
		public float Health { get { return health; } private set { health = value; } }
		[SerializeField]
		private float health = 0f;

		public BoundedStat HealthMax { get { return healthMax; } set { healthMax = value; } }
		[SerializeField]
		private BoundedStat healthMax = new BoundedStat(new Stat(), 0f);

		public Stat HealthRegen { get { return healthRegen; } set { healthRegen = value; } } 
		[SerializeField]
		private Stat healthRegen = new Stat();

		// Shield
		public float Shield { get { return shield; } private set { shield = value; } }
		[SerializeField]
		private float shield = 0f;

		public BoundedStat ShieldMax { get { return shieldMax; } set { shieldMax = value; } }
		[SerializeField]
		private BoundedStat shieldMax = new BoundedStat(new Stat(), 0f);

		public Stat ShieldRegen { get { return shieldRegen; } set { shieldRegen = value; } }
		[SerializeField]
		private Stat shieldRegen = new Stat();

		public float ShieldRegenDelay { get { return shieldRegenDelay; } set { shieldRegenDelay = value; } }
		[SerializeField]
		private float shieldRegenDelay = 0f;

		public BoundedStat ShieldRegenDelayMax { get { return shieldRegenDelayMax; } set { shieldRegenDelayMax = value; } }
		[SerializeField]
		private BoundedStat shieldRegenDelayMax = new BoundedStat(new Stat(), 0f);

		// Misc
		public bool Invincible
		{
			get { return invincibleCount > 0; }
		}
		[SerializeField]
		private int invincibleCount;

		public BoundedStat Movespeed { get { return movespeed; } set{ movespeed = value; } } 
		[SerializeField]
		private BoundedStat movespeed = new BoundedStat(new Stat(), 0f);
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
			if (victim == null)
				throw new ArgumentNullException("Cannot deal damage to null victim");

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
			faction = Faction.Neutral;
			Health = HealthMax.GetValue ();
			Shield = ShieldMax.GetValue ();
		}

		private void Update()
		{
			OnUpdate(this, Time.deltaTime);
		}

		#region STATUS_HANDLING

		public void AddStatus(Status s)
		{
			if (s == null)
				throw new ArgumentNullException ("Cannot add null status.");

			Status existing;
			if (statuses.TryGetValue(s.Name, out existing))
			{
				existing.IncrementStackCount(this, 1);
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

			if (statuses.TryGetValue (name, out Status status))
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
				s.OnUpdate (this, dTime);
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
