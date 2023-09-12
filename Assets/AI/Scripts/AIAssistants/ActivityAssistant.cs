using AI.Managers;
using AI.Units;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zenject;

namespace AI.Assistants
{
    public class ActivityAssistant
    {
		private IReadOnlyCollection<BotComponent> _enemies;
		
		//[Inject]
		
		private StateAIAssistant _aiAssistant;
		private ICoroutineDispatcher _dispatcher;
		private Transform _pool;
		private SignalBus _signal;

		[Inject]

		private AIManager _manager;

		private ActivityAssistant(LinkedList<BotComponent> bots, StateAIAssistant aiAssistant, ICoroutineDispatcher dispatcher, Transform pool, SignalBus signal)
		{
			_enemies = bots; _aiAssistant = aiAssistant; _dispatcher = dispatcher; _pool = pool; _signal = signal;
			_signal.SubscribeId<ActionResultInfo>("Die", OnDieResult);
		}

		public void OnDieResult(ActionResultInfo info)//todo
		{
			var removeUnits = new LinkedList<BotComponent>();
			var bot = info.Source as BotComponent;
			removeUnits.AddLast(bot);

			if (removeUnits.Count == 0) return;
			else
			{
				_manager.RemoveEnemies(removeUnits);
			}
		}

		private Transform GetNearBot(BotComponent curBot)//todo
		{
			Transform near = _pool;
			float distance = 50;
			foreach (BotComponent enemy in _enemies)
			{
				if (enemy == null || enemy.GetFabric == curBot.GetFabric) continue;
				float calc = Vector3.Distance(curBot.transform.position, enemy.transform.position);
				if (calc < distance)
				{
					distance = calc;
					near = enemy.transform;
				}
			}
			return near;
		}


		private IEnumerator CheckActivity(object[] parameters)// float distance, float delay)
		{
			var delay = (float)parameters[0];
			//var distanceActivation = (float)parameters[1];
			//var distanceFocus = (float)parameters[2];
			var distanceActivation = float.MaxValue;
			var distanceFocus = float.MaxValue;

			//distanceActivation *= distanceActivation;
			//distanceFocus *= distanceFocus;

			while (true)
			{
				foreach (var bot in _enemies)
				{
#if UNITY_EDITOR

					var last = bot.Activity;
#endif
					var neer_bot = GetNearBot(bot);
					var distance = Vector3.SqrMagnitude(bot.transform.position - neer_bot.position);
					bot.Activity = distance < distanceActivation;

#if UNITY_EDITOR
					if (last != bot.Activity) DebugManager.Log($"{bot.name} : changed its activity mode to '{bot.Activity}'", GetType());
#endif

					if (!bot.Activity) continue;

					//Если : игрок близко - бот триггерится
					if (!bot.Hunt || !(neer_bot == _pool))
					{
						//новая цель бота
						bot.Hunt = true;
						bot.Target = new TargetPoint(neer_bot);
						//Обновляем поведение бота
						_aiAssistant.UpdateStateAI(bot);
#if UNITY_EDITOR
						DebugManager.Log($"{bot.name} : triggered on '{neer_bot.name}'", GetType());
#endif
					}
					//Игрок убежал от бота, а бот взаимодействовал с ним
					if (distance >= distanceFocus && bot.Hunt)
					{
						_aiAssistant.ClearStateAI(bot);
#if UNITY_EDITOR
						DebugManager.Log($"{bot.name} : lost '{GetNearBot(bot).name}'", GetType());
#endif
					}
				}

				yield return new WaitForSeconds(delay);
			}
		}

		public void Execute(float intervalCheckActivity, float distanceActivation, float distanceFocus)
		{
			//var func = new Func<IEnumerator>(() => CheckActivity(distanceActivation, intervalCheckActivity));

			_dispatcher.ExecuteAsync(CheckActivity, new object[] { intervalCheckActivity, distanceActivation, distanceFocus }, this);
		}
	}
}
