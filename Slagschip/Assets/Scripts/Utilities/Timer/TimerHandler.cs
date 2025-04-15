using UnityEngine;
using UnityEngine.Events;

namespace Utilities.Timer
{
    public class TimerHandler : MonoBehaviour
    {
        public static TimerHandler instance;

        public UnityAction<float> onTick = (deltaTime) => { };

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        void Update()
        {
            onTick.Invoke(Time.deltaTime);
        }
    }
}
