using Makhai.ComplexStats;
using Makhai.Core.Data;
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
		public IControlModule Control { get; set; }

		/// <summary>
		/// Unique identifier for this ability.
		/// </summary>
		public string Name { get { return GetName(); } }

		/// <summary>
		/// Short text describing what this ability does.
		/// </summary>
		public string Description { get { return GetDescription(); } }

		/// <summary>
		/// Graphical representation of this ability for use in UI.
		/// </summary>
		public Sprite Icon { get { return GetIcon(); } }

		/// <summary>
		/// Ability is not usable if a cooldown is in progress.
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
		public int ChargesMax { get { return GetMaxCharges(); } }
		
		/// <summary>
		/// Controls the update behavior of this ability. If false, update is ignored.
		/// </summary>
		public bool Active
		{
			get { return inactiveCount <= 0; }
			set { inactiveCount += value ? -1 : 1; if (inactiveCount < 0) inactiveCount = 0; }
		}
		private int inactiveCount;

		/// <summary>
		/// Controls the invoke behavior of this ability. If false, use is ignored.
		/// </summary>
		public bool Available
		{
			get { return unavailableCount <= 0; }
			set { unavailableCount += value ? -1 : 1; if (unavailableCount < 0) unavailableCount = 0; }
		}
		private int unavailableCount;

		public bool InUse { get => useRoutine != null; }

		public event UseStartCallback startUse;
		public event UseEndCallback endUse;

		private IEnumerator useRoutine;
		#endregion

		#region INSTANCE_METHODS

		protected Ability()
		{
			this.Control = null;

			Cooldown = new Timer(GetMaxCooldown());
			Charges = 0;
			inactiveCount = unavailableCount = 0;

			useRoutine = null;
		}

		protected abstract string GetName();
		protected abstract string GetDescription();
		protected abstract Sprite GetIcon();
		protected abstract float GetMaxCooldown();
		protected abstract int GetMaxCharges();

		public bool IsReady()
		{
			return Active && Available && (Cooldown.IsCompleted () || Charges > 0);
		}

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
					InvokeEnd(subject);
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
				else if(IsReady() && IsInvoked())
				{
					Use(subject);
				}
			}
		}

		/// <summary>
		/// Polls the control for the "set-off" condition(s) to start invocation.
		/// </summary>
		/// <returns></returns>
		protected virtual bool IsInvoked()
		{
			bool? invoked = Control?.GetControlStart();
			return invoked.HasValue ? invoked.Value : false;
		}

		/// <summary>
		/// Called when invocation starts. Returns whether invocation should continue.
		/// </summary>
		/// <param name="subject">The controller on which to operate</param>
		/// <returns>Should invocation continue?</returns>
		protected virtual bool InvokeStart(Controller subject) { return false; }

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
