using AI.Units;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace AI.Assistants
{
    public class MoveAssistant
    {
        private IReadOnlyCollection<BotComponent> _bots;
        private StateAIAssistant _aiAssistant;

        private float _distanceActivation;
        private float _rangeWander;
        private float _wanderStatusDuration;
        private float _minVelocity;
        private float _sqrArrivalDistance;

        /// <summary>
        /// Проверка расстояния для действия
        /// </summary>
        /// <param name="bot">Проверяемый бот</param>
        public SteeringBehavioursType CheckCorrectDistance(BotComponent bot, ActionType type)
        {
            Interval<float> interval = bot.GetExpectedInterval(type);

            //Для неопределенных в дистанции действий не нужно перемещаться
            if (interval == Interval<float>.FloatIndefinite)
            {
                //UpdateMovementGroup(bot);
                bot.SetVelocity(Vector3.zero);
                return SteeringBehavioursType.Idle;
            }

            //Расстояние между ботом и его целью
            var sqrRange = Vector3.SqrMagnitude(bot.transform.position - bot.Target.GetPoint);
            var sqrInterval = interval.Pow();

            SteeringBehavioursType moveType = default(SteeringBehavioursType);
            //Бот не достает - нужно подойти
            if (sqrRange > sqrInterval.Max) moveType = SteeringBehavioursType.Seek;
            //Бот слишком близко, нужно отойти
            else if (sqrRange < sqrInterval.Min) moveType = SteeringBehavioursType.Flee;

            return moveType;
		}

        /// <summary>
        /// Покадровое перемещение всех ботов
        /// </summary>
        public void OnMoves()
        {
            OnSeek(_bots.Where(t => t.SteeringType == SteeringBehavioursType.Seek && t.Activity));
            OnFlee(_bots.Where(t => t.SteeringType == SteeringBehavioursType.Flee && t.Activity));
            OnWander(_bots.Where(t => t.SteeringType == SteeringBehavioursType.Wander && t.Activity));
		}

        private void OnSeek(IEnumerable<BotComponent> bots)
        {
            foreach (var bot in bots)
            {
#if UNITY_EDITOR
                if (bot.Target == TargetPoint.Indefinite)
                {
                    DebugManager.Log("Попытка передвинуть юнита с неопределенной целью : " + bot.name, GetType());
                    UnityEditor.EditorApplication.isPaused = true;
                }
#endif
                var targetPosition = bot.Target.GetPoint;
                //targetPosition.y = bot.transform.position.y;

                //Сила стремления к цели
                var desVelocity = targetPosition - bot.transform.position;

                //Квадрат расстояния до цели
                var sqrLength = desVelocity.sqrMagnitude;

                //Если : расстояние мало - не нужно двигаться
                if (sqrLength < _sqrArrivalDistance)
                {
                    ActivateAction(bot);
                    continue;
                }

                var data = bot.GetSteeringData;
                desVelocity = desVelocity.normalized * data.MaxVelocity;
                //Если : мы находимся в зоне приближения
                if (sqrLength < data.DistanceArrival * data.DistanceArrival)
                    desVelocity = desVelocity * sqrLength / data.DistanceArrival;

                //Персонаж стоит на месте
                if(sqrLength <= _minVelocity) 
                {
                    ActivateAction(bot);
                    continue;
				}

                //Добавляемая сила к текущей
                var steering = desVelocity - bot.GetVelocity();

                //steering.y = bot.transform.position.y;
                //Ограничение силы и учет массы
                steering = Vector3.ClampMagnitude(steering, data.MaxVelocity) / bot.Mass;
                //Расчет общей силы движения
                var velocity = Vector3.ClampMagnitude(bot.GetVelocity(IgnoreAxisType.None) + steering, data.Speed);

                bot.SetVelocity(velocity, IgnoreAxisType.None);
            }
		}

        private void OnFlee(IEnumerable<BotComponent> bots)
        {
            foreach(var bot in bots)
            {
#if UNITY_EDITOR
                if (bot.Target == TargetPoint.Indefinite)
                {
                    DebugManager.Log("Попытка передвинуть юнита с неопределенной целью : " + bot.name, GetType());
                    UnityEditor.EditorApplication.isPaused = true;
                }
#endif
                var targetPosition = bot.Target.GetPoint;
                var data = bot.GetSteeringData;

                //Сила стремления к цели
                var desVelocity = bot.transform.position - targetPosition;
                //Квадрат расстояния до цели
                var sqrLength = desVelocity.sqrMagnitude;

                desVelocity = desVelocity.normalized * data.MaxVelocity;
                //Если : мы находимся вне зоны приближения
                if (sqrLength > data.DistanceAvoid * data.DistanceAvoid)
                    desVelocity = desVelocity * sqrLength / data.DistanceAvoid;//todo здесь будет замедляться при отдалении

                //Персонаж стоит на месте
                if (desVelocity.sqrMagnitude <= _minVelocity)
                {
                    ActivateAction(bot);
                    continue;
                }

                //Добавляемая сила к текущей
                var steering = desVelocity - bot.GetVelocity();
                //Ограничение силы и учет массы
                steering = Vector3.ClampMagnitude(steering, data.MaxVelocity) / bot.Mass;
                //Расчет общей силы движения
                var velocity = Vector3.ClampMagnitude(bot.GetVelocity(IgnoreAxisType.None) + steering, data.Speed);

                bot.SetVelocity(velocity);
            }
		}

        private void ActivateAction(BotComponent bot)
        {
            bot.SetVelocity(Vector3.zero);
            if (bot.Hunt) _aiAssistant.UpdateStateAI(bot);
        }

        private void OnWander(IEnumerable<BotComponent> bots)
        {
            foreach (var bot in bots)
            {
                var status = bot.TryToGetStatus("system_Wander");

                //Статус блуждания не найден
                if (status == null)
                {
                    var point = bot.transform.position + Random.insideUnitSphere * _rangeWander;
                    point.y = bot.transform.position.y;

                    bot.AddStatus(new StatusDataArgs("system_Wander", _wanderStatusDuration, bot, bot, StatusType.Single)); 
                    bot.Target = new TargetPoint(point);
				}
			}
            //Обычное стремление к цели. Блуждание имитируется регулярной сменой цели
            OnSeek(bots);
		}

        internal void SetParams(StateAIAssistant assistant, float distanceActivation, float rangeWander, float wanderStatusDuration, float minVelocity, float sqrArrivalDistance)
        {
            _aiAssistant = assistant; _distanceActivation = distanceActivation; 
            _rangeWander = rangeWander; _wanderStatusDuration = wanderStatusDuration; 
            _minVelocity = minVelocity; _sqrArrivalDistance = sqrArrivalDistance;
        }

        internal BotComponent FindNearestBotByUnit(BaseUnit unit)
        {
            var distance = _distanceActivation;
            BotComponent target = null;

            foreach(var bot in _bots)
            {
                var range = Vector3.Distance(bot.transform.position, unit.transform.position);
                if (range < distance)
                {
                    distance = range;
                    target = bot;
				}
			}

            //Если не найдены активированные юниты - то возвращаем нуль
            return target;
		}

        private MoveAssistant(LinkedList<BotComponent> bots)
        {
            _bots = bots;
        }
    }
}
