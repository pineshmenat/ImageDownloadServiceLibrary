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
        private enum Source { Bing, Instagram };

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public bool saveTodaysBingWallpaper()
        {
            bool status = false;
            String bingWallpaperUrl = getImageURL("https://www.bing.com/?cc=ca", "[/]{2}[a-zA-Z0-9./_-]+[.]jpg", "[/]az[a-zA-Z0-9./_-]+[.]jpg");
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

                SqlCommand cmd = new SqlCommand("SELECT [image_name] FROM [images] Where [source] = '"+Source.Bing+"';", sqlconnection);
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
                        insertCommand.Parameters.AddWithValue("@source", "" + Source.Bing);
                        insertCommand.Parameters.AddWithValue("@imageName", bingWallpaperName);
                        sqlconnection.Open();
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
                    insertCommand.Parameters.AddWithValue("@source", "" + Source.Bing);
                    insertCommand.Parameters.AddWithValue("@imageName", bingWallpaperName);
                    sqlconnection.Open();
                    int recordsAffected = insertCommand.ExecuteNonQuery();
                    if (recordsAffected >= 1)
                        status = true;
                    else status = false;
                }
            }
            if (File.Exists(bingWallpaperPath))
            {
                File.Delete(bingWallpaperPath);
            }
            return status;
        }

        public String getImageURL(String url, String Regex, String Regex1)
        {
            String urlimg = null;
            String response = "";

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 5.1; rv:19.0) Gecko/20100101 Firefox/19.0");
                response = client.DownloadString(url);
            }

            //Regex imgRegex = new Regex(@"[/]{2}[a-zA-Z0-9./_-]+[.]jpg");
            Regex imgRegex = new Regex(@""+Regex+"");
            Match imgMatch = imgRegex.Match(response);

            //Regex imgRegex1 = new Regex(@"[/]az[a-zA-Z0-9./_-]+[.]jpg");
            Regex imgRegex1 = new Regex(@"" + Regex1 + "");
            Match imgMatch1 = imgRegex1.Match(response);

            if(imgMatch.ToString().IndexOf("instagram") > 0)
            {
                urlimg = imgMatch.ToString();
            } 
            else if(imgMatch.Success)
            {
                urlimg = "http:" + imgMatch;
            }
            else if (imgMatch1.Success)
            {
                urlimg = url + imgMatch1;
            }

            return urlimg;
        }

        public bool downloadInstagramImage(String url)
        {
            bool status = false;

            int index = url.IndexOf("?");
            if (index > 0)
                url = url.Substring(0, index);

            if (url.Contains("www.instagram.com"))
            {
                String instagramImageUrl = getImageURL(url, "https://instagram.[a-z0-9-]+.fna.fbcdn.net/[a-zA-Z0-9./_-]+[.]jpg", "");
                String instagramImageName = instagramImageUrl.Substring(instagramImageUrl.LastIndexOf("p/") + 1);
                instagramImageName = instagramImageName.Replace("/", "");
                String instagramImagePath = "./" + instagramImageName;

                using (var imageClient = new WebClient())
                {
                    imageClient.DownloadFile(instagramImageUrl, instagramImagePath);
                }

                byte[] imageBytes = System.IO.File.ReadAllBytes(instagramImagePath);

                using (SqlConnection sqlconnection = new SqlConnection("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=image_downloader;Integrated Security=True"))
                {
                    sqlconnection.Open();

                    SqlCommand cmd = new SqlCommand("SELECT [image_name] FROM [images] Where [source] = '" + Source.Instagram + "';", sqlconnection);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (instagramImageName == reader["image_name"].ToString())
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
                            insertCommand.Parameters.AddWithValue("@source", "" + Source.Instagram);
                            insertCommand.Parameters.AddWithValue("@imageName", instagramImageName);
                            sqlconnection.Open();
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
                        insertCommand.Parameters.AddWithValue("@source", ""+ Source.Instagram);
                        insertCommand.Parameters.AddWithValue("@imageName", instagramImageName);
                        sqlconnection.Open();
                        int recordsAffected = insertCommand.ExecuteNonQuery();
                        if (recordsAffected >= 1)
                            status = true;
                        else status = false;
                    }
                }
                if (File.Exists(instagramImagePath))
                {
                    File.Delete(instagramImagePath);
                }
            } else
            {
                InvalidUrlFault fault = new InvalidUrlFault();
                fault.Error = "Invalid URL";
                fault.Details = "Invalid Instagram URL!!";
                throw new FaultException<InvalidUrlFault>(fault);
            }
            return status;
        }

        public List<Image> getBingWallpapers()
        {
            List<Image> bingWallpapers = new List<Image>();

            using (SqlConnection sqlconnection = new SqlConnection("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=image_downloader;Integrated Security=True"))
            {
                SqlCommand cmd = new SqlCommand("SELECT [image_id],[image],[source],[created_date],[image_name] FROM [images] WHERE [source] = '" + Source.Bing + "'", sqlconnection);
                sqlconnection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Image image = new Image();
                    image.image_id = Convert.ToInt32(reader["image_id"]);
                    image.image = (byte[])reader["image"];
                    image.source = reader["source"].ToString();
                    image.created_date = (DateTime.Parse(reader["created_date"].ToString())).Date;
                    image.image_name= reader["image_name"].ToString();
                    bingWallpapers.Add(image);
                }
                sqlconnection.Close();
            }

            return bingWallpapers;
        }

        public List<Image> getInstagramImages()
        {
            List<Image> instagramImages = new List<Image>();

            using (SqlConnection sqlconnection = new SqlConnection("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=image_downloader;Integrated Security=True"))
            {
                SqlCommand cmd = new SqlCommand("SELECT [image_id],[image],[source],[created_date],[image_name] FROM [images] WHERE [source] = '"+Source.Instagram+"'", sqlconnection);
                sqlconnection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Image image = new Image();
                    image.image_id = Convert.ToInt32(reader["image_id"]);
                    image.image = (byte[])reader["image"];
                    image.source = reader["source"].ToString();
                    image.created_date = (DateTime.Parse(reader["created_date"].ToString())).Date;
                    image.image_name = reader["image_name"].ToString();
                    instagramImages.Add(image);
                }
                sqlconnection.Close();
            }

            return instagramImages;
        }
    }
}
