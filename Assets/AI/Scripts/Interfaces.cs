using AI.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public interface ICoroutineDispatcher
    {
        /// <summary>
        /// Запускает корутину из не Unity сущности
        /// </summary>
        /// <param name="action">Метод-корутина</param>
        /// <param name="args">Массив аргументов корутины</param>
        /// <param name="sender">Отправитель метода</param>
        /// <returns>Идентификатор для контроля корутины</returns>
        int ExecuteAsync(Func<object[], IEnumerator> action, object[] args, object sender);

        /// <summary>
        /// Проверяет работает-ли еще корутина
        /// </summary>
        /// <param name="sender">Отправитель метода</param>
        /// <param name="index">Идентификатор запроса</param>
        /// <returns>true - если корутина активна</returns>
        bool IsWork(object sender, int index);

        /// <summary>
        /// Останавливает корутину для не Unity сущности
        /// </summary>
        /// <param name="sender">Отправитель метода</param>
        /// <param name="index">Идентификатор запроса</param>
        void Stop(object sender, int index);
    }
}
