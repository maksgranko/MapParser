using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MapParser
{
    internal class BannerUtility
    {
        public static List<Banner> GetBannersInfo(string jsonString, string dimension = "world")
        {
            if (string.IsNullOrEmpty(jsonString)) {  return null; }
                var banners = new List<Banner>(); 
            var jsonArray = JArray.Parse(jsonString);

            foreach (var item in jsonArray)
            {
                var type = item["type"].ToString();
                if (type == "icon")
                {
                    var data = item["data"];
                    var point = data["point"];
                    var position = new float[]
                    {
                    (float)point["x"],
                    0, // As there's no y in JSON, setting it to 0
                    (float)point["z"]
                    };

                    string name = null;
                    var options = item["options"];
                    if (options != null)
                    {
                        var tooltip = options["tooltip"];
                        if (tooltip != null)
                        {
                            var content = tooltip["content"].ToString();
                            name = System.Net.WebUtility.HtmlDecode(content);
                            name = name.Replace("<center>", "").Replace("</center>", "");
                        }
                    }

                    banners.Add(new Banner
                    {
                        Dimension = dimension,
                        Position = position,
                        Name = name
                    });
                }
            }

            return banners;
        }
    }
}
