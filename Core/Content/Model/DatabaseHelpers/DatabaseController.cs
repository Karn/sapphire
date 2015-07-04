using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Core.Content.Model.DatabaseHelpers {

    /// <summary>
    /// DatabaseController handles creation/initialization of a SQLite Database 
    /// to cache content for the app.
    /// Additionally, it provides an inteface for all database CRUD operations.
    /// </summary>
    public class DatabaseController {
        SQLiteConnection dbConn;

        private static DatabaseController _databaseController;

        private DatabaseController() {
            using (dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                dbConn.CreateTable<Account>(); //Defines table mapping for user accounts.
                dbConn.CreateTable<Blog>(); //Defines table mapping for blogs associated with user accounts.
            }
        }

        public static DatabaseController GetInstance() {
            if (_databaseController == null)
                _databaseController = new DatabaseController();
            return _databaseController;
        }

        private static async Task<bool> CheckFileExists(string fileName) {
            try {
                var store = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            } catch {
            }
            return false;
        }

        #region Blogs
        // Retrieve the specific contact from the database.    
        public Blog GetBlog(string blogName) {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                try {
                    return dbConn.Query<Blog>("select * from Blog where AccountEmail =" + AuthenticationManager.Authentication.SelectedAccount + " and Name =" + blogName).FirstOrDefault();
                } catch {
                    return null;
                }
            }
        }
        // Retrieve the all contact list from the database.    
        public List<Blog> GetBlogs() {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                return dbConn.Table<Blog>().ToList();
            }
        }

        //Update existing conatct    
        public void UpdateBlog(Blog blog) {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                var existingBlog = dbConn.Query<Blog>("select * from Blog where AccountEmail =" + AuthenticationManager.Authentication.SelectedAccount + " and Name =" + blog.Name).FirstOrDefault();
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
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                dbConn.RunInTransaction(() => {
                    try {
                        dbConn.Insert(newBlog);
                    } catch {
                        Debug.WriteLine("Exists.");
                    }
                });
            }
        }

        //Delete specific contact    
        public void RemoveBlog(string blogName) {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                var existingconact = dbConn.Query<Blog>("select * from Blog where AccountEmail =" + AuthenticationManager.Authentication.SelectedAccount + " and Name =" + blogName).FirstOrDefault();
                if (existingconact != null) {
                    dbConn.RunInTransaction(() => {
                        dbConn.Delete(existingconact);
                    });
                }
            }
        }

        //Delete all contactlist or delete Contacts table    
        public void DeleteAllBlogs() {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                //dbConn.RunInTransaction(() =>    
                //   {    
                dbConn.DropTable<Blog>();
                dbConn.CreateTable<Blog>();
                dbConn.Dispose();
                dbConn.Close();
                //});    
            }
        }

        #endregion

        #region Accounts
        // Insert the new contact in the Contacts table.    
        public void AddAccount(Account newUser) {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                dbConn.RunInTransaction(() => {
                    try {
                        dbConn.Insert(newUser);
                    } catch {
                        Debug.WriteLine("Exists.");
                    }
                });
            }
        }

        public Account GetAccount(string accountEmail) {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                try {
                    return dbConn.Query<Account>("select * from Account where AccountEmail =" + accountEmail).FirstOrDefault();
                } catch {
                    return null;
                }
            }
        }

        public void UpdateAccount(Account user) {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                var existingUser = dbConn.Query<Account>("select * from Account where AccountEmail =" + user.AccountEmail).FirstOrDefault();
                if (existingUser != null) {
                    existingUser = user;
                    dbConn.RunInTransaction(() => {
                        dbConn.Update(existingUser);
                    });
                }
            }
        }
 
        public void RemoveAccount(string accountEmail) {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                var existingAccount = dbConn.Query<Account>("select * from Account where AccountEmail =" + accountEmail).FirstOrDefault();
                if (existingAccount != null) {
                    dbConn.RunInTransaction(() => {
                        dbConn.Delete(existingAccount);
                    });
                }
            }
        }
 
        public List<Account> GetAccounts() {
            using (var dbConn = new SQLiteConnection(DatabaseConstants.DB_PATH)) {
                return dbConn.Table<Account>().ToList();
            }
        }
        #endregion

    }

}
