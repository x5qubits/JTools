using System;

namespace JCommonDB.model
{
    public class TableIndex : Attribute
    {
        public bool IsAutoGenerate = false;
        public string ClassName = "";
        public bool IsEnum = false;

        public TableIndex()
        {

        }

        public TableIndex(bool AutoGenerate)
        {
            IsAutoGenerate = AutoGenerate;
        }

        public TableIndex(bool _IsEnum, string EnumClassName)
        {
            IsEnum = _IsEnum;
            ClassName = EnumClassName;
        }
    }
}
