using System;
using UnityEngine;

namespace Makhai.Core.Control
{
	[Serializable]
	public class KeyControl : IControlModule
	{
		[SerializeField]
		private KeyCode key;
		public KeyCode Key { get { return key; } set { key = value; } }

		public KeyControl(KeyCode key)
		{
			this.key = key;
		}

		public bool GetControlStart()
		{
			return Input.GetKeyDown(key);
		}

		public bool GetControlContinue()
		{
			return Input.GetKey(key);
		}

		public bool GetControlEnd()
		{
			return Input.GetKeyUp(key);
		}
	}
}
