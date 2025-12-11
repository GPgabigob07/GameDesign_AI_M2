using UnityEngine;
using UnityEngine.SceneManagement;

namespace Misc
{
    public class SceneNavigator: MonoBehaviour
    {
        [SerializeField]
        private SceneConfiguration _sceneConfig;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        private static SceneNavigator Instance { get; set; }
        
        public static void ToMainGame() => Instance.StartCoroutine(Instance._sceneConfig.ToMainGameCoroutine());
        
        public static void ToGenerator() => Instance._sceneConfig.ToGenerator();
        
        public void Quit() => Application.Quit();

        public static void ToMenu() => Instance._sceneConfig.ToMainMenu();
    }
}