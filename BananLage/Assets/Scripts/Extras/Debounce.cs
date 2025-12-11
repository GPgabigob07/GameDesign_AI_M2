namespace Extras
{
    using System;
    using System.Collections;
    using UnityEngine;

    public class Debounce
    {
        private readonly MonoBehaviour _runner;
        private Coroutine _routine;

        public Debounce(MonoBehaviour runner)
        {
            _runner = runner;
        }

        /// <summary>
        /// Runs an action after a delay. If called again before the delay ends, the timer resets.
        /// </summary>
        public void Run(float delay, Action action)
        {
            Cancel();
            _routine = _runner.StartCoroutine(RunRoutine(delay, action));
        }

        /// <summary>
        /// Immediately cancels any pending action.
        /// </summary>
        public void Cancel()
        {
            if (_routine != null)
            {
                _runner.StopCoroutine(_routine);
                _routine = null;
            }
        }

        private IEnumerator RunRoutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
            _routine = null;
        }
    }
}