using JCommon.FileDatabase.IO;
using System.IO;

namespace JCommon.FileDatabase
{
    /// <summary>
    /// A class that allows you to read file based on your own file strcture
    /// </summary>
    public class FileDatabase
    {
        private static object s_Sync = new object();
        static volatile FileDatabase s_Instance;
        public static FileDatabase Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Sync)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new FileDatabase();
                        }
                    }
                }
                return s_Instance;
            }
        }

        public static TMsg ReadFile<TMsg>(string path) where TMsg : DataFile, new()
        {
           return Instance.MReadFile<TMsg>(path);
        }

        public static void WriteFile(string path, DataFile msg)
        {
            Instance.MWriteFile(path, msg);
        }

        public static void WriteFile(DataFile msg)
        {
            WriteFile(msg.Path, msg);
        }

        public TMsg MReadFile<TMsg>(string path) where TMsg : DataFile, new()
        {
            if (File.Exists(path))
            {
                var msg = new TMsg
                {
                    Path = path
                };
                var reader = new DataReader(File.ReadAllBytes(path));
                msg.Deserialize(reader);                
                return msg;
            }
            return null;
        }

        public void MWriteFile(string path, DataFile msg)
        {
            msg.Path = path;
            DataWriter writer = new DataWriter();
            msg.Serialize(writer);
            File.WriteAllBytes(msg.Path, writer.ToArray());
        }

    }
}
