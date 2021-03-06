﻿namespace Makhai.Core
{
	interface IEntityEventUser
	{
		void OnUpdate(Entity subject, float dTime);

		void OnDamageTaken(CombatSnapshot snapshot);
		void OnDamageDealt(CombatSnapshot snapshot);
		void OnHealed(Entity subject, float amount);

		void OnShieldDepleted(Entity subject);
		void OnShieldStartRecharge(Entity subject);
		void OnShieldRecharged(Entity subject);

		bool OnDeath(Entity subject);
	}
}
