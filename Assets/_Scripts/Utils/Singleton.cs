using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
            }
            if (_instance == null)
            {
                Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            // DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Debug.Log("Pha huy " + gameObject.name);    
            Destroy(gameObject);
        }
    }
}