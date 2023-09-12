using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Managers
{
    public class CoroutineDispatcher : MonoBehaviour, ICoroutineDispatcher
    {
		private Dictionary<int, List<Coroutine>> _office = new Dictionary<int, List<Coroutine>>();


		/// <summary>
		/// Проверяет работает-ли еще корутина
		/// </summary>
		/// <param name="sender">Отправитель метода</param>
		/// <param name="index">Идентификатор запроса</param>
		/// <returns>true - если корутина активна</returns>
		public bool IsWork(object sender, int index)
		{
			return _office[sender.GetHashCode()][index] != null;
		}

		/// <summary>
		/// Останавливает корутину для не Unity сущности
		/// </summary>
		/// <param name="sender">Отправитель метода</param>
		/// <param name="index">Идентификатор запроса</param>
		public void Stop(object sender, int index)
		{
			if(IsWork(sender, index))
			{
				StopCoroutine(_office[sender.GetHashCode()][index]);
				_office[sender.GetHashCode()][index] = null;
			}
		}

		/// <summary>
		/// Запускает корутину из не Unity сущности
		/// </summary>
		/// <param name="action">Метод-корутина</param>
		/// <param name="args">Массив аргументов корутины</param>
		/// <param name="sender">Отправитель метода</param>
		/// <returns>Идентификатор для контроля корутины</returns>
		public int ExecuteAsync(Func<object[], IEnumerator> action, object[] args, object sender)
		{
			//Получаем уникальный хэш-код
			var code = sender.GetHashCode();
			//Если в офисе еще не зарезервирован данных объект - добавляем
			if(!_office.ContainsKey(code))
			{
				var l = new List<Coroutine>();
				l.Add(null);
				_office.Add(code, l);
			}

			var list = _office[code];
			//Находим первый свободный стол 
			int i = 0;
			for(; i < list.Count; i++)
			{
				//Заканчиваем поиск
				if (list[i] == null) break;
			}
			//Если не нашли свободное место - добавили новое
			if (i > list.Count) list.Add(null);

			list[i] = StartCoroutine(Execute(action, args, list, i));
			return i;
		}

		private IEnumerator Execute(Func<object[], IEnumerator> action, object[] args, List<Coroutine> list, int index)
		{
			yield return action.Invoke(args);

			list[index] = null;
		}
    }
}
