using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour
    where T : Component
{
    private static T _instance;
    public static T instance {
        get {
            return _instance;
        }
    }

    public void Awake() {
        // singleton pattern
        if (_instance != null)
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }
            return;
        }

        DontDestroyOnLoad(gameObject);
        _instance = this as T;
    }

    public static void Remove() {
        if (instance != null) {
            Destroy(instance.gameObject);
        }
        T[] items = FindObjectsOfType<T>();
        foreach (T item in items)
        {
            if (item != null && item.gameObject != null) {
                Destroy(item.gameObject);
            }
        }
    }
}
