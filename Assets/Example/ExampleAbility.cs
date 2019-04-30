using System.Collections;
using Makhai.Core.Control;
using UnityEngine;

namespace Example
{
	public class ExampleAbility : Ability
	{
		protected override string GetName()
		{
			return "Example Ability";
		}

		protected override string GetDescription()
		{
			return "An example ability";
		}

		protected override Sprite GetIcon()
		{
			return null;
		}

		protected override float GetMaxCooldown()
		{
			return 1f;
		}

		protected override int GetMaxCharges()
		{
			return 0;
		}

		protected override bool InvokeStart(Controller subject)
		{
			Debug.Log(GetName() + " used");
			return true;
		}

		protected override IEnumerator InvokeContinue(Controller subject)
		{
			while(Control.GetControlContinue())
			{
				Debug.Log(GetName() + " channeling");
				yield return null;
			}
		}

		protected override void InvokeEnd(Controller subject)
		{
			Debug.Log(GetName() + " done");
		}
	}
}
