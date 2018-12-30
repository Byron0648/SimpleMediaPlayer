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
    class CrawlSong
    {
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
        /// 向artistTable中加载id为i到j的歌手
        /// </summary>
        /// <param name="artistTable"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public static void GetArtist(Dictionary<string, string> artistTable, int i, int j)
        {
            try
            {
                //从数据库中加载每个歌手及其id
                Console.WriteLine("开始从数据库中加载数据");
                using (MySqlConnection connection = GetConnection())
                {
                    string str1 = ("select * from artist where id >= " + i +
                        " && id <= " + j + ";");

                    //将15个歌手的歌曲加载到字典里
                    using (MySqlCommand cmd = new MySqlCommand(str1, connection))
                    {
                        connection.Open();
                        cmd.Prepare();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                artistTable.Add(reader[1].ToString(), reader[2].ToString());
                            }
                        }
                        reader.Close();
                    }
                    //得到了一个15个元素的字典，key=歌手id，value=歌手name
                }
                Console.WriteLine("加载歌手完成");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("爬取id为" + i + "-" + j + "的歌手时出错");
            }
        }

        /// <summary>
        /// 获得artistTable中的歌手的歌曲并加载到songTable中
        /// </summary>
        /// <param name="artistTable"></param>
        /// <param name="songTable"></param>
        public static void GetSongDetail(Dictionary<string, string> artistTable,
            List<string[]> songTable)
        {
            //遍历每个歌手网页，把歌曲爬到字典里去
            foreach (var item1 in artistTable)
            {
                try
                {
                    //设置参数
                    HttpWebRequest request = WebRequest.Create("http://music.163.com/artist?id=" + item1.Key) as HttpWebRequest;
                    request.UserAgent = @"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)";
                    //发送请求并获取相应回应数据
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    //直到request.GetResponse()程序才开始向目标网页发送Post请求
                    Stream responseStream = response.GetResponseStream();
                    StreamReader sr = new StreamReader(responseStream, Encoding.UTF8);
                    //返回结果网页（html）代码
                    string content = sr.ReadToEnd();

                    if (content != "")
                    {
                        string pattern1 = "<ul\\sclass=\"f-hide\">.*?</ul>";

                        Regex rx1 = new Regex(pattern1);
                        Match mc1 = rx1.Match(content);
                        //得到所有歌的字符串


                        string pattern2 = "<a(\\s+(href=\"(?<url>([^\"])*)\"))+>(?<name>(.*?))</a>";
                        Regex rx2 = new Regex(pattern2);
                        MatchCollection mc2 = rx2.Matches(mc1.ToString());

                        //将所有歌加载到字典中
                        foreach (Match item2 in mc2)
                        {
                            Regex rxId = new Regex("\\d+");
                            Match mcId = rxId.Match(item2.Result("${url}"));
                            string id = mcId.ToString();
                            string name = item2.Result("${name}");
                            string artist = item1.Value;
                            if (id.Length < 15 && name.Length < 50 && artist.Length < 30)
                            {
                                string[] str = { id, name, artist };
                                songTable.Add(str);
                            }
                            else
                            {
                                Console.WriteLine(name + "的长度超出范围");
                            }
                        }
                        Console.WriteLine(item1.Value + "已爬取完");
                    }
                    else
                    {
                        Console.WriteLine(item1.Value + "未从中获取到内容");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("当前爬取的歌手：" + item1.Value);
                }
            }
        }

        /// <summary>
        /// 将songTable表中的数据上传到数据库
        /// </summary>
        /// <param name="songTable">id和name</param>
        public static void AddSong(List<string[]> songTable)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str.Append("INSERT INTO song (songid, songname, songartist) VALUES ");
                foreach (var item in songTable)
                {
                    str.Append("(\"" + item[0] + "\",\"" + item[1] + "\",\"" + item[2] + "\"),");
                }
                str.Remove(str.Length - 1, 1);
                str.Append(";");

                Console.WriteLine("开始向数据库上传歌曲数据");
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(str.ToString(), connection))
                    {
                        connection.Open();
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("上传完成");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Write("此时songTable表为：");
                foreach (var item in songTable)
                {
                    Console.WriteLine(item[0] + "\t" + item[1]);
                }
            }
        }

        /// <summary>
        /// 开始爬取歌曲
        /// </summary>
        public static void StartCrawl()
        {
            //可以爬5*i个歌手的全部歌曲
            for (int i = 1; i <= 100; i++)
            {
                Dictionary<string, string> artistTable = new Dictionary<string, string>();
                List<string[]> songTable = new List<string[]>();

                //加载歌手到artistTable中
                GetArtist(artistTable, 5 * i - 4, i * 5);
                //遍历字典，得到字典中歌手的全部歌曲以及对应的歌手
                GetSongDetail(artistTable, songTable);
                //加载并上传数据库
                AddSong(songTable);
            }
        }
    }
}
