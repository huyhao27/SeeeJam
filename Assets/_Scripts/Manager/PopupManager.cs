using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] private List<Popup> popups; 

    protected override void Awake()
    {
        base.Awake();
        foreach (var popup in popups)
        {
            popup.Hide();
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Paused)
        {
            ShowPopup<PausePopup>(); 
        }
        else if (state == GameState.GameOver)
        {
            ShowPopup<GameOverPopup>(); 
        }
        else
        {
            // If state is MainMenu or Playing, hide all popups
            HideAllPopups();
        }
    }
    
    public void ShowPopup<T>() where T : Popup
    {
        var popupToShow = popups.FirstOrDefault(p => p is T);
        if (popupToShow != null)
        {
            popupToShow.Show();
        }
    }

    public void HideAllPopups()
    {
        foreach (var popup in popups)
        {
            popup.Hide();
        }
    }
}