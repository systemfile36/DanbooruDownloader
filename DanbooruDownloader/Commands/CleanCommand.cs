using DanbooruDownloader.Utilities;
using Microsoft.Data.Sqlite;
using NLog;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanbooruDownloader.Commands
{
    public class CleanCommand
    {
        static Logger Log = LogManager.GetCurrentClassLogger();

        public static void Run(string path, string condition, string metaDatabaseName)
        {
            string imageDirPath = Path.Combine(path, PathUtility.IMG_DIR_NAME);
            string metaDatabasePath = Path.Combine(path, metaDatabaseName);

            if(!Directory.Exists(path) || !Directory.Exists(imageDirPath))
            {
                Log.Error($"Directory '{path}' is not exists.");
                return;
            }

            if(!File.Exists(metaDatabasePath))
            {
                Log.Error($"{metaDatabaseName} is not exists in {path}");
                return;
            }

            using (SqliteConnection connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = metaDatabasePath,
            }.ToString()))
            {
                connection.Open();

                //Count rows that matched condition
                long count = SQLiteUtility.CountByCondition(connection, condition);

                //if no rows matched condition, exit command
                if(count <= 0)
                {
                    Log.Info($"There is no matched data : {condition}");
                    return;
                }

                Log.Info($"{count} records matched '{condition}'. It will be deleted.");
                Console.Write("Are you sure?");
                
                if(!AskYesNo())
                {
                    Log.Info($"Canceled. Condition : {condition}");
                    return;
                }

                Log.Info($"Deleting {count} records and image data...");
                //Get md5 and file_ext from posts
                using (var reader = SQLiteUtility.GetReaderByColumnNames(connection, condition, "md5", "file_ext"))
                {
                    while (reader.Read())
                    {
                        string md5 = reader.GetString(0);
                        string extension = reader.GetString(1);

                        //Get path of image and metada 
                        string imagePath = PathUtility.GetLocalImagePath(imageDirPath, md5, extension);
                        string metadataPath = PathUtility.GetLocalMetadataPath(imageDirPath, md5);

                        try
                        {
                            //Delete image file
                            File.Delete(imagePath);
                            Log.Info($"Image file : {imagePath} is deleted");

                            //Delete metadata file 
                            File.Delete(metadataPath);
                            Log.Info($"Metadata file : {metadataPath} is deleted");
                        } catch(Exception ex)
                        {
                            Log.Error(ex);
                            return;
                        }
                    }
                }

                //Delete from database
                Log.Info($"Deleting {count} records from Database...");
                int deleteRows = SQLiteUtility.DeleteByCondition(connection, condition);

                if (deleteRows > 0)
                {
                    Log.Info($"Delete successfully");
                } else
                {
                    Log.Error($"Delete failed. Condition : {condition}");
                }

                Log.Info("Clean command is complete.");

            }
        }

        static bool AskYesNo()
        {
            while (true)
            {
                Console.Write("[Y/n]");
                string input = Console.ReadLine()?.Trim().ToLower();

                if (input == "y") return true;
                if (input == "n") return false;

                Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
            }
        }
    }
}
