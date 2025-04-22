using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FX
{
    using Enumeration;

    public class FXSystem : MonoBehaviour
    {
        [SerializeField] private List<Effect> effects;
        private int _effectCount = 0;
        private int _effectItteration = 0;
        private Queue<UnityAction> _queue = new Queue<UnityAction>();

        private void Awake()
        {
            FillQueue();
            InitializeEffects();
        }

        private void Start()
        {
            Play();
        }

        public void Play()
        {
            if (_queue.Count > 0)
            {
                UnityAction action = _queue.Dequeue();
                action.Invoke(); 
            }
        }

        private void FillQueue()
        {
            List<Effect> currentBatch = new List<Effect>();
            foreach (Effect effect in effects)
            {
                if (effect.timing == ETiming.StartAfterPrevious && currentBatch.Count > 0)
                {
                    List<Effect> batchCopy = new List<Effect>(currentBatch);
                    _queue.Enqueue(() => {
                        _effectCount = batchCopy.Count;
                        foreach (Effect effect in batchCopy)
                        {
                            effect.Play();
                        }
                    });

                    currentBatch.Clear();
                }

                currentBatch.Add(effect);
            }
            if (currentBatch.Count > 0)
            {
                List<Effect> batchCopy = new List<Effect>(currentBatch);
                _queue.Enqueue(() => {
                    _effectCount = batchCopy.Count;
                    foreach (Effect effect in batchCopy)
                    {
                        effect.Play();
                    }
                });
            }
        }
        private void InitializeEffects()
        {
            foreach (Effect effect in effects)
            {
                effect.Initialize();
                effect.onEffectEnd.AddListener(CheckEnd);
            }
        }

        private void CheckEnd()
        {
            _effectItteration++;
            if (_effectItteration == _effectCount)
            {
                _effectItteration = 0;
                Play();
            }
        }
    }
}
