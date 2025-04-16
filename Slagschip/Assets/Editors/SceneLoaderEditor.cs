using SceneManagement;
using UnityEditor;

namespace Editors
{
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            SceneLoader _sceneLoader = (SceneLoader)target;

            _sceneLoader.loadOnStart = EditorGUILayout.Toggle("Load On Start", _sceneLoader.loadOnStart);

            if (_sceneLoader.loadOnStart)
            {
                _sceneLoader.sceneToLoad = (SceneAsset)EditorGUILayout.ObjectField("Scene To Load", _sceneLoader.sceneToLoad, typeof(SceneAsset), true);
                
                _sceneLoader.isNetworked = EditorGUILayout.Toggle("Is Networked", _sceneLoader.isNetworked);

                if (_sceneLoader.isNetworked)
                    _sceneLoader.fallbackScene = (SceneAsset)EditorGUILayout.ObjectField("Fallback Scene", _sceneLoader.fallbackScene, typeof(SceneAsset), true);

                _sceneLoader.loadDelay = EditorGUILayout.FloatField("Load Delay", _sceneLoader.loadDelay);
            }

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_sceneLoader);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
