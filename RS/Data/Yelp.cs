using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

using RS.DataType;
using RS.Algorithm;
using RS.Data.Utility;

namespace RS.Data
{
    /// <summary>
    /// class Business
    /// </summary>
    public class Business
    {
        public string BusinessId { get; set; }

        public string FullAddress { get; set; }

        public List<string> Hours { get; set; } // 营业时间，暂时不设计结构体

        public string City { get; set; }

        public int ReviewCount = 0;

        public string Name { get; set; }

        public double Longitude = 0.0;

        public string State { get; set; }

        public double Stars = 0.0;

        public double Latitude = 0.0;

        public List<string> Attributes { get; set; }    // 

        public string Type { get; set; }

        public static Business ToBusiness(string jsonString)
        {
            JObject jo = JObject.Parse(jsonString);
            //string[] values =jo.Properties().Select(item => item.Value.ToString()).ToArray();

            if (jo == null)
            {
                Console.WriteLine(false);
                return null;
            }
            Business bus = new Business();

            bus.BusinessId = jo["business_id"].ToString();
            bus.FullAddress = jo["full_address"].ToString();
            bus.Longitude = (double)jo["longitude"];
            bus.State = jo["state"].ToString();
            bus.Stars = (int)jo["stars"];
            bus.Latitude = (double)jo["latitude"];
            bus.City = jo["city"].ToString();
            bus.ReviewCount = (int)jo["review_count"];
            bus.Name = jo["name"].ToString();
            bus.Type = jo["type"].ToString();

            return bus;
        }
    }

    /// <summary>
    /// class User
    /// </summary>
    public class User
    {
        public string YelpingSince { get; set; }

        public int ReviewCount = 0;

        public string Name { get; set; }

        public string UserId { get; set; }

        public List<string> Friends { get; set; }

        public int Fans = 0;

        public string Type { get; set; }

        public static User ToUser(string jsonString)
        {
            JObject jo = JObject.Parse(jsonString);
            //string[] values =jo.Properties().Select(item => item.Value.ToString()).ToArray();

            if (jo == null)
            {
                Console.WriteLine(false);
                return null;
            }
            User user = new User();

            user.YelpingSince = jo["yelping_since"].ToString();
            user.ReviewCount = (int)jo["review_count"];
            user.Name = jo["name"].ToString();
            user.UserId = jo["user_id"].ToString();
            user.Friends = JArray.Parse(jo["friends"].ToString()).Select(token => token.ToString()).ToList<string>();
            user.Fans = (int)jo["fans"];

            //user.Type = jo["type"].ToString();
            return user;
        }
    }

    /// <summary>
    /// class Review
    /// </summary>
    public class Review
    {
        public string UserId { get; set; }

        // public string ReviewId { get; set; } // invalid

        public double Stars = 0;

        public string Date { get; set; }

        public string Text { get; set; }

        //public string Type { get; set; }

        public string BusinessId { get; set; }

        public static Review ToReview(string jsonString)
        {
            JObject jo = JObject.Parse(jsonString);
            //string[] values =jo.Properties().Select(item => item.Value.ToString()).ToArray();

            if (jo == null)
            {
                Console.WriteLine(false);
                return null;
            }

            Review review = new Review();
            review.UserId = jo["user_id"].ToString();
            //review.ReviewId = jo["review_id"].ToString();
            review.Stars = (double)jo["stars"];
            review.Date = jo["date"].ToString();
            review.Text = jo["text"].ToString();
            review.BusinessId = jo["business_id"].ToString();
            //review.Type = jo["type"].ToString();
            return review;
        }
    }



    
    /// <summary>
    /// class Yelp, singleton
    /// used for yelp 2015 dataset challenge.
    /// </summary>
    public class Yelp2015
    {
        private Yelp2015() { }
        private static Yelp2015 _instance = null;
        public static Yelp2015 GetInstance()
        {
            if (null == _instance)
            {
                _instance = new Yelp2015();
            }
            return _instance;
        }

        public Hashtable UserTable = new Hashtable();    // Table of User Ids. Key: UserID, Value: Index in matrix
        public Hashtable BusinessTable = new Hashtable();   // Table of Videoes.  Key: UserID, Value: Index in matrix

        //public ArrayList UserList = new ArrayList();
        //public ArrayList BusinessList = new ArrayList();

        public int UserNumber { get { return UserTable.Count; } }
        public int BusinessNumber { get { return BusinessTable.Count; } }

