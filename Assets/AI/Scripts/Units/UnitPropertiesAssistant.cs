using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Units
{
	public class UnitPropertiesAssistant
	{
		private StatsData _params;


		public bool Immortal { get; set; }
		public float Health { get; private set; }
		public float FastAttackDamage => _params.BattleParams.FastAttackDamage;
		public float StrongAttackDamage => _params.BattleParams.StrongAttackDamage;

		public ref MobilityParamsData GetMobility => ref _params.MobilityParams;

		public bool SetDamage(float points)
		{
			if (Immortal) return false;

			Health -= points;

			return true;
		}

		internal UnitPropertiesAssistant(StatsData data)
		{
			_params = data;
			Health = _params.BaseParams.MaxHealth;
			
		}
	}
}
