using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Util
{
    public class SceneLoaderWithRetry : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] string _targetSceneName = "MainGameScene";

        void Start()
        {
            StartCoroutine(LoadSceneAsync());
        }

        IEnumerator LoadSceneAsync()
        {
            var asyncOperation = SceneManager.LoadSceneAsync(_targetSceneName);
            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
                if (asyncOperation.progress >= 0.9f)
                    asyncOperation.allowSceneActivation = true;

                yield return null;
            }
        }
    }
}