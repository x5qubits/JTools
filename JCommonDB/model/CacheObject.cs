using System;

namespace JCommonDB.model
{
    public class CacheObject<PK>
    {
        private PK uID;
        private const long ONE_DAY_MILISECONDS = 86400000L;
        private const long FIVE_SECOND_MILISECONDS = 5000L;
        private object entity;
        private long ttl = FIVE_SECOND_MILISECONDS;
        private long createTime = CurrentTimeMillis();
        private long expireTime = 0L;

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public static CacheObject<PK> ValueOf(PK ID, object entity)
        {;
            CacheObject<PK> entityCacheObject = new CacheObject<PK>
            {
                entity = entity,
                uID = ID,
                createTime = CurrentTimeMillis()
            };
            entityCacheObject.expireTime = (entityCacheObject.createTime + entityCacheObject.ttl);
            return entityCacheObject;
        }

        public static CacheObject<PK> ValueOf(PK ID, object entity, long timeToLive)
        {
            CacheObject<PK> entityCacheObject = new CacheObject<PK>
            {
                entity = entity,
                uID = ID,
                ttl = timeToLive,
                createTime = CurrentTimeMillis()
            };
            entityCacheObject.expireTime = (entityCacheObject.createTime + entityCacheObject.ttl);
            return entityCacheObject;
        }

        public string GetKey()
        {
            return GetEntity().GetType().Name + "_" + uID;
        }
        public PK GetId()
        {
            return uID;
        }

        public bool IsValidate()
        {
            return this.expireTime >= CurrentTimeMillis();
        }

        public object GetEntity()
        {
            return this.entity;
        }

        public long GetTtl()
        {
            return this.ttl;
        }

        public long GetCreateTime()
        {
            return this.createTime;
        }

        public long GetExpireTime()
        {
            return this.expireTime;
        }
    }

}
