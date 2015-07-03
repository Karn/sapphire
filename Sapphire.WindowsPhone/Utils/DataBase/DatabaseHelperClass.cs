using Sapphire.Utils.DataBase.DataTables;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sapphire.Utils.DataBase {
    //This class for perform all database CRUD operations    
    public class DatabaseHelperClass {
        SQLiteConnection dbConn;


        public const string DB_NAME = "ApplicationStorage.sqlite";
        public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_NAME));//DataBase Name


        public void CreateDB() {
            if (!CheckFileExists(DB_NAME).Result) {
                using (dbConn = new SQLiteConnection(DB_PATH)) {
                    dbConn.CreateTable<UserBlogs>();
                }
            }
        }
        private static async Task<bool> CheckFileExists(string fileName) {
            try {
                var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            } catch {
            }
            return false;
        }

        //Create Tabble    
        public async Task<bool> onCreate(string DB_PATH) {
            try {
                if (!CheckFileExists(DB_PATH).Result) {
                    using (dbConn = new SQLiteConnection(DB_PATH)) {
                        dbConn.CreateTable<UserBlogs>();
                    }
                }
                return true;
            } catch {
                return false;
            }
        }

        // Retrieve the specific contact from the database.    
        public UserBlogs GetBlog(string blogName) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                return dbConn.Query<UserBlogs>("select * from UserBlogs where BlogName =" + blogName).FirstOrDefault();
            }
        }
        // Retrieve the all contact list from the database.    
        public List<UserBlogs> GetBlogs() {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                return dbConn.Table<UserBlogs>().ToList();
            }
        }

        //Update existing conatct    
        public void AddOrUpdateBlog(UserBlogs blog) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                var existingBlog = dbConn.Query<UserBlogs>("select * from UserBlogs where BlogName =" + blog.BlogName).FirstOrDefault();
                if (existingBlog != null) {
                    existingBlog.BlogName = blog.BlogName;
                    existingBlog.Title = blog.Title;
                    existingBlog.Avatar = blog.Avatar;
                    existingBlog.Description = blog.Description;
                    existingBlog.Url = blog.Url;
                    existingBlog.PostCount = blog.PostCount;
                    existingBlog.LikedPostCount = blog.LikedPostCount;
                    existingBlog.FollowersCount = blog.FollowersCount;
                    existingBlog.FollowingCount = blog.FollowingCount;
                    dbConn.RunInTransaction(() => {
                        dbConn.Update(existingBlog);
                    });
                } else {
                    AddBlog(blog);
                }
            }
        }
        // Insert the new contact in the Contacts table.    
        public void AddBlog(UserBlogs newBlog) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                dbConn.RunInTransaction(() => {
                    dbConn.Insert(newBlog);
                });
            }
        }

        //Delete specific contact    
        public void RemoveBlog(string blogName) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                var existingconact = dbConn.Query<UserBlogs>("select * from UserBlogs where BlogName =" + blogName).FirstOrDefault();
                if (existingconact != null) {
                    dbConn.RunInTransaction(() => {
                        dbConn.Delete(existingconact);
                    });
                }
            }
        }

        //Delete all contactlist or delete Contacts table    
        public void DeleteAllBlogs() {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                //dbConn.RunInTransaction(() =>    
                //   {    
                dbConn.DropTable<UserBlogs>();
                dbConn.CreateTable<UserBlogs>();
                dbConn.Dispose();
                dbConn.Close();
                //});    
            }
        }
    }

}