        public int IndexOfUser(string userId)
        {
            if (!UserTable.Contains(userId))
                return -1;
            return (int)UserTable[userId];
        }
        public int IndexOfBusiness(string businessId)
        {
            if (!BusinessTable.Contains(businessId))
                return -1;
            return (int)BusinessTable[businessId];
        }

        public void Reset()
        {
            UserTable = new Hashtable();
            BusinessTable = new Hashtable();
        }

        public void UpdateUsers(List<User> users)
        {
            foreach (User user in users)
            {
                if (!UserTable.ContainsKey(user.UserId))
                {
                    UserTable.Add(user.UserId, UserTable.Count);
                }

                foreach (string f in user.Friends)
                {
                    if (!UserTable.ContainsKey(f))
                    {
                        UserTable.Add(f, UserTable.Count);
                    }
                }
            }
        }

        public void UpdateBusiness(List<Business> bs)
        {
            foreach (Business b in bs)
            {
                if (!BusinessTable.ContainsKey(b.BusinessId))
                {
                    BusinessTable.Add(b.BusinessId, BusinessTable.Count);
                }
            }
        }


        /// <summary>
        /// Get Businesses from a json file.
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public List<Business> GetBusinessesFromJsonFile(string jsonFile)
        {
            if (!new FileInfo(jsonFile).Exists)
                throw new ArgumentException("File doesn't exist: " + jsonFile);

            StreamReader reader = new StreamReader(jsonFile);
            List<Business> businesses = new List<Business>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                Business b = Business.ToBusiness(line);
                businesses.Add(b);
            }

            reader.Close();
            return businesses;
        }

        /// <summary>
        /// Get Users from a json file.
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public List<User> GetUsersFromJsonFile(string jsonFile)
        {
            if (!new FileInfo(jsonFile).Exists)
                throw new ArgumentException("File doesn't exist: " + jsonFile);

            StreamReader reader = new StreamReader(jsonFile);
            List<User> users = new List<User>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                User u = User.ToUser(line);
                users.Add(u);
            }

