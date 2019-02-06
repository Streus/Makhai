using Makhai.ComplexStats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Makhai.Core.Data
{
	[Serializable]
	public class Status : IEntityEventUser, INamedItem
	{
		#region STATIC_VARS

		public const int STACK_DECAY_RATE_COMMUNAL = -1;
		public const int STACK_DECAY_RATE_NONE = 0;
		public const int STACK_DECAY_RATE_SERIAL = 1;
		#endregion

		#region INSTANCE_VARS
		public string Name { get { return name; } private set { name = value; } }
		[SerializeField]
		private string name;

		public Timer Duration { get { return duration; } private set { duration = value; } }
		[SerializeField]
		private Timer duration;
		
		[SerializeField]
		private int stackCount;

		/// <summary>
		/// The maximum number of stacks this status can accrue.
		/// </summary>
		public int StacksMax { get { return stacksMax; } protected set { stacksMax = value; } }
		[SerializeField]
		private int stacksMax;

		/// <summary>
		/// Decrements Stacks when the duration is completed. If negative, Stacks will be set
		/// to zero rather than decaying.
		/// </summary>
		public int StackDecayRate { get { return stackDecayRate; } protected set { stackDecayRate = value; } }
		[SerializeField]
		private int stackDecayRate;

		/// <summary>
		/// Collection of statuses that define this status's behaviour.
		/// </summary>
		[SerializeField, HideInInspector]
		private List<StatusComponent> components;
		#endregion

		#region INSTANCE_METHODS

		public Status(string name, float duration, int stacksMax, int stackDecayRate, params StatusComponent[] components)
		{
			if (name == null || name == "")
				throw new ArgumentNullException ("Status name cannot be null or empty.");

			Name = name;

			Duration = new Timer (duration);

			stackCount = 1;
			StacksMax = stacksMax;
			StackDecayRate = stackDecayRate;

			this.components = new List<StatusComponent> (components);
		}

		public int GetStackCount()
		{
			return stackCount;
		}

		public void SetStackCount(Entity subject, int stacks)
		{
			int old = stackCount;
			stackCount = Math.Min(stacks, StacksMax);
			OnStacksChanged(subject, old, stackCount);
		}

		public void IncrementStackCount(Entity subject, int dStacks)
		{
			SetStackCount(subject, stackCount + dStacks);
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

		private void OnStacksChanged(Entity subject, int oldStacks, int newStacks)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnStacksChanged(subject, oldStacks, newStacks);
		}

		public void OnUpdate(Entity subject, float dTime)
		{
			if (Duration.Tick(dTime))
			{
				//if SDS is negative, set stacks to zero
				SetStackCount(subject, stackCount - StackDecayRate < 0 ? stackCount : StackDecayRate);
				if (stackCount > 0)
				{
					Duration.Reset ();
				}
			}

			//update components
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

		public void OnStatusAdded(Status s)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnStatusAdded (s);
		}

		public void OnStatusRemoved(Status s)
		{
			for (int i = 0; i < components.Count; i++)
				components[i].OnStatusRemoved (s);
		}

		public override bool Equals(object obj)
		{
			return Name.Equals (((Status)obj).Name);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode ();
		}
		#endregion
	}
}
