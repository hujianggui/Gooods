using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RS.DataType;
using System.Collections;

namespace RS.Data
{
    /// <summary>
    /// www.informatik.uni-freiburg.de/~cziegler/BX
    /// Contains the book rating information. Ratings ('Book-Rating') are either explicit, 
    /// expressed on a scale from 1-10 (higher values denoting higher appreciation), or implicit, expressed by 0.
    /// </summary>
    public class BookCrossing
    {
        public class User
        {
            public string UserId { get; set; }
            public string Location { get; set; }
            public string Age { get; set; } // Maybe NULL

            public User(string uId, string location, string age)
            {
                this.UserId = uId;
                this.Location = location;
                this.Age = age;
            }
        }

        public class Book
        {
            public string Isbn { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Year { get; set; }
            public string Publisher { get; set; }
            public string ImageUrlS { get; set; }
            public string ImageUrlM { get; set; }
            public string ImageUrlL { get; set; }
        }


        public static string DefaultDirectory = @"D:\data\dataset_dm\BX-CSV-Dump\";
        public static string DefaultRatingFile = DefaultDirectory + @"BX-Book-Ratings.csv";
        public static string DefaultUserFile = DefaultDirectory + @"BX-Users.csv";
        public static string DefaultBookFile = DefaultDirectory + @"BX-Books.csv";

        public static string RatingFile = DefaultDirectory + @"rating.csv";
        public static string BaseRatingFile = DefaultDirectory + @"bx.base.csv";
        public static string TestRatingFile = DefaultDirectory + @"bx.test.csv ";



        /// <summary>
        /// Get user from BX-Users.csv
        /// </summary>
        /// <param name="file">path of BX-Users.csv</param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<User> GetUsers(string file, string separator = ";")
        {
            if (!new FileInfo(file).Exists)
            {
                throw new ArgumentException("File doesn't exist: " + file);
            }

            StreamReader reader = new StreamReader(file);
            List<User> users = new List<User>();
            string _l = reader.ReadLine();
            char[] separators = separator.ToArray();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] elements = line.Split(separators, StringSplitOptions.None);
                if (elements.Length == 3)
                {
                    string uid = elements[0].TrimStart('\"').TrimEnd('\"');
                    string loc = elements[1].TrimStart('\"').TrimEnd('\"');
                    string age = elements[2].TrimStart('\"').TrimEnd('\"');
                    User r = new User(uid, loc, age);
                    users.Add(r);
                }
            }
            reader.Close();
            return users;
        }

        /// <summary>
        /// Get books from BX-Books.csv
        /// </summary>
        /// <param name="file">path of BX-Books.csv</param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<Book> GetBooks(string file, string separator = ";")
        {
            if (!new FileInfo(file).Exists)
            {
                throw new ArgumentException("File doesn't exist: " + file);
            }

            StreamReader reader = new StreamReader(file);
            List<Book> books = new List<Book>();
            string _l = reader.ReadLine();
            char[] separators = separator.ToArray();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] elements = line.Split(separators, StringSplitOptions.None);

                if (elements.Length == 8)
                {
                    Book r = new Book();
                    r.Isbn = elements[0].TrimStart('\"').TrimEnd('\"');
                    r.Title = elements[1].TrimStart('\"').TrimEnd('\"');
                    r.Author = elements[2].TrimStart('\"').TrimEnd('\"');
                    r.Year = elements[3].TrimStart('\"').TrimEnd('\"');
                    r.Publisher = elements[4].TrimStart('\"').TrimEnd('\"');
                    r.ImageUrlS = elements[5].TrimStart('\"').TrimEnd('\"');
                    r.ImageUrlM = elements[6].TrimStart('\"').TrimEnd('\"');
                    r.ImageUrlL = elements[7].TrimStart('\"').TrimEnd('\"');

                    books.Add(r);
                }
            }
            reader.Close();
            return books;
        }


        /// <summary>
        /// Get ratings from BX-Book-Ratings.csv
        /// </summary>
        /// <param name="file"></param>
        /// <returns>ratings table: string, userId; string, isbn; double score</returns>
        public static MyTable GetRatings(string file, string separator = ";")
        {
            if (!new FileInfo(file).Exists)
            {
                throw new ArgumentException("File doesn't exist: " + file);
            }

            MyTable ratingsTable = new MyTable();
            StreamReader reader = new StreamReader(file);
            string _l = reader.ReadLine();
            char[] separators = separator.ToArray();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().TrimStart('\"').TrimEnd('\"');
                string[] elements = line.Split(separators, StringSplitOptions.None);

                if (elements.Length == 3)
                {
                    string id = elements[0].TrimStart('\"').TrimEnd('\"');
                    string isbn = elements[1].TrimStart('\"').TrimEnd('\"');
                    string sRate = elements[2].TrimStart('\"').TrimEnd('\"');
                    double rate = Double.Parse(sRate);

                    if (!ratingsTable.ContainsKey(id, isbn))
                    {
                        ratingsTable.Add(id, isbn, rate);
                    }
                }
            }
            reader.Close();
            return ratingsTable;
        }

        public static List<Rating> GetRatings(MyTable table)
        {
            List<Rating> ratings = new List<Rating>();
            ArrayList mainKeys = new ArrayList(table.Keys);

            for (int i = 0; i < mainKeys.Count; i++)
            {
                string mKey = (string)mainKeys[i];
                Hashtable sTable = (Hashtable)table[mKey];
                int uId = i + 1;

                foreach (string isbn in sTable.Keys)
                {
                    int iId = table.SubKeyIndex(isbn);
                    double rate = (double)sTable[isbn];
                    Rating r = new Rating(uId, iId, rate);
                    ratings.Add(r);
                }
            }
            return ratings;
        }


        public static void Preprocessing()
        {
            List<User> users = GetUsers(DefaultUserFile);
            List<Book> books = GetBooks(DefaultBookFile);
            MyTable table = GetRatings(DefaultRatingFile);

            Hashtable usersTable = new Hashtable();
            foreach (User u in users)
            {
                if (!usersTable.ContainsKey(u.UserId))
                {
                    usersTable.Add(u.UserId, u);
                }
            }

            int counter25 = 0, counter50 = 0;

            // Some statics
            foreach (string userId in table.Keys)
            {
                if (usersTable.ContainsKey(userId))
                {
                    User u = (User)usersTable[userId];
                    int age = u.Age == "NULL" ? 0 : Int32.Parse(u.Age);

                    if (age <= 25 && age > 0)
                    {
                        Hashtable subTable = (Hashtable)table[userId];
                        counter25 += subTable.Keys.Count;
                    }

                    if (age > 50)
                    {
                        Hashtable subTable = (Hashtable)table[userId];
                        counter50 += subTable.Keys.Count;
                    }
                }
            }

            Console.WriteLine("{0}", counter25);
            Console.WriteLine("{0}", counter50);
        }

        public static void Statistics()
        {

        }

    }
}
