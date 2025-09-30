using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace MapParser
{
    internal class Program
    {
        private static string domain = "https://map.onimai.ru";
        private static Stopwatch sw = new Stopwatch();

        private static string _databasePath = @"onimai.db";
        private static bool dbType = false;

        private static List<string> worlds = new List<string>() { "world", "world_nether", "world_the_end" };
        private static List<Player> players;
        private static List<Banner> banners;

        private static void Main(string[] args)
        {
            Console.BufferHeight = 50;

            DatabaseManager.SQL _dbManagerSQL = null;
            DatabaseManager.LiteDB _dbManagerLiteDB = null;

            if (File.Exists("SQLDB"))
            {

                _databasePath = "server=192.168.0.100;uid=root;pwd=passwd;database=onimai";
                _dbManagerSQL = new DatabaseManager.SQL(_databasePath);
                _dbManagerSQL.InitializeDatabase();
                dbType = true;
            }
            else
            {
                _dbManagerLiteDB = new DatabaseManager.LiteDB(_databasePath);
                _dbManagerLiteDB.InitializeDatabase();
                dbType = false;
            }

            while (true)
            {
                try
                {
                    sw.Restart();

                    ;


                    Task[] tasks = new Task[]
                    {
                            Task.Run(() => { players = getPlayers();}),
                            Task.Run(() => { banners = getBanners();})
                    };
                    Task.WaitAll(tasks);

                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds + " Get JSON from Site");
                    sw.Restart();
                    if (dbType == true)
                    {
                        tasks = new Task[]
                        {
                            Task.Run(() => { _dbManagerSQL.InsertBannerHistory(banners); }),
                            Task.Run(() => { _dbManagerSQL.InsertPlayerHistory(players); }),
                            Task.Run(() => { _dbManagerSQL.UpdateCurrentBanners(banners); })
                        };
                        Task.WaitAll(tasks);
                    }
                    else
                    {
                        _dbManagerLiteDB.InsertBannerHistory(banners);
                        _dbManagerLiteDB.InsertPlayerHistory(players);
                        _dbManagerLiteDB.UpdateCurrentBanners(banners);
                    }

                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds + " Database Operation");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
        }
        private static void LogError(Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            File.AppendAllText($"error-{DateTime.Now.ToShortDateString()}.log", $"{DateTime.Now}: {ex}\n");
        }

        private static List<Banner> getBanners()
        {
            List<Banner> banners = new List<Banner>();

            Parallel.ForEach(worlds, (world) =>
            {
                string jsonRaw = WebClientUtility.Get($"{domain}/tiles/{world}/markers/pl3xmap_banners.json");
                List<Banner> bannerInfo = BannerUtility.GetBannersInfo(jsonRaw, world);
                if (bannerInfo == null) return;
                lock (banners)
                {
                    banners.AddRange(bannerInfo);
                }
            });

            return banners;
        }

        private static List<Player> getPlayers()
        {
            List<Player> players = new List<Player>();

            Parallel.ForEach(worlds, (world) =>
            {
                string jsonRaw = WebClientUtility.Get($"{domain}/tiles/{world}/markers/pl3xmap_players.json");
                List<Player> playerInfo = PlayerUtility.GetPlayersInfo(jsonRaw, world);
                lock (players)
                {
                    players.AddRange(playerInfo);
                }
            });

            return players;
        }
    }
}
    
