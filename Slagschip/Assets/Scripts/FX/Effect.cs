using FX.Enumeration;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Timer;

namespace FX
{
    [System.Serializable]
    public class Effect
    {
        public ETiming timing;
        public EEffectType effectType;

        public ParticleSystem particleSystem;
        public AudioClip audioClip;
        public AudioSource audioSource;

        [SerializeField] private float delay;

        public UnityEvent onEffectStart;
        public UnityEvent onEffectEnd;

        private CountdownTimer _delayTimer;
        private CountdownTimer _timer;

        public float Length
        {
            get
            {
                switch (effectType)
                {
                    case EEffectType.Particle:
                        return particleSystem.main.duration;
                    case EEffectType.Audio:
                        return audioClip.length;
                }
                return 0;
            }
        }

        public void Initialize()
        {
            _delayTimer = new CountdownTimer(delay);
            _timer = new CountdownTimer(delay + Length);

            _delayTimer.onTimerStop += PlayEffect;
            _delayTimer.onTimerStop += onEffectStart.Invoke;

            _timer.onTimerStop += onEffectEnd.Invoke;
        }

        public void Play()
        {
            _delayTimer.Start();
            _timer.Start();
        }

        public void Stop()
        {
            switch (effectType)
            {
                case EEffectType.Particle:
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    break;
                case EEffectType.Audio:
                    audioSource.Stop();
                    break;
            }
        }

        private void PlayEffect()
        {
            switch (effectType)
            {
                case EEffectType.Particle:
                    particleSystem.Play(); 
                    break;
                case EEffectType.Audio:
                    audioSource.clip = audioClip;
                    audioSource.Play();
                    break;
            }
        }
    }
}

