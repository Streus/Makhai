namespace Makhai.Core.Data
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

		void OnStatusAdded(Status s);
		void OnStatusRemoved(Status s);
	}
}
