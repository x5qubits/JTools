using JCommon.FileDatabase.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class DataFileExample : DataFile
    {
        public string test = "";
        public override void Deserialize(DataReader reader)
        {
            test = reader.ReadString();
        }
        public override void Serialize(DataWriter writer)
        {
            writer.Write(test);
        }
    }
}