            reader.Close();
            return users;
        }

        /// <summary>
        /// Get Reviews from a json file
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public List<Review> GetReviewsFromJsonFile(string jsonFile)
        {
            if (!new FileInfo(jsonFile).Exists)
                throw new ArgumentException("File doesn't exist: " + jsonFile);

            StreamReader reader = new StreamReader(jsonFile);
            List<Review> reviews = new List<Review>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                Review r = Review.ToReview(line);
                reviews.Add(r);
            }

            reader.Close();
            return reviews;
        }

        /// <summary>
        /// Write POIs from businesses, POI id are transformed from string to int (zero-based).
        /// </summary>
        /// <param name="businesses"></param>
        /// <param name="toFile"></param>
        /// <param name="separator"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public void WritePOIs(List<Business> businesses, string toFile, string separator = ",", bool append = false, string encoding = "GB2312")
        {
            if (null == businesses)
                throw new ArgumentNullException();

            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            foreach (Business b in businesses)
            {
                writer.WriteLine("{0}{1}{2}{3}{4}", IndexOfBusiness(b.BusinessId), separator, b.Latitude, separator, b.Longitude);
            }
            writer.Close();
        }

        /// <summary>
        /// Write Links from users, user id is transformed from string to int (zero-based).
        /// </summary>
        /// <param name="users"></param>
        /// <param name="toFile"></param>
        /// <param name="separator"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public void WriteLinks(List<User> users, string toFile, string separator = ",", bool append = false, string encoding = "GB2312")
        {
            if (null == users)
                throw new ArgumentNullException();

            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            foreach (User user in users)
            {
                foreach (string f in user.Friends)
                {
                    writer.WriteLine("{0}{1}{2}", IndexOfUser(user.UserId), separator, IndexOfUser(f));
                }
            }
            writer.Close();
        }

        /// <summary>
        ///  Write starts from reviews, user id, poi id are both transformed from string to int (zero-based).
        /// </summary>
        /// <param name="reviews"></param>
        /// <param name="toFile"></param>
        /// <param name="separator"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public void WriteRatings(List<Review> reviews, string toFile, string separator = ",", bool append = false, string encoding = "GB2312")
        {
            if (null == reviews)
                throw new ArgumentNullException();
            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            foreach (Review r in reviews)
            {
                writer.WriteLine("{0}{1}{2}{3}{4}{5}{6}", IndexOfUser(r.UserId), separator, 
                    IndexOfBusiness(r.BusinessId), separator, r.Stars, separator, r.Date);
            }
            writer.Close();
        }


        public static string DefaultDirectory    = @"D:\data\dataset_dm\yelp\";
        public string DefaultUserFile     = DefaultDirectory + @"yelp_academic_dataset_user.json";
        public string DefaultBusinessFile = DefaultDirectory + @"yelp_academic_dataset_business.json";
        public string DefaultReviewFile   = DefaultDirectory + @"yelp_academic_dataset_review.json";
        public string DefaultTipFile      = DefaultDirectory + @"yelp_academic_dataset_tip.json";
        public string DefaultCheckInFile  = DefaultDirectory + @"yelp_academic_dataset_checkin.json";

        public string POIFile = DefaultDirectory + @"POIs.csv";   // Extract from business file
        public string UserFile = DefaultDirectory + @"users.csv";    // Extract from user file
        public string RatingFile = DefaultDirectory + @"ratings.csv"; // Extract from review file
        public string LinkFile = DefaultDirectory + @"links.csv"; // Extract from edge file

        public string BaseRatingFile = DefaultDirectory + @"train.csv";
        public string TestRatingFile = DefaultDirectory + @"test.csv";

        // Preprocessing
        public void Preprocessing()
        {
            // Get POIs                      
            List<Business> bs = GetBusinessesFromJsonFile(DefaultBusinessFile);
            UpdateBusiness(bs);
            WritePOIs(bs, POIFile);

            // Get Users, and write edges
            List<User> users = GetUsersFromJsonFile(DefaultUserFile);
            UpdateUsers(users);
            WriteLinks(users, LinkFile);

            // Get Reviews
            List<Review> reviews = GetReviewsFromJsonFile(DefaultReviewFile);
            WriteRatings(reviews, RatingFile);
            //var v = from u in users group u by u.UserId into g orderby g.Count() descending  select users;
        }
    }

    public class YelpE
    {
        public static string DefaultDirectory = @"D:\data\dataset_dm\yelp\";

        public static string RatingFile = DefaultDirectory + @"ratings.csv"; 
        public static string POIFile    = DefaultDirectory + @"POIs.csv";   
        public static string UserFile   = DefaultDirectory + @"users.csv";    
        public static string LinkFile   = DefaultDirectory + @"links.csv"; 

        public static string BaseRatingFile = DefaultDirectory + @"train.csv";
        public static string TestRatingFile = DefaultDirectory + @"test.csv";

        public static int MaxUserId = 366714 + 1;
        public static int MaxItemId = 61183  + 1;

        public static void APITest()
        {
            // Get Social Edges
            List<Link> edges = Tools.GetLinks(LinkFile);
            var q1 = edges.Max(e => e.From);
            var q2 = edges.Max(e => e.From);
            Console.WriteLine(q1);
            Console.WriteLine(q2);

            // Get ratings
            List<Rating> ratings = Tools.GetRatings(RatingFile, ",");
            var q3 = ratings.Max(e => e.UserId);
            var q4 = ratings.Max(e => e.ItemId);
            var q5 = ratings.Max(e => e.Score);
            var q6 = ratings.Min(e => e.UserId);
            var q7 = ratings.Min(e => e.ItemId);
            var q8 = ratings.Min(e => e.Score);
            Console.WriteLine(q3);
            Console.WriteLine(q4);
            Console.WriteLine(q5);
            Console.WriteLine(q6);
            Console.WriteLine(q7);
            Console.WriteLine(q8);
        }

        // Algorithm testing
        public static void MatrixFactorizationTest(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(RatingFile, ",");
            var max = Tools.GetMaxUserIdAndItemId(ratings);
            var maxUserId = max.Item1 + 1;
            var maxItemId = max.Item2 + 1;
            MatrixFactorization model = new MatrixFactorization(maxUserId, maxItemId);
            var t = Tools.TrainTestSplit(ratings, testSize);
            model.TrySGD(t.Item1, t.Item2, 100);
        }

        public static void BiasedMatrixFactorizationTest(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(RatingFile, ",");
            var max = Tools.GetMaxUserIdAndItemId(ratings);
            var maxUserId = max.Item1 + 1;
            var maxItemId = max.Item2 + 1;
            BiasedMatrixFactorization model = new BiasedMatrixFactorization(maxUserId, maxItemId);
            var t = Tools.TrainTestSplit(ratings, testSize);
            model.TrySGD(t.Item1, t.Item2, 100, 0.005);
        }

    }

}
