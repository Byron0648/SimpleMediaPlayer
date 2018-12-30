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
    class CrawlArtist
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
        /// 将板块url中的歌手加载到数组中
        /// </summary>
        /// <param name="url"></param>
        /// <param name="artistTable"></param>
        public static void GetArtistDetail(string url, List<string[]> artistTable)
        {
            try
            {
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
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
                    //匹配歌手姓名和id
                    string pattern = "<a(\\s+(href=\"(?<url>([^\"])*)\")+\\s+(class=\"nm\\snm-icn\\sf-thide\\ss-fc0\")+\\s+(\\w+=\"(([^\"])*)\"))+>(?<name>(.*?))</a>";
                    Regex rx = new Regex(pattern);
                    MatchCollection mc = rx.Matches(content);
                    foreach (Match item in mc)
                    {
                        Regex rxId = new Regex("\\d+");
                        Match mcId = rxId.Match(item.Result("${url}"));
                        string id = mcId.ToString();
                        string name = item.Result("${name}");
                        string pic = GetArtistPic(id);
                        if (id.Length < 15 && name.Length < 30 && pic.Length < 150)
                        {
                            string[] str = { id, name, pic };
                            artistTable.Add(str);
                            Console.WriteLine("已爬取到" + name + "的全部信息");
                        }
                        else
                        {
                            Console.WriteLine(name + "的长度超出范围");
                        }
                    }
                    Console.WriteLine(url + "已爬取完");
                }
                else
                {
                    Console.WriteLine("未从" + url + "中获取到内容");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("当前爬取的网址：" + url);
            }
        }

        /// <summary>
        /// 通过歌手id获得歌手pic
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetArtistPic(string id)
        {
            string str = "";
            try
            {
                //设置参数
                HttpWebRequest request = WebRequest.Create("http://music.163.com/artist?id=" + id) as HttpWebRequest;
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
                    //<meta property="og:image" content="http://p4.music.126.net/ZAP_c5J1QJEvFrR9Lq-adw==/109951163186202231.jpg" />
                    string pattern = "<meta\\sproperty=\"og:image\"\\scontent=\"(?<url>([^\"])*)\"+(.*?)+/>";
                    Regex rx = new Regex(pattern);
                    Match mc = rx.Match(content);
                    str = mc.Result("${url}").ToString();
                }
                else
                {
                    Console.WriteLine("未从 " + id + " 中获取到内容");
                }
                return str;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("当前爬取的歌手id：" + id);
            }
            return str;
        }

        /// <summary>
        /// 向数据库上传字典中的数据
        /// </summary>
        /// <param name="artistTable"></param>
        public static void AddArtist(List<string[]> artistTable)
        {
            try
            {//You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use near 'og:image" content="http://p3.music.126.net/icDuFMTsiAc8sWHZA-c_0w==/109951162829' at line 1
                Console.WriteLine("开始向数据库上传数据");

                while (artistTable.Count != 0)
                {
                    StringBuilder str = new StringBuilder();
                    str.Append("INSERT INTO artist (artistid, artistname, artistpic) VALUES ");
                    for (int i = 0; i < 5; i++)
                    {
                        string[] item = artistTable[0];
                        str.Append("(\"" + item[0] + "\",\"" + item[1] + "\",\"" + item[2] + "\"),");
                        artistTable.Remove(item);
                    }
                    str.Remove(str.Length - 1, 1);
                    str.Append(";");

                    using (MySqlConnection connection = GetConnection())
                    {
                        using (MySqlCommand cmd = new MySqlCommand(str.ToString(), connection))
                        {
                            connection.Open();
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Console.WriteLine("上传完成");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                foreach (var item in artistTable)
                {
                    Console.WriteLine(item[0] + " " + item[1] + " " + item[2]);
                }
            }
        }

        /// <summary>
        /// 开始爬取歌手
        /// </summary>
        /// <param name="args"></param>
        public static void StartCrawl()
        {
            //歌手页面的id链接队列
            Queue<string> artistUrls = new Queue<string>();

            //根据歌手分类id和字母分类id新建数组 group initial
            int[] group = {1001, 1002, 1003, 2001, 2002, 2003, 6001,
                6002, 6003, 7001, 7002, 7003, 4001, 4002, 4003 };
            List<int> initial = new List<int>();
            initial.Add(0);
            for (int i = 65; i <= 90; i++)
            {
                initial.Add(i);
            }
            foreach (int item1 in group)
            {
                //队列里一个板块
                foreach (int item2 in initial)
                {
                    string url = @"http://music.163.com/discover/artist/cat?id=" + item1 + @"&initial=" + item2;
                    artistUrls.Enqueue(url);
                }

                while (artistUrls.Count != 0)//队列不为空时循环
                {
                    //重置字典
                    List<string[]> artistTable = new List<string[]>();
                    GetArtistDetail(artistUrls.Dequeue(), artistTable);
                    AddArtist(artistTable);
                }
            }
        }
    }
}
