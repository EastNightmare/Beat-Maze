using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Common
{
    public class Singleton<T> where T : new()
    {
        private static T m_Instance;

        /// <summary>
        /// Get singleton instance
        /// </summary>
        ///
        public static T instance
        {
            get
            {
                if (m_Instance == null) m_Instance = new T();
                return m_Instance;
            }
        }
    }

    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_Instance;

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static T instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindObjectOfType<T>();
                    if (m_Instance == null)
                    {
                        Debug.LogWarningFormat("There is no a {0} in the scene", typeof(T).ToString());
                        m_Instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    }
                }
                return m_Instance;
            }
        }

        public void DestroyInstance()
        {
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#else
            Destroy(gameObject);
#endif
            m_Instance = null;
        }

        protected void Awake()
        {
            if (this != instance)
            {
                Destroy(this.gameObject);
            }
        }
    }

    public class SingtonEditor<T> : EditorWindow where T : EditorWindow
    {
        private static T m_Instance;

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static T instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = EditorWindow.GetWindow<T>();
                    if (m_Instance == null)
                    {
                        Debug.LogWarningFormat("There is no a {0} in the editor", typeof(T).ToString());
                    }
                }
                return m_Instance;
            }
        }
    }
}