using Makhai.ComplexStats;
using Makhai.Core.Data;
using UnityEngine;

namespace Makhai.Core.Bullets
{
	/// <summary>
	/// A gameplay element that is used to perform some Entity damage interactions.
	/// </summary>
	[AddComponentMenu("Makhai/Bullet")]
	public class Bullet : MonoBehaviour
	{
		#region INSTANCE_VARS

		[SerializeField]
		private BulletBehavior behavior;

		[SerializeField]
		private Timer lifetime;
		#endregion

		#region STATIC_METHODS

		public static Bullet Create(BulletBehavior behavior, Entity source, Vector3 position, Quaternion rotation)
		{
			//TODO maybe a builder instead???
		}
		#endregion

		#region INSTANCE_METHODS

		public Bullet()
		{
			behavior = null;
			lifetime = new Timer (float.PositiveInfinity);
		}

		#region EVENTS
		private void Awake()
		{
			lifetime.Reset ();
		}

		private void Start()
		{
			behavior?.OnStart (this);
		}

		private void Update()
		{
			lifetime.Tick (Time.deltaTime);
			behavior?.OnUpdate (this);
		}

		private void FixedUpdate()
		{
			behavior?.OnFixedUpdate (this);
		}

		private bool OnHit(Collider other)
		{
			return (behavior?.OnHit (this, other)).Value;
		}

		private void OnDeath()
		{
			behavior.OnDeath (this);
			lifetime.Complete ();
			Destroy (gameObject);
		}

		private void OnDestroy()
		{
			
		}
		#endregion
		#endregion
	}
}
