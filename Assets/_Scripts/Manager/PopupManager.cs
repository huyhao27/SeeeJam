
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
            popup.gameObject.SetActive(false);
        }
    }
    
    public T ShowPopup<T>() where T : Popup
    {
        var popupToShow = popups.FirstOrDefault(p => p is T);
        if (popupToShow != null)
        {
            popupToShow.Show();
            return popupToShow as T;
        }
        else
        {
            Debug.LogError("Popup type " + typeof(T) + " not found in the PopupManager list.");
            return null;
        }
    }
    
    public void HidePopup<T>() where T : Popup
    {
        var popupToHide = popups.FirstOrDefault(p => p is T);
        if (popupToHide != null)
        {
            popupToHide.Hide();
        }
    }
    
    public void HideAllPopups()
    {
        foreach (var popup in popups)
        {
            if (popup.gameObject.activeSelf)
            {
                popup.Hide();
            }
        }
    }
}