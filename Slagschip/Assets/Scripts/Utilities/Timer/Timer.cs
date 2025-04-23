using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities.Timer
{
    public abstract class Timer
    {
        protected float initialTime;

        protected float Time { get; set; }
        public bool IsRunning { get; protected set; }

        public float Progress => Time / initialTime;

        public UnityAction onTimerStart = () => { };
        public UnityAction onTimerStop = () => { };

        public Timer(float value)
        {
            initialTime = value;
            IsRunning = false;

            if (TimerHandler.instance == null)
            {
                throw new System.NullReferenceException("TimerHandler is not found in scene!");
            }
            TimerHandler.instance.onTick += Tick;
        }

        public void Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                onTimerStart.Invoke();
            }
        }

        public void Stop() 
        {
            if (IsRunning)
            {
                IsRunning = false;
                onTimerStop.Invoke();
            }
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public abstract void Tick(float deltaTime);
    }
}
