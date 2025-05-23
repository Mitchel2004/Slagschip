using UnityEditor;
using UnityEngine;
using Ships;

#if UNITY_EDITOR
namespace Editors
{
    [CustomEditor(typeof(PlacingIndicator))]
    public class PlacingIndicatorEditor : Editor
    {
        PlacingIndicator Target => (PlacingIndicator)target;

        private void OnEnable()
        {
            Target.Initialize();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generate Mesh"))
            {
                Target.GenerateMesh();
            }
        }
    }
}
#endif
