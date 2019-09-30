using System.Collections.Generic;
using JCommon;
using JCommonDB.model;
using NHibernate;

namespace JCommonDB
{
    public sealed class DbService : HibernateAdapter<uint>
    {
        #region SINGLETON
        private static object s_Sync = new object();
        static volatile DbService s_Instance;
        public static DbService Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Sync)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new DbService();
                        }
                    }
                }
                return s_Instance;
            }
        }
        #endregion

        #region VARS
        private static readonly Dictionary<string, CacheObject<uint>> ENTITYS = new Dictionary<string, CacheObject<uint>>();
        private static readonly List<string> UPDATE_QUEUE = new List<string>();

        public int UpdateEvery = 10;

        static HibernateAdapter<uint> _HAdapter = null;
        public HibernateAdapter<uint> HAdapter
        {
            get
            {
                if (_HAdapter == null)
                {
                    _HAdapter = new HibernateAdapter<uint>();
                }
                return _HAdapter;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public DbService()
        {
            Invoker.InvokeRepeating(OnTimedEvent, UpdateEvery * 1000);
        }
        #endregion

        #region LOGIC
        void OnTimedEvent()
        {
            RunUpdate();
        }

        string GetKey<T>(uint key)
        {
            return typeof(T).Name + "_" + key;
        }

        bool GetFromCache<T>(uint key, out T cacheObject)
        {
            string StrKey = GetKey<T>(key);
            bool rt = false;
            T ex = default;
            lock (ENTITYS)
            {
                if (ENTITYS.TryGetValue(StrKey, out CacheObject<uint> e))
                {
                    ex = (T)e.GetEntity();
                    rt = true;
                }
            }
            cacheObject = ex;
            return rt;
        }

        public void RemoveFromDB<T>(uint Id)
        {
            string Key = GetKey<T>(Id);
            lock (ENTITYS)
            {
                if (ENTITYS.TryGetValue(Key, out CacheObject<uint> c))
                {
                    Delete(c.GetEntity());
                    ENTITYS.Remove(Key);
                }
                else
                {
                    if (GetFromCacheOrFromDB(Id, out T x))
                    {
                        ENTITYS.Remove(Key);
                        Delete(x);
                    }
                }
            }
        }

        public bool GetFromCacheOrFromDB<T>(uint Key, out T obj)
        {
            if (GetFromCache(Key, out T x))
            {
                obj = x;
                return true;
            }
            else
            {
                obj = HAdapter.Get<T>(Key);
                if (obj != default)
                {
                    Put2ObjectMap(CacheObject<uint>.ValueOf(Key, obj));
                }
                return true;
            }
        }

        void Put2ObjectMap(CacheObject<uint> x)
        {
            lock (ENTITYS)
                ENTITYS[x.GetKey()] = x;
        }

        public void ISubmitUpdate2Queue(uint Key, object Entity)
        {
            if (Entity is CacheObject<uint> x)
            {
                Put2ObjectMap(x); // UPDATE OBJECT IN LIST
                lock (UPDATE_QUEUE)
                    if (!UPDATE_QUEUE.Contains(x.GetKey()))
                        UPDATE_QUEUE.Add(x.GetKey());
            }
            else
            {
                x = CacheObject<uint>.ValueOf(Key, Entity);
                Put2ObjectMap(x);
                lock (UPDATE_QUEUE)
                    if (!UPDATE_QUEUE.Contains(x.GetKey()))
                        UPDATE_QUEUE.Add(x.GetKey());
            }
        }

        public void IUpdateEntityIntime(uint Key, object Entity)
        {
            if (Entity is CacheObject<uint> x)
            {
                Put2ObjectMap(x); // UPDATE OBJECT IN LIST
                Update(x.GetEntity());
                lock (UPDATE_QUEUE)
                    UPDATE_QUEUE.Remove(x.GetKey());
            }
            else
            {
                x = CacheObject<uint>.ValueOf(Key, Entity);
                Put2ObjectMap(x);
                Update(x.GetEntity());
                lock (UPDATE_QUEUE)
                    UPDATE_QUEUE.Remove(x.GetKey());
            }
        }

        public T ICreateEntity<T>(T Entity)
        {
            return Save(Entity);
        }

        private bool TryDequeue(out string data)
        {
            string r = null;
            lock (UPDATE_QUEUE)
            {
                if (UPDATE_QUEUE.Count > 0)
                {
                    r = UPDATE_QUEUE[0];
                    UPDATE_QUEUE.RemoveAt(0);
                }
            }
            data = r;
            return r != null;
        }

        void RunUpdate()
        {
            if (TryDequeue(out string ekey))
            {
                lock (ENTITYS)
                {
                    if (ENTITYS.TryGetValue(ekey, out CacheObject<uint> entity))
                    {
                        if (entity.IsValidate())
                        {
                            Update(entity.GetEntity());
                        }
                        else
                        {
                            Update(entity.GetEntity());

                            ENTITYS.Remove(ekey);
                        }
                    }
                }
            }
        }

        ISession IGetSession()
        {
            return HAdapter.GetSession();
        }

        bool IForceQuit()
        {
            while (TryDequeue(out string ekey))
            {
                lock (ENTITYS)
                {
                    if (ENTITYS.TryGetValue(ekey, out CacheObject<uint> entity))
                    {
                        if (entity.IsValidate())
                        {
                            Update(entity.GetEntity());
                        }
                        else
                        {
                            Update(entity.GetEntity());
                            ENTITYS.Remove(ekey);
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region LAZY 
        public static void SubmitUpdate2Queue(uint Key, object Entity)
        {
            Instance.ISubmitUpdate2Queue(Key, Entity);
        }

        public static T CreateEntity<T>(T Entity)
        {
            return Instance.ICreateEntity(Entity);
        }

        public static void UpdateEntityIntime(uint Key, object Entity)
        {
            Instance.IUpdateEntityIntime(Key, Entity);
        }

        public static T GetFromCache<T>(uint Key)
        {
            if (Instance.GetFromCacheOrFromDB(Key, out T obj))
                return obj;

            return default;
        }

        public static void RemoveEntityFromDatabase<T>(uint Key)
        {
            Instance.RemoveFromDB<T>(Key);
        }

        public static ISession GetDBSession => Instance.IGetSession();

        public static bool ForceQuit()
        {
            return Instance.IForceQuit();
        }
        #endregion
    }
}
