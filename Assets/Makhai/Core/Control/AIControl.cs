using System;
using UnityEngine;

namespace Makhai.Core.Control
{
	/// <summary>
	/// Simulates a key-press-like control scheme for AI interaction with abilities.
	/// </summary>
	public class AIControl : IControlModule
	{
		private bool state;
		private bool lastState;

		private bool tapping;

		public AIControl()
		{
			state = lastState = false;
			tapping = false;
		}

		/// <summary>
		/// Toggle the control on.
		/// </summary>
		public void Press()
		{
			state = true;
		}

		/// <summary>
		/// Toggle the control off.
		/// </summary>
		public void Release()
		{
			state = false;
		}

		/// <summary>
		/// Toggle the control on and queue it up to toggle off on the next update.
		/// </summary>
		public void Tap()
		{
			state = true;
			tapping = true;
		}

		/// <summary>
		/// Refresh the state of the control.
		/// </summary>
		public void Update()
		{
			lastState = state;
			if(tapping)
			{
				Release();
				tapping = false;
			}
		}

		public bool GetControlStart()
		{
			return (state != lastState) && state;
		}

		public bool GetControlContinue()
		{
			return state;
		}

		public bool GetControlEnd()
		{
			return (state != lastState) && !state;
		}
	}
}
