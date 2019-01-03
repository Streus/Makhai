using System;
using UnityEngine;

namespace Makhai.Core.Bullets
{
	[Serializable]
	public abstract class BulletBehavior
	{
		public abstract GameObject GetPrefab();

		public virtual void OnStart(Bullet subject) { }
		public virtual void OnUpdate(Bullet subject) { }
		public virtual void OnFixedUpdate(Bullet subject) { }
		public virtual bool OnHit(Bullet subject, Collider other) { return true; }
		public virtual void OnDeath(Bullet subject) { }
	}
}
