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

		/// <summary>
		/// The Entity that created this bullet.
		/// </summary>
		public Entity Source { get; private set; }

		[SerializeField]
		private Timer lifetime;

		/// <summary>
		/// What factions will this bullet deal damage to?
		/// </summary>
		public Faction HitMask { get; set; }
		#endregion

		#region STATIC_METHODS

		public static Bullet Create(BulletBehavior behavior, Entity source, Vector3 position, Quaternion rotation)
		{
			GameObject inst = Instantiate (behavior.GetPrefab (), position, rotation);
			Bullet bullet = inst.GetComponent<Bullet> ();
			bullet.behavior = behavior;
			bullet.Source = source;
			bullet.lifetime = new Timer (behavior.GetLifetimeDuration ());
			bullet.HitMask = ~source.Affiliation;

			return bullet;
		}

		public static Bullet Create(BulletBehavior behavior, Entity source, Transform parent)
		{
			GameObject inst = Instantiate (behavior.GetPrefab (), parent, false);
			Bullet bullet = inst.GetComponent<Bullet> ();
			bullet.behavior = behavior;
			bullet.Source = source;
			bullet.lifetime = new Timer (behavior.GetLifetimeDuration ());
			bullet.HitMask = ~source.Affiliation;

			return bullet;
		}
		#endregion

		#region INSTANCE_METHODS

		private Bullet()
		{
			behavior = null;
			Source = null;
			lifetime = null;
		}

		#region EVENTS
		private void Awake()
		{
			
		}

		private void Start()
		{
			behavior?.OnStart (this);
		}

		private void Reset()
		{
			behavior = null;
			lifetime?.Reset ();
		}

		private void Update()
		{
			if (lifetime.IsCompleted ())
			{
				OnDeath ();
			}
			else
			{
				behavior?.OnUpdate (this);
				lifetime.Tick (Time.deltaTime);
			}
		}

		private void FixedUpdate()
		{
			behavior?.OnFixedUpdate (this);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!collision.collider.isTrigger)
			{
				Entity other = collision.gameObject.GetComponent<Entity> ();
				if ((HitMask & other.Affiliation) != 0)
				{
					OnHit (collision, other);
				}
			}
		}

		private void OnHit(Collision collision, Entity victim = null)
		{
			if ((behavior?.OnHit (this, collision, victim)).Value)
			{
				OnDeath ();
			}
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (!collision.collider.isTrigger)
			{
				Entity other = collision.gameObject.GetComponent<Entity> ();
				if ((HitMask & other.Affiliation) != 0)
				{
					OnHit2D (collision, other);
				}
			}
		}

		private void OnHit2D(Collision2D collision, Entity victim = null)
		{
			if ((behavior?.OnHit2D (this, collision, victim)).Value)
			{
				OnDeath ();
			}
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
