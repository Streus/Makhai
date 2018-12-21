using System;
using System.Collections.Generic;

namespace Makhai.Core
{
	[Serializable]
	public class Status : IEntityEventUser
	{
		public string Name { get; private set; }

		public float InitialDuration { get; private set; }
		public float RemainingDuration { get; set; }

		/// <summary>
		/// Percentage of remaining time / initial set time.
		/// </summary>
		public float Progress { get { return RemainingDuration / InitialDuration; } }

		/// <summary>
		/// Has the remaining time reached zero?
		/// </summary>
		public bool IsCompleted { get { return RemainingDuration <= 0f; } }

		private int stackCount;
		public int Stacks
		{
			get { return stackCount; }
			set
			{
				stackCount = Math.Min(value, StacksMax);
				for (int i = 0; i < components.Count; i++)
				{
					components[i].Stacks = stackCount;
				}
			}
		}

		public int StacksMax { get; set; }

		/// <summary>
		/// Collection of statuses that define this status's behaviour.
		/// </summary>
		private List<StatusComponent> components;

		public Status(float duration, params StatusComponent[] components)
		{
			InitialDuration = duration;
			RemainingDuration = InitialDuration;

			stackCount = 1;
			StacksMax = 1;

			this.components = new List<StatusComponent> (components);
		}

		public void OnApply(Entity subject)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnApply (subject);
		}

		public void OnRevert(Entity subject)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnApply (subject);
		}

		public void OnUpdate(Entity subject, float dTime)
		{
			RemainingDuration -= dTime;
			for (int i = 0; i < components.Count; i++)
			{
				components[i].OnUpdate (subject, dTime);
			}
		}

		public void OnDamageDealt(CombatSnapshot snapshot)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnDamageDealt (snapshot);
		}

		public void OnDamageTaken(CombatSnapshot snapshot)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnDamageTaken (snapshot);
		}

		public bool OnDeath(Entity subject)
		{
			bool continueDeath = true;
			for (int i = 0; i < components.Count; i++)
			{
				if (!components[i].OnDeath (subject))
					continueDeath = false;
			}
			return continueDeath;
		}

		public void OnHealed(Entity subject, float amount)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnHealed (subject, amount);
		}

		public void OnShieldDepleted(Entity subject)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnShieldDepleted (subject);
		}

		public void OnShieldRecharged(Entity subject)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnShieldRecharged (subject);
		}

		public void OnShieldStartRecharge(Entity subject)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnShieldStartRecharge (subject);
		}
	}
}
