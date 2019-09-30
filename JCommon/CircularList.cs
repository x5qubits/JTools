using System.Collections.Generic;

namespace JCommon
{
    public class CircularList<T> : List<T>
    {
        protected int Index = -1;
        public CircularList()
        {
            Index = -1;
        }

        public CircularList(List<T> obj)
        {
            Index = -1;
            AddRange(obj);
           
        }

        public CircularList(T[] obj)
        {
            Index = -1;
            AddRange(obj);
        }

        public T GetNext()
        {
            if (Count == 0)
                return default;

            Index++;
            if (Index < Count)
            {
                T v = base[Index];
                return v;
            }
            else
            {
                Index = 0;
                T v = base[Index];
                return v;
            }
        }

        public T GetLast()
        {
            if (Count == 0)
                return default;

            if (Index < Count)
            {
                T v = base[Index];
                return v;
            }
            else
            {
                T v = base[0];
                return v;
            }
            
        }

        public T First()
        {
            if (Count == 0)
                return default;

            return base[0];
        }

        public T Last()
        {
            if (Count == 0)
                return default;

            return base[Count - 1];
        }

        public T GetPreviews()
        {
            if (Count == 0)
                return default;

            int x = Index - 1;
            if(x < 0)
            {
                x = Count - 1;
            }
            if (x < Count)
            {
                T v = base[x];
                return v;
            }
            else
            {
                T v = base[0];
                return v;
            }
        }
    }
}
