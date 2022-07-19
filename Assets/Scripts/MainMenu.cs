using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.MultiPlayerProject
{
    public class MainMenu : MonoBehaviour
    {
        public Launcher launcher;
        public void JoinMatch()
        {
            launcher.Join();
        }

        public void CreateMatch()
        {
            launcher.Create();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}