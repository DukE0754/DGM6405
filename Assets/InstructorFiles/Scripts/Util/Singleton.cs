using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Base class for singleton pattern
/// Note that not all singletons are automatically marked as DontDestroyOnLoad
/// Each script must be marked as such
/// </summary>
/// <typeparam name="TSingletonClass"></typeparam>
public abstract class Singleton<TSingletonClass> : MonoBehaviour where TSingletonClass : MonoBehaviour
{
    [SerializeField] private bool _willNotDestroyOnLoad;
    public static TSingletonClass Instance { get; private set; }

    public virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as TSingletonClass;
        
        if (_willNotDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        Destroy(gameObject);
        Instance = null;
    }
}