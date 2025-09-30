using LiteDB;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace MapParser
{
    public class DatabaseManager
    {
        public class LiteDB
        {
            private readonly string _connectionString;

            public LiteDB(string connectionString)
            {
                _connectionString = connectionString;
            }

            public void InitializeDatabase()
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    var bannersHistory = db.GetCollection<BannerDB>("BannersHistory");
                    var playersHistory = db.GetCollection<PlayerDB>("PlayersHistory");
                    var banners = db.GetCollection<BannerDB>("Banners");

                    bannersHistory.EnsureIndex(x => x.Timestamp);
                    playersHistory.EnsureIndex(x => x.Timestamp);
                    banners.EnsureIndex(x => x.Timestamp);
                }
            }

            public void InsertBannerHistory(List<Banner> banners)
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    try
                    {
                        db.BeginTrans();

                        var collection = db.GetCollection<BannerDB>("BannersHistory");

                        foreach (var banner in banners)
                        {
                            var existing = collection.FindOne(Query.And(
                            Query.EQ("Name", banner.Name),
                            Query.EQ("Position[0]", banner.Position[0]),
                            Query.EQ("Position[1]", banner.Position[1]),
                            Query.EQ("Position[2]", banner.Position[2])
                            ));

                            if (existing == null)
                            {
                                collection.Insert(new BannerDB
                                {
                                    Name = banner.Name,
                                    Position = banner.Position,
                                    Dimension = banner.Dimension,
                                    Timestamp = DateTime.Now
                                });
                            }
                        }

                        db.Commit();
                    }
                    catch (Exception)
                    {
                        db.Rollback();
                        throw;
                    }
                }
            }

            public void InsertPlayerHistory(List<Player> players)
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    try
                    {
                        db.BeginTrans();

                        var collection = db.GetCollection<PlayerDB>("PlayersHistory");

                        foreach (var player in players)
                        {
                            var existing = collection.FindOne(Query.And(
                            Query.EQ("Name", player.Name),
                            Query.EQ("Position[0]", player.Position[0]),
                            Query.EQ("Position[1]", player.Position[1]),
                            Query.EQ("Position[2]", player.Position[2]),
                            Query.EQ("Health", player.Health),
                            Query.EQ("Armor", player.Armor)
                            ));

                            if (existing == null)
                            {
                                collection.Insert(new PlayerDB
                                {
                                    Name = player.Name,
                                    KeyUUID = player.KeyUUID,
                                    RotationAngle = player.RotationAngle,
                                    Position = player.Position,
                                    Health = player.Health,
                                    Armor = player.Armor,
                                    Dimension = player.Dimension,
                                    Timestamp = DateTime.Now
                                });
                            }
                        }

                        db.Commit();
                    }
                    catch (Exception)
                    {
                        db.Rollback();
                        throw;
                    }
                }
            }

            public void UpdateCurrentBanners(List<Banner> banners)
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    try
                    {
                        db.BeginTrans();

                        var collection = db.GetCollection<BannerDB>("Banners");
                        var existingBanners = collection.FindAll();

                        // Update existing banners and add new ones
                        foreach (var banner in banners)
                        {
                            var existing = collection.FindOne(Query.And(
                            Query.EQ("Name", banner.Name),
                            Query.EQ("Position[0]", banner.Position[0]),
                            Query.EQ("Position[1]", banner.Position[1]),
                            Query.EQ("Position[2]", banner.Position[2])
                           ));

                            if (existing == null)
                            {
                                collection.Insert(new BannerDB
                                {
                                    Name = banner.Name,
                                    Position = banner.Position,
                                    Dimension = banner.Dimension,
                                    Timestamp = DateTime.Now
                                });
                            }
                        }

                        // Remove banners that are no longer present
                        foreach (var existing in existingBanners)
                        {
                            if (!banners.Exists(b =>
                                b.Name == existing.Name &&
                                b.Position[0] == existing.Position[0] &&
                                b.Position[1] == existing.Position[1] &&
                                b.Position[2] == existing.Position[2]))
                            {
                                collection.Delete(existing.Id);
                            }
                        }

                        db.Commit();
                    }
                    catch (Exception)
                    {
                        db.Rollback();
                        throw;
                    }
                }
            }
        }
        public class SQL
        {
            private readonly string _connectionString;

            public SQL(string connectionString)
            {
                _connectionString = connectionString;
                InitializeDatabase();
            }

            public void InitializeDatabase()
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string bannersHistoryTable = @"CREATE TABLE IF NOT EXISTS BannersHistory (
                                            Id INT AUTO_INCREMENT PRIMARY KEY,
                                            Name VARCHAR(255),
                                            PositionX DOUBLE,
                                            PositionY DOUBLE,
                                            PositionZ DOUBLE,
                                            Dimension VARCHAR(255),
                                            Timestamp DATETIME)";
                    string playersHistoryTable = @"CREATE TABLE IF NOT EXISTS PlayersHistory (
                                            Id INT AUTO_INCREMENT PRIMARY KEY,
                                            Name VARCHAR(255),
                                            KeyUUID VARCHAR(255),
                                            RotationAngle DOUBLE,
                                            PositionX DOUBLE,
                                            PositionY DOUBLE,
                                            PositionZ DOUBLE,
                                            Health DOUBLE,
                                            Armor DOUBLE,
                                            Dimension VARCHAR(255),
                                            Timestamp DATETIME)";
                    string bannersTable = @"CREATE TABLE IF NOT EXISTS Banners (
                                    Id INT AUTO_INCREMENT PRIMARY KEY,
                                    Name VARCHAR(255),
                                    PositionX DOUBLE,
                                    PositionY DOUBLE,
                                    PositionZ DOUBLE,
                                    Dimension VARCHAR(255),
                                    Timestamp DATETIME)";

                    using (var command = new MySqlCommand(bannersHistoryTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (var command = new MySqlCommand(playersHistoryTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (var command = new MySqlCommand(bannersTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            public void InsertPlayerHistory(List<Player> players)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = new MySqlCommand("", connection, transaction);
                        foreach (var player in players)
                        {
                            command.Parameters.Clear();
                            command.CommandText = @"INSERT INTO PlayersHistory (Name, KeyUUID, RotationAngle, PositionX, PositionY, PositionZ, Health, Armor, Dimension, Timestamp) 
                                            VALUES (@Name, @KeyUUID, @RotationAngle, @PositionX, @PositionY, @PositionZ, @Health, @Armor, @Dimension, @Timestamp)";
                            command.Parameters.AddWithValue("@Name", player.Name);
                            command.Parameters.AddWithValue("@KeyUUID", player.KeyUUID);
                            command.Parameters.AddWithValue("@RotationAngle", player.RotationAngle);
                            command.Parameters.AddWithValue("@PositionX", player.Position[0]);
                            command.Parameters.AddWithValue("@PositionY", player.Position[1]);
                            command.Parameters.AddWithValue("@PositionZ", player.Position[2]);
                            command.Parameters.AddWithValue("@Health", player.Health);
                            command.Parameters.AddWithValue("@Armor", player.Armor);
                            command.Parameters.AddWithValue("@Dimension", player.Dimension);
                            command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
            }

            public void InsertBannerHistory(List<Banner> banners)
            {
                int count = 0;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = new MySqlCommand("", connection, transaction);
                        foreach (var banner in banners)
                        {

                            if (banner.Name == null)
                            {
                                command.Parameters.Clear();
                                command.CommandText = @"SELECT COUNT(*) FROM BannersHistory 
                                            WHERE Name IS NULL 
                                            AND PositionX = @PositionX 
                                            AND PositionY = @PositionY 
                                            AND PositionZ = @PositionZ";
                                command.Parameters.AddWithValue("@PositionX", banner.Position[0]);
                                command.Parameters.AddWithValue("@PositionY", banner.Position[1]);
                                command.Parameters.AddWithValue("@PositionZ", banner.Position[2]);
                            }
                            else
                            {
                                command.Parameters.Clear();
                                command.CommandText = @"SELECT COUNT(*) FROM BannersHistory 
                                            WHERE Name = @Name 
                                            AND PositionX = @PositionX 
                                            AND PositionY = @PositionY 
                                            AND PositionZ = @PositionZ";
                                command.Parameters.AddWithValue("@Name", banner.Name);
                                command.Parameters.AddWithValue("@PositionX", banner.Position[0]);
                                command.Parameters.AddWithValue("@PositionY", banner.Position[1]);
                                command.Parameters.AddWithValue("@PositionZ", banner.Position[2]);
                            }

                            count = Convert.ToInt32(command.ExecuteScalar());
                            if (count == 0)
                            {
                                command.Parameters.Clear();
                                command.CommandText = @"INSERT INTO BannersHistory (Name, PositionX, PositionY, PositionZ, Dimension, Timestamp) 
                                            VALUES (@Name, @PositionX, @PositionY, @PositionZ, @Dimension, @Timestamp)";
                                command.Parameters.AddWithValue("@Name", (object)banner.Name ?? DBNull.Value);
                                command.Parameters.AddWithValue("@PositionX", banner.Position[0]);
                                command.Parameters.AddWithValue("@PositionY", banner.Position[1]);
                                command.Parameters.AddWithValue("@PositionZ", banner.Position[2]);
                                command.Parameters.AddWithValue("@Dimension", banner.Dimension);
                                command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }

            public void UpdateCurrentBanners(List<Banner> banners)
            {
                int count = 0;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = new MySqlCommand("", connection, transaction);
                        command.CommandText = "DELETE FROM Banners";
                        command.ExecuteNonQuery();

                        foreach (var banner in banners)
                        {
                            if (banner.Name == null)
                            {
                                command.Parameters.Clear();
                                command.CommandText = @"SELECT COUNT(*) FROM Banners
                                            WHERE Name IS NULL 
                                            AND PositionX = @PositionX 
                                            AND PositionY = @PositionY 
                                            AND PositionZ = @PositionZ";
                                command.Parameters.AddWithValue("@PositionX", banner.Position[0]);
                                command.Parameters.AddWithValue("@PositionY", banner.Position[1]);
                                command.Parameters.AddWithValue("@PositionZ", banner.Position[2]);
                            }
                            else
                            {
                                command.Parameters.Clear();
                                command.CommandText = @"SELECT COUNT(*) FROM Banners
                                            WHERE Name = @Name 
                                            AND PositionX = @PositionX 
                                            AND PositionY = @PositionY 
                                            AND PositionZ = @PositionZ";
                                command.Parameters.AddWithValue("@Name", banner.Name);
                                command.Parameters.AddWithValue("@PositionX", banner.Position[0]);
                                command.Parameters.AddWithValue("@PositionY", banner.Position[1]);
                                command.Parameters.AddWithValue("@PositionZ", banner.Position[2]);
                            }

                            count = Convert.ToInt32(command.ExecuteScalar());
                            if (count == 0)
                            {
                                command.Parameters.Clear();
                                command.CommandText = @"INSERT INTO Banners (Name, PositionX, PositionY, PositionZ, Dimension, Timestamp) 
                                            VALUES (@Name, @PositionX, @PositionY, @PositionZ, @Dimension, @Timestamp)";
                                command.Parameters.AddWithValue("@Name", (object)banner.Name ?? DBNull.Value);
                                command.Parameters.AddWithValue("@PositionX", banner.Position[0]);
                                command.Parameters.AddWithValue("@PositionY", banner.Position[1]);
                                command.Parameters.AddWithValue("@PositionZ", banner.Position[2]);
                                command.Parameters.AddWithValue("@Dimension", banner.Dimension);
                                command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }

        }
    }
    public class BannerDB
    {
        [BsonId]
        public int Id { get; set; }
        public float[] Position { get; set; }
        public string Name { get; set; }
        public string Dimension { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PlayerDB
    {
        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }
        public string KeyUUID { get; set; }
        public float RotationAngle { get; set; }
        public float[] Position { get; set; }
        public float Health { get; set; }
        public float Armor { get; set; }
        public string Dimension { get; set; }
        public DateTime Timestamp { get; set; }
    }
}