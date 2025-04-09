using System;
using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    public class SceneLoader : NetworkBehaviour
    {
        public static SceneLoader instance;

        public bool loadOnStart;
        public SceneAsset sceneToLoad;
        public bool isNetworked;
        public float loadDelay;

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

        private void Start()
        {
            if (loadOnStart)
                LoadScene(sceneToLoad.name, isNetworked);
        }

        private IEnumerator LoadNetworkedScene(string _sceneName)
        {
            yield return new WaitForSeconds(loadDelay);
            
            LoadNetworkedSceneRpc(_sceneName);
        }

        [Rpc(SendTo.Server)]
        private void LoadNetworkedSceneRpc(string _sceneName)
        {
            NetworkManager.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
        }

        private IEnumerator LoadLocalScene(string _sceneName)
        {
            yield return new WaitForSeconds(loadDelay);

            AsyncOperation _asyncLoad = SceneManager.LoadSceneAsync(_sceneName);

            while (!_asyncLoad.isDone)
                yield return null;
        }

        public void LoadScene(string _sceneName, bool _isNetworked)
        {
            if (_isNetworked)
            {
                StartCoroutine(LoadNetworkedScene(_sceneName));
            }
            else
            {
                StartCoroutine(LoadLocalScene(_sceneName));
            }
        }
    }
}
