using System.Collections;
using Unity.Netcode;
using UnityEditor;
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
        public SceneAsset fallbackScene;
        public float loadDelay;

        private void Awake()
        {
            if(instance == null)
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
            if (NetworkManager.LocalClientId == 0 == IsOwner)
            {
                yield return new WaitForSeconds(loadDelay);

                LoadNetworkedSceneRpc(_sceneName);
            }
            else
            {
                LoadScene(fallbackScene.name, false, 0);
            }
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

            while(!_asyncLoad.isDone)
                yield return null;
        }

        private IEnumerator LoadLocalScene(string _sceneName, float _loadDelay)
        {
            yield return new WaitForSeconds(_loadDelay);

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

        public void LoadScene(string _sceneName, bool _isNetworked, float _loadDelay)
        {
            if(_isNetworked)
            {
                StartCoroutine(LoadNetworkedScene(_sceneName));
            }
            else
            {
                StartCoroutine(LoadLocalScene(_sceneName, _loadDelay));
            }
        }
    }
}
