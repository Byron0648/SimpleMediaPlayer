using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace SimpleMediaPlayer
{
    class GetSong
    {
        /// <summary>
        /// http下载歌曲文件
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool HttpDownloadMp3(string[] str, string path)
        {
            //文件夹路径
            //string tempPath = System.IO.Path.GetDirectoryName(path) + @"\temp";
            System.IO.Directory.CreateDirectory(path);

            //文件路径
            string tempFile = path + "\\" + str[1] + "-" + str[2] + ".mp3"; //临时文件

            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);    //存在则删除
            }

            try
            {
                // 设置参数
                FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                //发送请求并获取相应回应数据//
                HttpWebRequest request = WebRequest.Create(@"http://music.163.com/song/media/outer/url?id=" + str[0]) as HttpWebRequest;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //创建本地文件写入流
                Stream responseStream = response.GetResponseStream();

                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                fs.Close();
                responseStream.Close();
                //System.IO.File.Move(tempFile, path);
                Console.WriteLine(str[1] + "-" + str[2] + ".mp3已下载完成");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// http下载歌词文件
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool HttpDownloadLrc(string[] str, string path)
        {
            //文件夹路径
            //string tempPath = System.IO.Path.GetDirectoryName(path) + @"\temp";
            System.IO.Directory.CreateDirectory(path);

            //文件路径
            string tempFile = path + "\\" + str[1] + "-" + str[2] + ".lrc"; //临时文件

            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);    //存在则删除
            }

            try
            {
                // 设置参数
                FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                //发送请求并获取相应回应数据//
                HttpWebRequest request = WebRequest.Create(@"http://music.163.com/api/song/lyric?id=" + str[0] + @"&lv=1&kv=1&tv=-1") as HttpWebRequest;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //创建本地文件写入流
                Stream responseStream = response.GetResponseStream();

                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                fs.Close();
                responseStream.Close();
                //System.IO.File.Move(tempFile, path);
                Console.WriteLine(str[1] + "-" + str[2] + ".lrc已下载完成");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// http下载图片文件
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool HttpDownloadPic(string[] str, string path)
        {
            //文件夹路径
            //string tempPath = System.IO.Path.GetDirectoryName(path) + @"\temp";
            System.IO.Directory.CreateDirectory(path);

            //文件路径
            string tempFile = path + "\\" + str[1] + "-" + str[2] + ".jpg"; //临时文件

            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);    //存在则删除
            }

            try
            {
                // 设置参数
                FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                //发送请求并获取相应回应数据//
                HttpWebRequest request = WebRequest.Create(@str[3]) as HttpWebRequest;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //创建本地文件写入流
                Stream responseStream = response.GetResponseStream();

                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                fs.Close();
                responseStream.Close();
                //System.IO.File.Move(tempFile, path);
                Console.WriteLine(str[1] + "-" + str[2] + ".jpg已下载完成");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 与数据库连接
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection GetConnection()
        {
            MySqlConnection connection = new MySqlConnection(
                "server=localhost;user id=root;password=MySQL;database=mediaplayer;charset=utf8");
            return connection;
        }

        /// <summary>
        /// 通过歌手得到歌手图片
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        public static string GetPic(string artistname)
        {
            string pic = "";
            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    string cmdStr = "select * from artist where artistname = \"" + artistname + "\";";

                    using (MySqlCommand cmd = new MySqlCommand(cmdStr, connection))
                    {
                        connection.Open();
                        cmd.Prepare();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                pic = reader[3].ToString();
                            }
                        }
                    }
                }
                return pic;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return pic;
            }
        }

        /// <summary>
        /// 根据字符串在数据库中查找，返回一个List
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<string[]> GetSongList(string str)
        {
            try
            {
                //id name artist
                //string[] songDetails = new string[3];
                //歌曲列表
                List<string[]> songs = new List<string[]>();

                using (MySqlConnection connection = GetConnection())
                {
                    string cmdStr = "select * from song where songname like \"%"
                        + str + "%\" or songartist like \"%" + str + "%\";";

                    using (MySqlCommand cmd = new MySqlCommand(cmdStr, connection))
                    {
                        connection.Open();
                        cmd.Prepare();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string[] songDetails = new string[4];
                                songDetails[0] = reader[1].ToString();
                                songDetails[1] = reader[2].ToString();
                                songDetails[2] = reader[3].ToString();
                                songs.Add(songDetails);
                            }
                        }
                    }
                }
                foreach (var item in songs)
                {
                    item[3] = GetPic(item[2]);
                }
                return songs;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}

