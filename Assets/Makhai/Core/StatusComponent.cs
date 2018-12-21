using System;

namespace Makhai.Core
{
	[Serializable]
	public class StatusComponent : IEntityEventUser
	{
		public int Stacks { get; set; }

		public virtual void OnApply(Entity subject)
		{

		}

		public virtual void OnRevert(Entity subject)
		{

		}

		public virtual void OnUpdate(Entity subject, float dTime)
		{

		}

		public virtual void OnDamageDealt(CombatSnapshot snapshot)
		{

		}

		public virtual void OnDamageTaken(CombatSnapshot snapshot)
		{

		}

		public virtual bool OnDeath(Entity subject)
		{
			return true;
		}

		public virtual void OnHealed(Entity subject, float amount)
		{

		}

		public virtual void OnShieldDepleted(Entity subject)
		{

		}

		public virtual void OnShieldRecharged(Entity subject)
		{

		}

		public virtual void OnShieldStartRecharge(Entity subject)
		{

		}
	}
}
