using Makhai.Core.Control;
using System.Collections.Generic;
using UnityEngine;

namespace Example
{
	public class PlayerController : Controller
	{
		private AIControl abilityControl;

		public void Awake()
		{
			abilityControl = new AIControl();
			abilityList.Add(new ExampleAbility() { Control = abilityControl });
		}

		protected override void Update()
		{
			base.Update();
			if(Random.value < 0.9)
			{
				abilityControl.Press();
			}
			else
			{
				abilityControl.Release();
			}
		}
	}
}
