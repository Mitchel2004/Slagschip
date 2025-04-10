using UnityEngine;

namespace Missile.Data
{
    [System.Serializable]
    public struct MissileData
    {
        public AnimationCurve heightCurve;
        public float heightMultiplier;
        public float speed;
        [SerializeField] private Vector3 startPos;

        public Vector3 StartPos
        { 
            get => startPos; 
            private set => startPos = value; 
        }
        public Vector3 EndPos
        {
            get; 
            set;
        }

        public MissileData(MissileData _original, Vector3 _endPos)
        {
            heightCurve = _original.heightCurve;
            heightMultiplier = _original.heightMultiplier;
            speed = _original.speed;
            startPos = _original.startPos;

            EndPos = _endPos;
        }
    }
}
