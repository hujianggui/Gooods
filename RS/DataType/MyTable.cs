using System;
using System.Collections;

namespace RS.DataType
{
    /// <summary>
    /// class MyTable
    /// Two-Level Hashtable: hastables in a hashtable
    /// </summary>
    public class MyTable
    {
        /// <summary>
        /// class SubKeyComparer
        /// </summary>
        private class SubKeyComparer : IComparer
        {
            private Hashtable SubKeyTable = null;

            public SubKeyComparer(Hashtable subKeyTable)
            {
                this.SubKeyTable = subKeyTable;
            }

            // Asc
            public int Compare(object x, object y)
            {
                int f = (int)SubKeyTable[x];
                int s = (int)SubKeyTable[y];

                if (f < s)
                    return -1;
                else if (f == s)
                    return 0;
                else return 1;
            }
        }

        private Hashtable Main = new Hashtable();

        private Hashtable SubKeyAll = new Hashtable();  // Set of SubKeys

        public MyTable() {}

        public virtual bool ContainsKey(object mainKey, object subKey)
        {
            if (null == mainKey || null == subKey)
                throw new System.ArgumentNullException();

            if (Main.ContainsKey(mainKey))
            {
                Hashtable subTable = (Hashtable)Main[mainKey];
                if (subTable.ContainsKey(subKey))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void Add(object mainKey, object subKey, object value)
        {
            // if null
            if ( null == mainKey || null == subKey)
                throw new ArgumentNullException();

            // If already exists
            if (ContainsKey(mainKey, subKey))
                throw new ArgumentException();

            // If readonly or fix-size
            if (Main.IsFixedSize || Main.IsReadOnly)
                throw new NotSupportedException();

            if (null == Main[mainKey])
                Main[mainKey] = new Hashtable();

            Hashtable subTable = (Hashtable)Main[mainKey];
            subTable.Add(subKey, value); 

            if (!SubKeyAll.Contains(subKey))
            {
                SubKeyAll.Add(subKey, SubKeyAll.Count+1); // note
            }
        }

        public virtual object this[object mainKey, object subKey]
        {
            get { return ((Hashtable)Main[mainKey])[subKey];}
            set { ((Hashtable)Main[mainKey])[subKey] = value; }
        }

        public virtual object this[object mainKey] 
        {
            get { return Main[mainKey]; }
            set { Main[mainKey] = value; }
        }

        public virtual ICollection Keys { get { return Main.Keys; } }

        public virtual int Count { get { return Main.Count; } }

        public virtual Hashtable SubKeyTable { get { return SubKeyAll; } }

        public virtual int SubKeyIndex(object subKey)
        {
            if (SubKeyAll.ContainsKey(subKey))
                return (int)SubKeyAll[subKey];
            return -1;
        }

        public virtual ArrayList GetSubKeyList()
        {
            ArrayList list = new ArrayList(SubKeyAll.Keys);
            list.Sort(new SubKeyComparer(SubKeyAll));
            return list;
        }

        public virtual bool ContainsMainKey(object mainKey)
        {
            if (Main.ContainsKey(mainKey))
            {
                return true;
            }
            return false;
        }
    }


    public class MyTableTest
    {
        public static void Test()
        {
            MyTable t = new MyTable();
            
            const int size = 7000;
            Random r = new Random();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (!t.ContainsKey(i, j))
                    {
                        t.Add(i, j, r.Next());
                    }
                }
                Console.WriteLine("{0} Numbers", i*size);
            }
        }
    }

}
