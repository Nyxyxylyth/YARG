﻿using UnityEngine;
using YARG.Core.Input;
using YARG.Menu.Navigation;

namespace YARG.Gameplay.HUD
{
    public class GenericPause : MonoBehaviour
    {
        private PauseMenuManager _pauseMenuManager;

        private void Awake()
        {
            _pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        }

        private void OnEnable()
        {
            Navigator.Instance.PushScheme(new NavigationScheme(new()
            {
                NavigationScheme.Entry.NavigateSelect,
                new NavigationScheme.Entry(MenuAction.Red, "Back", Resume),
                NavigationScheme.Entry.NavigateUp,
                NavigationScheme.Entry.NavigateDown,
            }, false));
        }

        private void OnDisable()
        {
            Navigator.Instance.PopScheme();
        }

        public void Resume()
        {
            _pauseMenuManager.PopMenu();
        }

        public void Restart()
        {
            _pauseMenuManager.Restart();
        }

        public void RestartInPractice()
        {
            GlobalVariables.Instance.IsPractice = true;
            _pauseMenuManager.Restart();
        }

        public void RestartInQuickPlay()
        {
            GlobalVariables.Instance.IsPractice = false;
            _pauseMenuManager.Restart();
        }

        public void SelectSections()
        {
            _pauseMenuManager.OpenMenu(PauseMenuManager.Menu.SelectSections);
        }

        public void BackToLibrary()
        {
            _pauseMenuManager.Quit();
        }
    }
}