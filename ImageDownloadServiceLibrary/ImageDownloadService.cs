using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageDownloadServiceLibrary
{
    public class ImageDownloadService : IImageDownloadService
    {
        private String[] source = { "Bing", "Instagram" };

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public bool saveTodaysBingWallpaper()
        {
            bool status = false;
            String bingWallpaperUrl = getBingWallpaperURL();
            String bingWallpaperName = bingWallpaperUrl.Substring(bingWallpaperUrl.LastIndexOf("/") + 1); //image name
            bingWallpaperUrl = bingWallpaperUrl.Replace("\\/", "/"); ;
            String bingWallpaperPath = "./" + bingWallpaperName;

            using (var imageClient = new WebClient())
            {
                imageClient.DownloadFile(bingWallpaperUrl, bingWallpaperPath);
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(bingWallpaperPath);

            using (SqlConnection sqlconnection = new SqlConnection("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=image_downloader;Integrated Security=True"))
            {
                sqlconnection.Open();

                SqlCommand cmd = new SqlCommand("SELECT [image_name] FROM [images] Where [source] = '"+source[0]+"';", sqlconnection);
                SqlDataReader reader = cmd.ExecuteReader();
                if(reader.Read())
                {
                    if(bingWallpaperName == reader["image_name"].ToString())
                    {
                        status = true;
                        sqlconnection.Close();
                        return status;
                    }
                    else
                    {
                        sqlconnection.Close();
                        string insertXmlQuery = @"INSERT INTO [images] ([image], [source], [image_name]) VALUES (@imageBytes, @source, @imageName)";
                        //Insert Image into Sql Table
                        SqlCommand insertCommand = new SqlCommand(insertXmlQuery, sqlconnection);
                        SqlParameter sqlParam = insertCommand.Parameters.AddWithValue("@imageBytes", imageBytes);
                        sqlParam.DbType = System.Data.DbType.Binary;
                        insertCommand.Parameters.AddWithValue("@source", source[0]);
                        insertCommand.Parameters.AddWithValue("@imageName", bingWallpaperName);

                        int recordsAffected = insertCommand.ExecuteNonQuery();
                        if (recordsAffected >= 1)
                            status = true;
                        else status = false;
                    }
                }
                else
                {
                    sqlconnection.Close();
                    string insertXmlQuery = @"INSERT INTO [images] ([image], [source], [image_name]) VALUES (@imageBytes, @source, @imageName)";
                    //Insert Image into Sql Table
                    SqlCommand insertCommand = new SqlCommand(insertXmlQuery, sqlconnection);
                    SqlParameter sqlParam = insertCommand.Parameters.AddWithValue("@imageBytes", imageBytes);
                    sqlParam.DbType = System.Data.DbType.Binary;
                    insertCommand.Parameters.AddWithValue("@source", source[0]);
                    insertCommand.Parameters.AddWithValue("@imageName", bingWallpaperName);
                    sqlconnection.Open();
                    int recordsAffected = insertCommand.ExecuteNonQuery();
                    if (recordsAffected >= 1)
                        status = true;
                    else status = false;
                }
            }
            return status;
        }

        public String getBingWallpaperURL()
        {
            String urlimg = null;
            String url = "http://www.bing.com/";

            String response = "";
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 5.1; rv:19.0) Gecko/20100101 Firefox/19.0");
                response = client.DownloadString("https://www.bing.com/?cc=ca");
            }
            
            Regex imgRegex = new Regex(@"[/]{2}[a-zA-Z0-9./_-]+[.]jpg");
            Match imgMatch = imgRegex.Match(response);

            Regex imgRegex1 = new Regex(@"[/]az[a-zA-Z0-9./_-]+[.]jpg");
            Match imgMatch1 = imgRegex1.Match(response);

            if (imgMatch.Success)
            {
                urlimg = "http:" + imgMatch;
            }
            else if (imgMatch1.Success)
            {
                urlimg = url + imgMatch1;
            }

            return urlimg;
        }
    }
}
