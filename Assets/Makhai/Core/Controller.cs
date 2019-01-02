using UnityEngine;

namespace Makhai.Core
{
	/// <summary>
	/// Base class for AI/Player control managers.
	/// </summary>
	public abstract class Controller : MonoBehaviour
	{
		#region INSTANCE_VARS

		[SerializeField]
		private Entity self = new Entity ();
		#endregion

		#region INSTANCE_METHODS

		private void Update()
		{
			self.OnUpdate (null, Time.deltaTime);
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
	}
}
