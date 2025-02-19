using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DanbooruDownloader.Utilities
{
    public static class SQLiteUtility
    {
        public static void TryCreateTable(SqliteConnection connection)
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                SqliteCommand command = connection.CreateCommand();

                command.Transaction = transaction;
                command.CommandText = @"
CREATE TABLE IF NOT EXISTS posts
( 
    id INTEGER NOT NULL PRIMARY KEY,
    created_at INTEGER,
    uploader_id INTEGER,
    score INTEGER,
    source TEXT,
    md5 TEXT,
    last_comment_bumped_at INTEGER,
    rating TEXT,
    image_width INTEGER,
    image_height INTEGER,
    tag_string TEXT,
    is_note_locked INTEGER,
    fav_count INTEGER,
    file_ext TEXT,
    last_noted_at INTEGER,
    is_rating_locked INTEGER,
    parent_id INTEGER,
    has_children INTEGER,
    approver_id INTEGER,
    tag_count_general INTEGER,
    tag_count_artist INTEGER,
    tag_count_character INTEGER,
    tag_count_copyright INTEGER,
    file_size INTEGER,
    is_status_locked INTEGER,
    pool_string TEXT,
    up_score INTEGER,
    down_score INTEGER,
    is_pending INTEGER,
    is_flagged INTEGER,
    is_deleted INTEGER,
    tag_count INTEGER,
    updated_at INTEGER,
    is_banned INTEGER,
    pixiv_id INTEGER,
    pixiv_ugoira_frame_data TEXT,
    last_commented_at INTEGER,
    has_active_children INTEGER,
    bit_flags INTEGER,
    tag_count_meta INTEGER,
    keeper_data TEXT,
    uploader_name TEXT,
    has_large INTEGER,
    has_visible_children INTEGER,
    children_ids TEXT,
    is_favorited INTEGER,
    tag_string_general TEXT,
    tag_string_character TEXT,
    tag_string_copyright TEXT,
    tag_string_artist TEXT,
    tag_string_meta TEXT,
    file_url TEXT,
    large_file_url TEXT,
    preview_file_url TEXT
);";
                command.ExecuteNonQuery();

                transaction.Commit();
            }
        }

        public static void InsertOrReplace(SqliteConnection connection, IEnumerable<JObject> posts)
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                foreach (JObject post in posts)
                {
                    //remove 'media_asset' field. (to avoid SQL error)
                    var properties = post.Properties().Where(p => p.Name != "media_asset");

                    SqliteCommand command = connection.CreateCommand();

                    command.CommandText = $@"
INSERT OR REPLACE INTO posts ({string.Join(',', properties.Select(p => p.Name))}) VALUES
({string.Join(',', properties.Select(p => '@' + p.Name))})";

                    foreach (JProperty property in properties)
                    {
                        object value = null;

                        switch (property.Value.Type)
                        {
                            case JTokenType.Boolean:
                                {
                                    value = property.Value.ToObject<bool>();
                                }
                                break;
                            default:
                                {
                                    value = property.Value.ToString();
                                }
                                break;

                        }
                        command.Parameters.AddWithValue($"@{property.Name}", value);
                    }

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Get SqliteDataReader that result of SELECT with condition
        /// </summary>
        /// <param name="connection">Connection to a SQLite DB</param>
        /// <param name="condition">SQL WHERE condition</param>
        /// <param name="columnNames">Column names of table 'posts'</param>
        /// <returns>SqliteDataReader for result</returns>
        public static SqliteDataReader GetReaderByColumnNames(SqliteConnection connection, string condition, params string[] columnNames)
        {
            SqliteCommand command = connection.CreateCommand();

            command.CommandText = $@"SELECT {string.Join(",", columnNames)} FROM posts WHERE {condition}";

            return command.ExecuteReader();
        }

        /// <summary>
        /// Count rows that matched condition and return it.
        /// </summary>
        /// <param name="connection">Connection to a SQLite DB</param>
        /// <param name="condition">SQL WHERE condition</param>
        /// <returns>The number of rows that matched condition</returns>
        public static long CountByCondition(SqliteConnection connection, string condition)
        {
            using(SqliteCommand command = connection.CreateCommand())
            {
                //Query that count posts that match the condition
                command.CommandText = $"SELECT COUNT(*) FROM posts WHERE {condition}";

                //Execute query and return result
                return (long)command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Delete rows that matched condition
        /// </summary>
        /// <param name="connection">Connection to a SQLite DB</param>
        /// <param name="condition">SQL WHERE condition</param>
        /// <returns>The number of rows that deleted</returns>
        public static int DeleteByCondition(SqliteConnection connection, string condition)
        {
            //begin transaction
            using(SqliteTransaction transaction = connection.BeginTransaction())
            {
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM posts WHERE {condition}";
                command.Transaction = transaction;

                int deletedRows = command.ExecuteNonQuery();

                transaction.Commit();

                return deletedRows;
            }
        }
    }
}
