using UnityEngine;

namespace Missile.Data
{
    [System.Serializable]
    public struct MissileData
    {
        public AnimationCurve heightCurve;
        public float heightMultiplier;
        public float speed;
        public bool hit;
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

        public MissileData(MissileData _original, Vector3 _endPos, bool _hit)
        {
            heightCurve = _original.heightCurve;
            heightMultiplier = _original.heightMultiplier;
            speed = _original.speed;
            hit = _hit;
            startPos = _original.startPos;

            EndPos = _endPos;
        }
    }
}
