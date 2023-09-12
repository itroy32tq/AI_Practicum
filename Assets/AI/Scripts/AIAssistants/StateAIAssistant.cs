using AI.Units;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Zenject;

namespace AI.Assistants
{
    /// <remarks>
    /// 1. Определяет группу и действие для бота
    /// 2. Изменяет приоритеты действий
    /// </remarks>
    public class StateAIAssistant
    {
        private IReadOnlyDictionary<ActionResultType, float> _weights;
        private MoveAssistant _moveAssistant;
        private IReadOnlyCollection<BotComponent> _bots;
        private SignalBus _signal;

        /// <summary>
        /// Сбрасывает бота на начальное состояние
        /// </summary>
        /// <param name="bot">Бот</param>
        public void ClearStateAI(BotComponent bot)
        {
            bot.RemoveStatusByName("system_Wander");
            bot.RemoveStatusByName("system_StateAI");
            bot.Hunt = false;
            bot.SteeringType = SteeringBehavioursType.Wander;
		}

        /// <summary>
        /// Обновление текущего действия у бота
        /// </summary>
        /// <param name="bot">Бот</param>
        public void UpdateStateAI(BotComponent bot)
        {
            var max = float.MinValue;
            var bytes = new LinkedList<byte>();

            //Определение приоритетного действия
            var currentActionWeights = bot.CurrentActionWeightsAI;

            //Проходим по всем доступным боту действиям
            foreach (var type in currentActionWeights.Keys)
            {
                //Находим самую приоритетное действие у бота
                if (max < currentActionWeights[type])
                {
                    max = currentActionWeights[type];
                    bytes.Clear();
                    bytes.AddLast((byte)type);
                }
                //Равноприоритетные действия добавляются
                else if (max == currentActionWeights[type])
                {
                    bytes.AddLast((byte)type);
                }
            }
            //Проверяем, нужно-ли перемещение
            var actionType = (ActionType)GetRandomByte(bytes);
            bot.SteeringType = _moveAssistant.CheckCorrectDistance(bot, actionType);

            //Определяем действие бота
            if (bot.SteeringType == SteeringBehavioursType.Idle)
            {
                bot.CurrentActionType = (ActionType)GetRandomByte(bytes);
            }
            else
            {
                bot.CurrentActionType = ActionType.MOVE;
            }
        }


        private void OnActionResult(ActionResultInfo info)
        {
#if UNITY_EDITOR
            try
            {
#endif
                var bot = info.Source as BotComponent;

                //Изменение приоритетов бота на основе действий игрока
                if (bot == null) UpdatePriority(info);
                //Изменение приоритетов бота на основе своих действий
                else
                {
                    //Мы можем прервать базовое действие, например MOVE
                    if(bot.CurrentActionWeightsAI.ContainsKey(info.Type))
                        bot.CurrentActionWeightsAI[info.Type] += bot.DeltaActionWeightsAI[info.Type] * _weights[info.Result];
                    UpdateStateAI(bot);
                }
#if UNITY_EDITOR
			}
			finally
            {
                DebugManager.Log($"{info.Source} called signal with args: {info.Type} | {info.Result}", GetType());
            }
#endif
		}

        //Обновление приоритета действия бота по результату действия игрока
        private void UpdatePriority(ActionResultInfo info)
        {
            //Все коэффициенты, которые содержат связаны с данным действием игрока и текущим его результатом
            var coefs = AIUtility.GetPlayerActionCoefs.Where(t => t.PlayerActions.Contains(info.Type) && t.Results.Contains(info.Result));

            var target = info.Target as BotComponent;
            //Если : действие игрока не связано ни с какой целью
            if (target == null) target = _moveAssistant.FindNearestBotByUnit(info.Source);

            //Проходка по всем данным
            foreach (var coef in coefs)
            {   
                //Проходка по всем действиям бота
                foreach (var action in coef.BotActions)
                {
                    //Проверяем, что данный юнит имеет в своих способностях запрашиваемое действие
                    if(target.CurrentActionWeightsAI.ContainsKey(action))
                        target.CurrentActionWeightsAI[action] += target.DeltaActionWeightsAI[action] * coef.Coef;
                }
            }
        }

		private void OnDieResult(ActionResultInfo info)
        {
            //todo вызывать старт анимации смерти
            //оповещать менеджера АИ и смерти
        }



        private byte GetRandomByte(LinkedList<byte> list)
        {
            var random = Random.Range(0, list.Count);

            int i = 0;
            foreach(var it in list)
            {
                if (i == random) return it;
                i++;
			}

            throw new System.ApplicationException("Ошибка в поиске случайного действия");
		}

        public void SetConfiguration(IReadOnlyDictionary<ActionResultType, float> weights)
        {
            _weights = weights;
        }

        private StateAIAssistant(LinkedList<BotComponent> bots, MoveAssistant moveAssistant, SignalBus signal)
        {
            _bots = bots; _moveAssistant = moveAssistant; _signal = signal;

            _signal.SubscribeId<ActionResultInfo>("Unit", OnActionResult);
            //_signal.SubscribeId<ActionResultInfo>("Die", OnDieResult);
        }
	}
}
