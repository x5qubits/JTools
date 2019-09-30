using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using JCommonDB.model;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;


namespace JCommonDB
{
    public class HibernateAdapter<PK>
    {
        ISessionFactory factory;
        ISession session;
        readonly Configuration Config;
        string MapSavePath = "Configs/HibernateMaps.xml";
        string CfgPath = "Configs/HibernateConfig.xml";
        string Class2MapNameSpace = "Tests";
        bool IsDebug = true;

        public HibernateAdapter()
        {
            try
            {               
                Config = new Configuration().Configure(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CfgPath));        
                string mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MapSavePath);
                if (!IsDebug && File.Exists(mapPath))
                {
                    Config.AddXmlString(File.ReadAllText(mapPath));
                    factory = Config.BuildSessionFactory();
                    return;
                }
                
                Type[] typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), Class2MapNameSpace);
                bool Added = false;
                for (int i = 0; i < typelist.Length; i++)
                {
                    Added = false;
                    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
                    xml += "<hibernate-mapping xmlns=\"urn:nhibernate-mapping-2.2\" assembly=\"" + Class2MapNameSpace.Split('.')[0] + "\" namespace=\"" + Class2MapNameSpace + "\">";
                    xml += "<class name=\"" + typelist[i].Name + "\">";
                    foreach (PropertyInfo prop in typelist[i].GetProperties())
                    {
                        if (prop.GetCustomAttribute(typeof(TableIndex)) != null && !Added)
                        {
                            TableIndex att = (TableIndex)prop.GetCustomAttribute(typeof(TableIndex));
                            if (!att.IsEnum)
                            {
                                xml += "<id name=\"" + prop.Name + "\" column=\"" + prop.Name + "\" type=\"" + prop.PropertyType.Name + "\">";
                                if (att.IsAutoGenerate)
                                    xml += "<generator class=\"native\"></generator>";
                                xml += "</id>";
                            }
                            else
                            {
                                xml += "<property name=\"" + prop.Name + "\" type =\"" + att.ClassName + "\" />";
                            }
                            Added = true;
                        }
                        else
                        {
                            xml += "<property name=\"" + prop.Name + "\" column=\"" + prop.Name + "\" type =\"" + prop.PropertyType.Name + "\"></property>";
                        }
                    }
                    xml += "</class>";
                    xml += "</hibernate-mapping>";
                    XDocument doc = XDocument.Parse(xml);
                    File.WriteAllText(mapPath, doc.ToString());      
                    Config.AddXmlString(doc.ToString());
                }
                factory = Config.BuildSessionFactory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected void ValidateSession()
        {
            if (factory == null)
            {
                factory = Config.BuildSessionFactory();
            }
            if (session == null)
            {
                session = factory.OpenSession();
            }
            if (!session.IsConnected)
            {
                session.Reconnect();
            }
            if (!session.IsOpen)
            {
                session = factory.OpenSession();
            }
        }

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        public ISession GetSession()
        {
            ValidateSession();
            return session;
        }

        public bool Delete(object paramClass)
        {
            bool deleted = false;
            using (ITransaction transaction = GetSession().BeginTransaction())
            {
                if (paramClass != null)
                {
                    GetSession().Delete(GetSession().Merge(paramClass));
                    transaction.Commit();
                    deleted = true;
                }
            }
            return deleted;
        }

        public object Execute(DetachedCriteria detachedCriteria)
        {
            if (detachedCriteria != null)
            {
                ICriteria criteria = detachedCriteria.GetExecutableCriteria(GetSession());
                return criteria.List();
            }
            return null;
        }

        public T Get<T>(PK ID)
        {
            ValidateSession();
            return (T)session.Get(typeof(T), ID);
        }

        public T Save<T> (T entity)
        {
            T v = default;
            using (ITransaction transaction = GetSession().BeginTransaction())
            {
                v = (T)GetSession().Merge(entity);               
                transaction.Commit();
            }
            return v;
        }

        public void Update<T>(params T[] entities)
        {
            using (ITransaction transaction = GetSession().BeginTransaction())
            {
                foreach (T entity in entities)
                {
                    try
                    {
                        GetSession().Merge(entity);
                    }
                    catch (Exception)
                    {
                        GetSession().Save(entity);
                    }
                }
                transaction.Commit();
            }
        }

        public void Update<T>(List<T> entities)
        {
            using (ITransaction transaction = GetSession().BeginTransaction())
            {
                foreach (T entity in entities)
                {
                    try
                    {
                        GetSession().Merge(entity);
                    }
                    catch (Exception)
                    {
                        GetSession().Save(entity);
                    }
                }
                transaction.Commit();
            }
        }

    }
}
