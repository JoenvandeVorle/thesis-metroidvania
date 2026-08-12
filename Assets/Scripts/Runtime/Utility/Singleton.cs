using System;
using UnityEngine;

namespace Metroidvania
{
    // This some real gourmet shit
    public abstract class Singleton<T> where T : Singleton<T>
    {
        private static readonly Lazy<T> lazyInstance =
            new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T);
        public static T instance => lazyInstance.Value;
        protected Singleton() { }
    }

    public abstract class StaticMonoInstance<T> : MonoBehaviour where T : StaticMonoInstance<T>
    {
        public static T instance { get; private set; }

        protected virtual void Awake() => instance = this as T;

        protected virtual void OnDestroy()
        {
            if (instance == (this as T))
                instance = null;
        }
    }

    public abstract class MonoSingleton<T> : StaticMonoInstance<T> where T : MonoSingleton<T>
    {
        protected override void Awake()
        {
            if (instance)
                Destroy(gameObject);
            base.Awake();
        }
    }

    public abstract class SingletonPersistent<T> : MonoSingleton<T> where T : SingletonPersistent<T>
    {
        protected override void Awake()
        {
            base.Awake();
            // if (!Vars.DOING_TESTS) // Persistent singletons tend to cause issues in tests
            DontDestroyOnLoad(gameObject);
            name = $"[{typeof(T).Name}]";
        }
    }
}
