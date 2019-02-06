using Makhai.ComplexStats;
using System;
using System.Collections;
using UnityEngine;

namespace Makhai.Core.Control
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
		public string Name { get { return name; } private set { name = value; } }
		[SerializeField]
		private string name;

		/// <summary>
		/// Ability is not usable if a cooldown is in progress.
		/// </summary>
		public Timer Cooldown { get { return cooldown; } private set { cooldown = value; } }
		[SerializeField]
		private Timer cooldown;

		/// <summary>
		/// The number of times this ability can be used while a cooldown is in progress.
		/// Is incremented by completing a cooldown.
		/// </summary>
		public int Charges { get { return charges; } private set { charges = value; } }
		[SerializeField]
		private int charges;

		/// <summary>
		/// The maximum number of charges this ability can accrue.
		/// </summary>
		public int ChargesMax { get { return chargesMax; } protected set { chargesMax = value; } }
		[SerializeField]
		private int chargesMax;

		/// <summary>
		/// Controls the update behavior of this ability. If false, update is ignored.
		/// </summary>
		public bool Active
		{
			get { return inactiveCount <= 0; }
			set { inactiveCount += value ? -1 : 1; if (inactiveCount < 0) inactiveCount = 0; }
		}
		[SerializeField]
		private int inactiveCount;

		/// <summary>
		/// Controls the invoke behavior of this ability. If false, use is ignored.
		/// </summary>
		public bool Available
		{
			get { return unavailableCount <= 0; }
			set { unavailableCount += value ? -1 : 1; if (unavailableCount < 0) unavailableCount = 0; }
		}
		[SerializeField]
		private int unavailableCount;

		public bool InUse { get => useRoutine != null; }

		public event UseStartCallback startUse;
		public event UseEndCallback endUse;

		private IEnumerator useRoutine;
		#endregion

		#region INSTANCE_METHODS

		protected Ability(string name, float cooldownMax, int chargesMax)
		{
			Name = name;
			Cooldown = new Timer (cooldownMax);
			ChargesMax = chargesMax;

			useRoutine = null;
		}

		public bool IsReady()
		{
			return Active && Available && (Cooldown.IsCompleted () || Charges > 0);
		}

		/// <summary>
		/// Called when invocation starts. Returns whether invocation should continue.
		/// </summary>
		/// <param name="subject">The controller on which to operate</param>
		/// <returns>Should invocation continue?</returns>
		protected virtual bool InvokeStart(Controller subject) { return true; }

		/// <summary>
		/// Main behavior loop of abilities that do not terminate in a single update (in InvokeStart).
		/// </summary>
		/// <param name="subject">The controller on which to operate</param>
		/// <returns></returns>
		protected virtual IEnumerator InvokeContinue(Controller subject) { yield break; }

		/// <summary>
		/// Called when InvokeContinue ends.
		/// </summary>
		/// <param name="subject">The controller on which to operate</param>
		protected virtual void InvokeEnd(Controller subject) { }

		/// <summary>
		/// Starts execution of the behavior of this ability.
		/// </summary>
		/// <param name="subject">The AI/Player data to use for invocation.</param>
		/// <returns></returns>
		public bool Use(Controller subject)
		{
			if (IsReady())
			{
				bool success = InvokeStart (subject);
				OnUseStart (subject, success);

				if (Charges > 0)
				{
					Charges--;
				}
				else if (Cooldown.IsCompleted () || Charges < ChargesMax)
				{
					Cooldown.Reset ();
				}

				//init continuing use behavior
				if (success)
				{
					useRoutine = InvokeContinue(subject);
				}

				return true;
			}
			return false;
		}

		#region EVENTS

		public void OnUpdate(Controller subject, float dTime)
		{
			//currently being used
			if (useRoutine != null)
			{
				bool hasNext;
				try
				{
					hasNext = useRoutine.MoveNext();
				}
				catch(Exception e)
				{
					Debug.LogException(e);
					hasNext = false;
				}

				if(!hasNext)
				{
					//use finished or duration completed
					useRoutine = null;
					OnUseEnd(subject);
				}
			}
			//not being used, either waiting to be used or cooling down
			else
			{
				if (Active && !Cooldown.IsCompleted())
				{
					if (Cooldown.Tick(dTime))
					{
						//finished cooldown
						Cooldown.Complete();
						if (Charges < ChargesMax)
						{
							Charges++;
							if (Charges < ChargesMax)
								Cooldown.Reset();
						}
					}
				}
			}
		}

		protected void OnUseStart(Controller subject, bool success)
		{
			startUse?.Invoke (subject, success);
		}

		protected void OnUseEnd(Controller subject)
		{
			endUse?.Invoke(subject);
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

		public delegate void UseStartCallback(Controller subject, bool success);
		public delegate void UseEndCallback(Controller subject);
		#endregion
	}
}
