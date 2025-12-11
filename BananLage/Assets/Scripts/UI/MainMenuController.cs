using Misc;
using UnityEngine;

namespace UI
{
    public class MainMenuController: MonoBehaviour
    {
        
        public void Game() => SceneNavigator.ToGenerator();
        
        public void Quit() => Application.Quit();
    }
}