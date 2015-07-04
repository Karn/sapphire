using Core.Content.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sapphire.Utils.DataBase {
    //This class for perform all database CRUD operations    
    public class DatabaseController {
        SQLiteConnection dbConn;


        public const string DB_NAME = "ApplicationStorage.sqlite";
        public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_NAME));//DataBase Name


        public void CreateDB() {
            if (!CheckFileExists(DB_NAME).Result) {
                using (dbConn = new SQLiteConnection(DB_PATH)) {
                    dbConn.CreateTable<Blog>();
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
                        dbConn.CreateTable<Blog>();
                    }
                }
                return true;
            } catch {
                return false;
            }
        }

        // Retrieve the specific contact from the database.    
        public Blog GetBlog(string blogName) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                return dbConn.Query<Blog>("select * from Blog where BlogName =" + blogName).FirstOrDefault();
            }
        }
        // Retrieve the all contact list from the database.    
        public List<Blog> GetBlogs() {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                return dbConn.Table<Blog>().ToList();
            }
        }

        //Update existing conatct    
        public void UpdateBlog(Blog blog) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                var existingBlog = dbConn.Query<Blog>("select * from Blog where Name =" + blog.Name).FirstOrDefault();
                if (existingBlog != null) {
                    existingBlog = blog;
                    dbConn.RunInTransaction(() => {
                        dbConn.Update(existingBlog);
                    });
                }
            }
        }
        // Insert the new contact in the Contacts table.    
        public void AddBlog(Blog newBlog) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                dbConn.RunInTransaction(() => {
                    dbConn.Insert(newBlog);
                });
            }
        }

        //Delete specific contact    
        public void RemoveBlog(string blogName) {
            using (var dbConn = new SQLiteConnection(DB_PATH)) {
                var existingconact = dbConn.Query<Blog>("select * from UserBlogs where Name =" + blogName).FirstOrDefault();
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
                dbConn.DropTable<Blog>();
                dbConn.CreateTable<Blog>();
                dbConn.Dispose();
                dbConn.Close();
                //});    
            }
        }
    }

}
