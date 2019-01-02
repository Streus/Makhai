using Makhai.ComplexStats;
using System;

namespace Makhai.Core
{
	/// <summary>
	/// Invokeable behavior for use with an Entity.
	/// </summary>
	[Serializable]
	public abstract class Ability : INamedItem
	{
		#region INSTANCE_VARS

		/// <summary>
		/// Unique identifier for this ability.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Ability is unusable if a cooldown is in progress.
		/// </summary>
		public Timer Cooldown { get; private set; }

		/// <summary>
		/// The number of times this ability can be used while a cooldown is in progress.
		/// Is incremented by completing a cooldown.
		/// </summary>
		public int Charges { get; private set; }

		/// <summary>
		/// The maximum number of charges this ability can accrue.
		/// </summary>
		public int ChargesMax { get; protected set; }

		private int inactiveCount;
		/// <summary>
		/// Controls the update behavior of this ability. If false, update is ignored.
		/// </summary>
		public bool Active
		{
			get { return inactiveCount <= 0; }
			set { inactiveCount += value ? -1 : 1; }
		}

		private int unavailableCount;
		/// <summary>
		/// Controls the invoke behavior of this ability. If false, use is ignored.
		/// </summary>
		public bool Available
		{
			get { return unavailableCount <= 0; }
			set { unavailableCount += value ? -1 : 1; }
		}

		public event UseCallback used;
		#endregion

		#region INSTANCE_METHODS

		protected Ability(string name, float cooldownMax, int chargesMax)
		{
			Name = name;
			Cooldown = new Timer (cooldownMax);
			ChargesMax = chargesMax;
		}

		public bool IsReady()
		{
			return Active && Available && (Cooldown.IsCompleted () || Charges > 0);
		}

		protected abstract bool Invoke(Controller subject);

		/// <summary>
		/// Invokes the behavior of this ability
		/// </summary>
		/// <param name="subject">The AI/Player data to use for invocation.</param>
		/// <returns></returns>
		public bool Use(Controller subject)
		{
			if (IsReady())
			{
				bool success = Invoke (subject);
				OnUse (subject, success);

				if (Charges > 0)
				{
					Charges--;
				}
				else if (Cooldown.IsCompleted () || Charges < ChargesMax)
				{
					Cooldown.Reset ();
				}

				return success;
			}
			return false;
		}

		#region EVENTS

		public void OnUpdate(float dTime)
		{
			if (Active && !Cooldown.IsCompleted())
			{
				if (Cooldown.Tick (dTime))
				{
					Cooldown.Complete ();
					if (Charges < ChargesMax)
					{
						Charges++;
						if(Charges < ChargesMax)
							Cooldown.Reset ();
					}
				}
			}
		}

		protected void OnUse(Controller subject, bool success)
		{
			used?.Invoke (subject, success);
		}
		#endregion

		public override bool Equals(object obj)
		{
			return Name.Equals (((Ability)obj).Name);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode ();
		}

		public override string ToString()
		{
			return Name + " [Cooldown: " + Cooldown.ToString () + ", Charges: " + Charges + " / " + ChargesMax + "]";
		}
		#endregion

		#region INTERNAL_TYPES

		public delegate void UseCallback(Controller subject, bool success);
		#endregion
	}
}
