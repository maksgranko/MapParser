using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

public static class WebClientUtility
{
    public static void UploadFile(string filePath, string uploadUrl)
    {
        WebClient client = new WebClient();
        try
        {
            byte[] responseArray = client.UploadFile(uploadUrl, filePath);
            Console.WriteLine("\nResponse Received. The contents of the file uploaded are:\n{0}",
                System.Text.Encoding.ASCII.GetString(responseArray));
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    public static void DownloadFile(string downloadUrl, string destinationPath)
    {
        WebClient client = new WebClient();
        try
        {
            client.DownloadFile(downloadUrl, destinationPath);
            Console.WriteLine("File downloaded successfully to " + destinationPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    public static string Get(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";

        try
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return null;
        }
    }

    public static string Post(string url, string data)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        request.ContentLength = byteArray.Length;

        try
        {
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return null;
        }
    }
    public static string EncodeToJson(object obj)
    {
        try
        {
            return JsonConvert.SerializeObject(obj);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during JSON encoding: " + ex.Message);
            return null;
        }
    }

    public static T DecodeFromJson<T>(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during JSON decoding: " + ex.Message);
            return default(T);
        }
    }
}
