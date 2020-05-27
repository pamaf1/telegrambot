using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using System.Threading;

namespace ConsoleApp22
{
    class Program
    {
        private static TelegramBotClient client;
        public static List<string> Messages = new List<string>();
        private static string response = null;
        private static string response1 = null;
        private static float response3;
        private static float response2;
        private static string response4 = null;
        static void Main(string[] args)
        {
           
            client = new TelegramBotClient("1158311086:AAHdEOGVZWpa6fIIkV18v6zq1SBWIxhob-s");
     
            client.OnMessage += Bot_OnMessage;

            client.StartReceiving();
            Console.ReadLine();
            client.StopReceiving();
        }
        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            WebClient wc = new WebClient();
            response1 = wc.DownloadString("https://webapplication120200527122608.azurewebsites.net/news");
            string newsResponse = "{\"response\":" + response1 + "}";

            JObject result = JObject.Parse(newsResponse);
            IList<JToken> results = result["response"].Children().ToList();

            var message = e.Message.Text;

            switch (message)
            {
                case "/start":
                    await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: "Добрий день! Ви розпачали роботу з Mood assistant!\nCписок доступних команд:\n/weathers - покаже погоду в Києві зараз\n/news - покаже актуальні новини за сьогодні\n/searchnews - почати пошук новин\n/music - почати пошук музики\n----------------------------------------------------------\nЩоб скористатися командами, достатньо натиснути на назви команд в цьому повідомлені. Надалі ви також зможете натискати на назви команд, які виділені синім кольором, щоб взаємодіяти з ботом.");
                    break;
                case "/weathers":
                  GetWeather();     
               await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: "Температура в Києві: " + response2 + " °C" + " \nВідчувається як: " + response3 + " °C" + " \n" + response4);                                   
                    break;

                case "/news":
                    ShowNews(e, results);
                    break;

                case "/searchnews":
                    {
                      await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: "Введіть заголовок новини, яку Ви хочете знайти:");
                        Messages.Add(message);
                        client.OnMessage += Bot_OnMessage;
                        static async void Bot_OnMessage(object sender, MessageEventArgs e)
                        {
                            if (Messages.Contains("/searchnews")) 
                            {
                                if (e.Message.Text != "/weathers" && e.Message.Text != "/news" && e.Message.Text != "/searchnews" && e.Message.Text != "/music")
                                {
                                    SearchNews(e, e.Message.Text);
                                }
                                Messages.Clear();
                            }
                        
                        }                                             
                        break;
                    }                               
                case "/music":
                    {                       
                       await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: "Введіть назву пісні або виконавця:");
                        Messages.Add(message);
                        client.OnMessage += Bot_OnMessage;
                       static void Bot_OnMessage(object sender, MessageEventArgs e)
                       {                        
                            if (Messages.Contains("/music"))
                            {
                                if (e.Message.Text != "/weathers" && e.Message.Text != "/news" && e.Message.Text != "/searchnews" && e.Message.Text != "/music")
                                {
                                    SearchMusic(e, e.Message.Text);
                                }                               
                                Messages.Clear();
                               
                            }
                       }
                        break;
                    }
                case "/help":

                    break;

            }
           

        }
        public static string GetStr(string A)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(A);

            request.Method = "GET";

            WebResponse response = request.GetResponse();

            Stream s = response.GetResponseStream();

            StreamReader reader = new StreamReader(s);

            string answer = reader.ReadToEnd();

            response.Close();

            return answer;
        }
       
        private static void GetWeather()
        {
            using (var webClient = new WebClient())
            {
                response = webClient.DownloadString("https://webapplication120200527122608.azurewebsites.net/weather");
            }
            string weatherResponse = "{\"response\":"+response+"}";
            JObject obj = JObject.Parse(weatherResponse);
            IList<JToken> results = obj["response"].Children().ToList();
            foreach (JToken result in results)
            {
                TemperatureInfo ns = result.ToObject<TemperatureInfo>();
                response2 = ns.temp;
                response3 = ns.feels_Like;
                response4 = ns.description;
            }          
        }

        private async static void ShowNews(MessageEventArgs e, IList<JToken> list)
        {                     
            foreach (JToken result in list)
            {
                NewsResponse nr = result.ToObject<NewsResponse>();               
                await client.SendTextMessageAsync(chatId: e.Message.Chat, text: nr.title + "\n" + nr.url);                
                
            }

        }
      
        private async static void SearchMusic(MessageEventArgs e, string search)
        {
            WebClient wc = new WebClient();
            string res = wc.DownloadString($"https://webapplication120200527122608.azurewebsites.net/music?title={search}");
            if(res == "[]")
            {
                await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, "Нічого не знайдено, введіть команду /music знову, щоб знайти пісню");
            }
            string musicSearch = "{\"response\":" + res + "}";

            JObject result = JObject.Parse(musicSearch);
            IList<JToken> results = result["response"].Children().ToList();

            foreach (JToken result2 in results)
            {
                MusicResponse mr = result2.ToObject<MusicResponse>();
                await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: mr.name + "\n" + mr.url);
                
            }
            if (res != "[]")
                await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, "Якщо ви знову хочете знайти якусь пісню, введіть команду /music знову");

        }
        private async static void SearchNews(MessageEventArgs e, string search)
        {
            string news = GetStr($"https://webapplication120200527122608.azurewebsites.net/newssearch?title={e.Message.Text}");

            if (news != "")
            {
                await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, news);
                await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, "Якщо ви знову хочете знайти якусь новину, введіть команду /searchnews знову");
            }
           else { await client.SendTextMessageAsync(chatId: e.Message.Chat.Id, "Нічого не знайдено, введіть команду /searchnews знову, щоб знайти іншу новину");  }

        }    
  
    }
    class TemperatureInfo
    {
        public float temp { get; set; }
        public float feels_Like { get; set; }
        public string description { get; set; }
    }
    class NewsResponse
    {
        public string title { get; set; }
        public string url { get; set; }

    }
    class MusicResponse
    {
        public string name { get; set; }
        public string url { get; set; }
    }
}
