using System.Collections;
using Makhai.Core.Control;
using UnityEngine;

namespace Example
{
	public class ExampleAbility : Ability
	{
		public ExampleAbility() : base("Example", 1f, 0) { }

		protected override bool InvokeStart(Controller subject)
		{
			Debug.Log(Name + " used");
			return true;
		}

		protected override IEnumerator InvokeContinue(Controller subject)
		{
			while(Control.GetControlContinue())
			{
				Debug.Log(Name + " channeling");
				yield return null;
			}
		}

		protected override void InvokeEnd(Controller subject)
		{
			Debug.Log(Name + " done");
		}
	}
}
