using UnityEngine;

namespace Kit.Common
{
    public class SingletonDontDestroy<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject g = new GameObject(typeof(T).Name + "_Singleton");
                    _instance = g.AddComponent<T>();
                    DontDestroyOnLoad(g);
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = GetComponent<T>();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                }
                return _instance;
            }
        }
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = GetComponent<T>();
            }
            else
            {
                Destroy(this);
            }
        }

        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }
}