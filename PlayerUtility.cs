using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace MapParser
{
    internal class PlayerUtility
    {
        public static List<Player> GetPlayersInfo(string json, string dimension = "world")
        {
            List<Player> players = new List<Player>();

            try
            {
                JArray jsonArray = JArray.Parse(json);

                foreach (JObject item in jsonArray)
                {
                    if (item["data"] == null || item["options"] == null || item["options"]["tooltip"] == null)
                    {
                        continue; // Skip if required fields are missing
                    }

                    Player player = new Player
                    {
                        KeyUUID = item["data"]["key"]?.ToString(),
                        RotationAngle = item["data"]["rotationAngle"] != null ? (float)item["data"]["rotationAngle"] : 0,
                        Position = new float[]
                        {
                        item["data"]["point"]?["x"] != null ? (float)item["data"]["point"]["x"] : 0,
                        0, // Assuming y-coordinate as 0, since it's not provided in the JSON
                        item["data"]["point"]?["z"] != null ? (float)item["data"]["point"]["z"] : 0
                        }
                    };

                    string tooltipContent = item["options"]["tooltip"]["content"]?.ToString();
                    if (!string.IsNullOrEmpty(tooltipContent))
                    {
                        player.Name = ExtractNameFromTooltip(tooltipContent);
                        player.Health = ExtractHealthFromTooltip(tooltipContent);
                        player.Armor = ExtractArmorFromTooltip(tooltipContent);
                    }
                    player.Dimension = dimension;
                    players.Add(player);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return players;
        }

        private static string ExtractNameFromTooltip(string tooltipContent)
        {
            try
            {
                Regex regex = new Regex(@"alt='(.*?)'", RegexOptions.Singleline);
                Match match = regex.Match(tooltipContent);
                return match.Success ? match.Groups[1].Value : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while extracting name: " + ex.Message);
                return string.Empty;
            }
        }

        private static float ExtractHealthFromTooltip(string tooltipContent)
        {
            try
            {
                string healthString = "Health ";
                int startIndex = tooltipContent.IndexOf(healthString) + healthString.Length;
                int endIndex = tooltipContent.IndexOf('\'', startIndex);
                return float.Parse(tooltipContent.Substring(startIndex, endIndex - startIndex).Trim()) / 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while extracting health: " + ex.Message);
                return 0;
            }
        }

        private static float ExtractArmorFromTooltip(string tooltipContent)
        {
            try
            {
                string armorString = "Armor ";
                int startIndex = tooltipContent.IndexOf(armorString) + armorString.Length;
                int endIndex = tooltipContent.IndexOf('\'', startIndex);
                return float.Parse(tooltipContent.Substring(startIndex, endIndex - startIndex).Trim()) / 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while extracting armor: " + ex.Message);
                return 0;
            }
        }
    }
}
