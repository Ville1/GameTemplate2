using System;
using UnityEngine;

namespace Game
{
    public class Timer
    {
        public delegate void TimerCallback();

        public float Interval { get; private set; }
        public float CurrentTime { get; private set; }
        public TimerCallback Action { get; private set; }
        public TimerCallback OnStartAction { get; private set; }
        public TimerCallback OnEndAction { get; private set; }
        public bool IsPaused { get; set; }
        public bool IsActive { get; private set; }
        public long Cycle { get; private set; }
        public long? MaxCycles { get; private set; }

        private bool onStartCalled;

        public Timer(float interval, TimerCallback action, bool isPaused = false, long? maxCycles = null)
        {
            Initialize(interval, action, null, null, isPaused, maxCycles);
        }

        public Timer(float interval, long maxCycles, TimerCallback action, bool isPaused = false)
        {
            Initialize(interval, action, null, null, isPaused, maxCycles);
        }

        public Timer(float interval, TimerCallback action, TimerCallback onStart, TimerCallback onEnd, bool isPaused = false, long? maxCycles = null)
        {
            Initialize(interval, action, onStart, onEnd, isPaused, maxCycles);
        }

        private void Initialize(float interval, TimerCallback action, TimerCallback onStart, TimerCallback onEnd, bool isPaused, long? maxCycles)
        {
            if(interval <= 0.0f) {
                throw new ArgumentException(string.Format("Interval {0} is less or equal to zero", interval));
            }
            if (maxCycles.HasValue && maxCycles.Value <= 0) {
                throw new ArgumentException(string.Format("MaxCycles {0} is less or equal to zero", maxCycles.Value));
            }

            Interval = interval;
            CurrentTime = interval;
            Action = action;
            OnStartAction = onStart;
            OnEndAction = onEnd;
            IsPaused = isPaused;
            IsActive = true;
            Cycle = 0;
            MaxCycles = maxCycles;
            onStartCalled = false;
        }

        public void Update()
        {
            if (IsPaused || !IsActive) {
                return;
            }

            if (!onStartCalled) {
                onStartCalled = true;
                if(OnStartAction != null) {
                    OnStartAction();
                }
            }

            CurrentTime -= Time.deltaTime;
            if(CurrentTime <= 0.0f) {
                Action();
                CurrentTime += Interval;
                Cycle = Cycle == long.MaxValue ? 0 : Cycle + 1;
                if(MaxCycles.HasValue && Cycle >= MaxCycles.Value) {
                    Stop();
                }
            }
        }

        public void Stop()
        {
            IsActive = false;
            if(OnEndAction != null) {
                OnEndAction();
            }
        }

        public void Restart()
        {
            Initialize(Interval, Action, OnStartAction, OnEndAction, false, MaxCycles);
        }
    }
}
