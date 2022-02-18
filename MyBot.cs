using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace ListSmurfsR6TelegramBot
{
    public sealed class MyBot : TelegramBotBase
    {
        public static BotClient Bot = new BotClient(GetBotToken());
        public static User Me = Bot.GetMe();

        private Message message;
        private bool hasText;
        private User appUser;

        public override void OnUpdate(Update update)
        {
            Console.WriteLine("New update with id: {0}. Type: {1}", update?.UpdateId, update?.Type.ToString("F"));
            base.OnUpdate(update);
        }

        protected override void OnMessage(Message message)
        {
            // Ignore user 777000 (Telegram)
            if (message?.From.Id == TelegramConstants.TelegramId)
            {
                return;
            }
            Console.WriteLine("New message from chat id: {0}", message.Chat.Id);

            appUser = message.From; // Save current user;
            this.message = message; // Save current message;
            hasText = !string.IsNullOrEmpty(message.Text); // True if message has text;

            Console.WriteLine("Message Text: {0}", hasText ? message.Text : "|:O");

            if (message.Chat.Type == ChatType.Private) // Private Chats
            {
            }
            else // Group chats
            {

            }
            if (hasText)
            {
                if (message.Text.StartsWith('/')) // New commands
                {
                    // If the command includes a mention, you should verify that it is for your bot, otherwise you will need to ignore the command.
                    var pattern = string.Format(@"^\/(?<COMMAND>\w*)(?:|@{0})(?:$|\s(?<PARAMETERS>.*))", Me.Username);
                    var match = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var command = match.Groups.Values.Single(v => v.Name == "COMMAND").Value; // Get command name
                        var @params = match.Groups.Values.SingleOrDefault(v => v.Name == "PARAMETERS")?.Value; // Get command params
                        var parameters = @params?.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();

                        Console.WriteLine("New command: {0}", command);
                        OnCommand(command, parameters);
                    }
                }


            }
            if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Введите имя смурфа:"))
            {
                string nickname = message.Text;
                if (IsCheckSmurfDB(nickname))
                {
                    Bot.SendMessage(message.Chat.Id, "Смурф с таким именем уже существует");
                }
                else
                {
                    if (IsValidationSmurf(nickname))
                    {
                        SaveSmurfDB(nickname);
                        Bot.SendMessage(message.Chat.Id, $"{nickname} добавлен в список");
                    }
                    else Bot.SendMessage(message.Chat.Id, "Недопустимое имя");
                }
            }
            if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Удалить смурфа:"))
            {
                string nickname = message.Text;
                if (IsCheckSmurfDB(nickname))
                {
                    DeleteSmurfDB(nickname);
                    Bot.SendMessage(message.Chat.Id, $"{nickname} удален из списка");
                }
                else
                {
                    Bot.SendMessage(message.Chat.Id, $"{nickname} не найден в списке");
                }

            }
            if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Проверить смурфа:"))
            {
                string nickname = message.Text;
                if (IsCheckSmurfDB(nickname))
                {
                    Bot.SendMessage(message.Chat.Id, "Такой смурф уже существует");
                }
                else
                {
                    Bot.SendMessage(message.Chat.Id, "Такого смурфа нет в списке");
                }
            }
        }

        private void OnCommand(string cmd, string[] args)
        {
            Console.WriteLine("Params: {0}", args.Length);
            switch (cmd)
            {
                case "add":
                    AddSmurf();
                    break;
                case "showlist":
                    DisplayList();
                    break;
                case "delete":
                    DeleteSmurf();
                    break;
                case "check":
                    CheckSmurf();
                    break;
            }
        }
        private void AddSmurf()
        {
            var hello = string.Format("Привет {0}! Введите имя смурфа:", appUser.FirstName);
            Bot.SendMessage(message.Chat.Id, hello, replyMarkup: new ForceReply { Selective = true });
        }
        private void DeleteSmurf()
        {
            var hello = string.Format("Удалить смурфа:");
            Bot.SendMessage(message.Chat.Id, hello, replyMarkup: new ForceReply { Selective = true });
        }
        private void CheckSmurf()
        {
            var hello = string.Format("Проверить смурфа:");
            Bot.SendMessage(message.Chat.Id, hello, replyMarkup: new ForceReply { Selective = true });
        }
        private static void SaveSmurfDB(string nickname)
        {
            using var connection = new SqliteConnection("Data Source=smurfsdata.db");
            connection.Open();
            string sqlExpression = $"INSERT INTO Smurfs (Nickname) VALUES ('{nickname}')";
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            command.ExecuteNonQuery();
        }
        private void DisplayList()
        {
            var list = new StringBuilder();
            using var connection = new SqliteConnection("Data Source=smurfsdata.db");
            connection.Open();
            string sqlExpression = "SELECT * FROM Smurfs LIMIT 20";
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            using SqliteDataReader reader = command.ExecuteReader();
            if (reader.HasRows) // если есть данные
            {
                while (reader.Read())   // построчно считываем данные
                {
                    list.Append($"{reader.GetValue(1)}\n");
                }
                Bot.SendMessage(message.Chat.Id, list.ToString());
            }
            else
            {
                Bot.SendMessage(message.Chat.Id, "Список пуст");
            }
        }
        private static void DeleteSmurfDB(string nickname)
        {
            using var connection = new SqliteConnection("Data Source=smurfsdata.db");
            connection.Open();
            string sqlExpression = $"DELETE FROM Smurfs WHERE Nickname = '{nickname}'";
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            command.ExecuteNonQuery();
        }
        private static bool IsCheckSmurfDB(string nickname)
        {
            using var connection = new SqliteConnection("Data Source=smurfsdata.db");
            connection.Open();
            string sqlExpression = $"SELECT * FROM Smurfs WHERE Nickname = '{nickname}'";
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            using SqliteDataReader reader = command.ExecuteReader();
            if (reader.HasRows) // если есть данные
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static bool IsValidationSmurf(string nickname)
        {
            if (nickname != null && nickname.Length > 1 && nickname.Length <= 20)
            {
                return true;
            }
            else return false;
        }
        private static string GetBotToken()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("config.xml");
            XmlElement xRoot = xDoc.DocumentElement;
            XmlNode attr = xRoot.Attributes.GetNamedItem("token");
            return attr.Value;
        }

        protected override void OnBotException(BotRequestException exp)
        {
            Console.WriteLine("New BotException: {0}", exp?.Message);
            Console.WriteLine("Error Code: {0}", exp.ErrorCode);
            Console.WriteLine();
        }

        protected override void OnException(Exception exp)
        {
            Console.WriteLine("New Exception: {0}", exp?.Message);
            Console.WriteLine();
        }
    }

}

