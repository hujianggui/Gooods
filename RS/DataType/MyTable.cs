using System;
using System.Collections;
using System.Diagnostics;

namespace RS.DataType
{
    /// <summary>
    /// Two-Level Hashtable: hastables in another hashtable
    /// </summary>
    public class MyTable
    {
        private Hashtable Main = new Hashtable();

        private Hashtable SubKeys = new Hashtable();  // Set of SubKeys: id - index

        public MyTable() {}

        public virtual bool ContainsKey(object mainKey, object subKey)
        {
            if (null == mainKey || null == subKey)
                throw new ArgumentNullException();

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
            {
                throw new ArgumentNullException();
            }                

            // If already exists
            if (ContainsKey(mainKey, subKey))
            {
                throw new ArgumentException();
            }               

            // If readonly or fix-size
            if (Main.IsFixedSize || Main.IsReadOnly)
            {
                throw new NotSupportedException();
            }                

            if (null == Main[mainKey])
            {
                Main[mainKey] = new Hashtable();
            }                

            Hashtable subTable = (Hashtable)Main[mainKey];
            subTable.Add(subKey, value); 

            if (!SubKeys.Contains(subKey))
            {
                SubKeys.Add(subKey, SubKeys.Count); 
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

        public virtual Hashtable SubKeyTable { get { return SubKeys; } }

        public virtual int SubKeyIndex(object subKey)
        {
            if (SubKeys.ContainsKey(subKey))
            {
                return (int)SubKeys[subKey];
            }                
            return -1;
        }

        public virtual Array GetMainKeyArray()
        {
            object[] array = new object[Main.Keys.Count];
            Main.Keys.CopyTo(array, 0);
            return array;
        }

        public virtual Array GetSubKeyArray()
        {
            object[] array = new object[SubKeys.Keys.Count];
            SubKeys.Keys.CopyTo(array, 0);
            return array;
        }

        public virtual bool ContainsMainKey(object mainKey)
        {
            return Main.ContainsKey(mainKey);
        }

        public virtual bool ContainsSubKey(object subKey)
        {
            return SubKeys.ContainsKey(subKey);
        }
    }


    public class MyTableTest
    {
        /// <summary>
        /// A case for testing integer index.
        /// </summary>
        public static void Test()
        {
            MyTable t = new MyTable();
            
            const int size = 700;
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
                //Console.WriteLine("{0} Numbers", i*size);
                long usedMemory = Process.GetCurrentProcess().WorkingSet64;
                Console.WriteLine("{0} Numbers, MM:{1} MB ", i * size, usedMemory / 1024 / 1024);
            }
        }



        public static void Read()
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
                        t.Add(i, j, r.NextDouble());
                    }
                }
            }

            double sum = 0.0;
            foreach (var _r in t.Keys)
            {
                Hashtable row = (Hashtable)t[_r];
                foreach (var _c in row.Keys)
                {
                    sum += (double)row[_c];
                }
            }
            Console.WriteLine("sum,{0}", sum);
            long usedMemory = Process.GetCurrentProcess().WorkingSet64;
            Console.WriteLine("{0} Numbers, MM:{1} MB ", size * size, usedMemory / 1024 / 1024);
        }

        /// <summary>
        /// a case for testing string index.
        /// </summary>
        public static void Test2()
        {
            MyTable t = new MyTable();

            const int size = 700;
            Random r = new Random();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (!t.ContainsKey(i.ToString(), j.ToString()))
                    {
                        t.Add(i.ToString(), j.ToString(), r.Next());
                    }
                }
                Console.WriteLine("{0} Numbers", i * size);
            }
        }
    }

}
