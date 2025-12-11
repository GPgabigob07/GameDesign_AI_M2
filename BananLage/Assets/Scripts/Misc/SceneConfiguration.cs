using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Misc
{
    [CreateAssetMenu(fileName = "Scene Config")]
    public class SceneConfiguration : ScriptableObject
    {
        [SerializeField] private string mainMenu, bootstrap, mainGame, mapGenerator, uiScene;

        public void ToMainMenu()
        {
            SceneManager.LoadScene(mainMenu);
        }

        public IEnumerator ToMainGameCoroutine()
        {
            var boot = SceneManager.LoadSceneAsync(bootstrap, LoadSceneMode.Single);
            yield return boot; // must activate first!
            
            var game = SceneManager.LoadSceneAsync(mainGame, LoadSceneMode.Additive);
            var ui   = SceneManager.LoadSceneAsync(uiScene, LoadSceneMode.Additive);

            var ops = new[] { game, ui };

            foreach (var op in ops)
                op.allowSceneActivation = false;
        
            while (ops.Any(op => op.progress < 0.9f))
                yield return null;
            
            foreach (var op in ops)
                op.allowSceneActivation = true;

            while (ops.Any(op => !op.isDone))
                yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainGame));
        }

        public void ToGenerator()
        {
            SceneManager.LoadScene(mapGenerator, LoadSceneMode.Single);
        }
    }
}