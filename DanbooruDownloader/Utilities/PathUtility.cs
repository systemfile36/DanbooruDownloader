using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DanbooruDownloader.Utilities
{
    public static class PathUtility
    {
        public const string TEMP_DIR_NAME = "_temp";
        public const string IMG_DIR_NAME = "images";
        public const string META_DB_NAME = "danbooru.sqlite";

        public const string META_FILE_SUFFIX = "-danbooru.json";

        public static void CreateDirectoryIfNotExists(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        public static void ChangeFileTimestamp(string path, DateTime createTime, DateTime updateTime)
        {
            File.SetCreationTime(path, createTime);
            File.SetLastWriteTime(path, updateTime);
        }

        //
        public static string GetLocalMetadataPath(string imgDirPath, string md5)
        {
            return Path.Combine(imgDirPath, md5.Substring(0, 2), string.Concat(md5, META_FILE_SUFFIX));
        }

        public static string GetLocalImagePath(string imgDirPath, string md5, string extension)
        {
            return Path.Combine(imgDirPath, md5.Substring (0, 2), $"{md5}.{extension}");
        }
    }
}
