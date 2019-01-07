using Makhai.Core.Data;
using System;
using UnityEngine;

namespace Makhai.Core.Bullets
{
	[Serializable]
	public abstract class BulletBehavior
	{
		public abstract GameObject GetPrefab();
		public abstract float GetLifetimeDuration();

		public virtual void OnStart(Bullet subject) { }
		public virtual void OnUpdate(Bullet subject) { }
		public virtual void OnFixedUpdate(Bullet subject) { }
		public virtual bool OnHit(Bullet subject, Collision other, Entity victim) { return true; }
		public virtual bool OnHit2D(Bullet subject, Collision2D other, Entity victim) { return true; }
		public virtual void OnDeath(Bullet subject) { }
	}
}
