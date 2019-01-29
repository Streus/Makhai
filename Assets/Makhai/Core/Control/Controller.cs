using Makhai.Core.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Makhai.Core.Control
{
	/// <summary>
	/// Base class for AI/Player control managers.
	/// </summary>
	public abstract class Controller : MonoBehaviour
	{
		#region INSTANCE_VARS

		[SerializeField]
		private Entity self = new Entity ();

		[SerializeField]
		protected List<Ability> abilityList;
		#endregion

		#region INSTANCE_METHODS

		private void Update()
		{
			self.OnUpdate (null, Time.deltaTime);
			for(int i = 0; i < abilityList.Count; i++)
			{
				abilityList[i].OnUpdate(this, Time.deltaTime);
			}
		}

		public bool UseAbility(int index)
		{
			return abilityList[index].Use(this);
		}

		public bool UseAbility(string name)
		{
			return UseAbility ((Ability other) => { return other.Name == name; });
		}

		public bool UseAbility(SearchPredicate searchMethod)
		{
			foreach (Ability a in abilityList)
			{
				if (a != null && searchMethod (a))
				{
					return a.Use (this);
				}
			}
			throw new ArgumentException ("Failed to find ability matching predicate");
		}

		/// <summary>
		/// Get the Entity data tied to this controller.
		/// </summary>
		/// <returns></returns>
		public Entity GetEntity()
		{
			return self;
		}

		/// <summary>
		/// Get a location that is the focus of some AI/Player action.
		/// </summary>
		/// <returns></returns>
		public virtual Vector3 GetTargetPosition()
		{
			return Vector3.zero;
		}
		#endregion

		#region INTERNAL_TYPES

		public delegate bool SearchPredicate(Ability other);
		#endregion
	}
}
