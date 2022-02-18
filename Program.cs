using System;
using System.Linq;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace ListSmurfsR6TelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start!");
            MyBot.Bot.SetMyCommands(new BotCommand("add", "добавить смурфа в список"),
                new BotCommand("showlist", "показать список смурфов"), new BotCommand("delete", "удалить смурфа"),
                new BotCommand("check", "тест на смурфа"));
            MyBot.Bot.DeleteWebhook();
            // Long Polling: Start
            var updates = MyBot.Bot.GetUpdates();
            while (true)
            {
                if (updates.Any())
                {
                    foreach (var update in updates)
                    {
                        var botInstance = new MyBot();
                        botInstance.OnUpdate(update);
                    }
                    var offset = updates.Last().UpdateId + 1;
                    updates = MyBot.Bot.GetUpdates(offset);
                }
                else
                {
                    updates = MyBot.Bot.GetUpdates();
                }
            }
        }
    }
}
