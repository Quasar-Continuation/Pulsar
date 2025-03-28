using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Server.TelegramSender
{
    internal class Send
    {
        public static async Task<string> SendConnectionMessage(string botToken, string chatId, string clientName, string ipAddress, string country)
        {
            try
            {
                string message = $"✨ *New Client Connected!* ✨\n" +
                               $"👤 Name: {clientName}\n" +
                               $"🌐 IP: {ipAddress}\n" +
                               $"🌎 Country: {country}";

                string url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(message)}";
                Console.WriteLine($"Request URL: {url}");

                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 10000;

                using (WebResponse response = await request.GetResponseAsync())
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    if (httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"Telegram API returned status: {httpResponse.StatusCode}");
                    }

                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string responseText = await reader.ReadToEndAsync();
                        Console.WriteLine($"Response: {responseText}");
                        return responseText;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
                    using (Stream stream = errorResponse.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string errorText = await reader.ReadToEndAsync();
                        Console.WriteLine($"Error Response: {errorText}");
                        throw new Exception($"Telegram API error: {errorResponse.StatusCode} - {errorText}");
                    }
                }
                throw new Exception($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send Telegram message: {ex.Message}");
                throw;
            }
        }
    }
}