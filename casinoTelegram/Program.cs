using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace casinoTelegram
{
    internal class Program
    {
        private enum BotState
        {
            Default,
            ChooseRange,
            Game,
            GameUpTo10,
            MySurvey
        }

        private static BotState currentState = BotState.Default;
        private static int targetNumber;
        private static int maxNumber;
        private static Dictionary<long, UserData> userDataDict = new Dictionary<long, UserData>();

        private class UserData
        {
            public string FirstName;
            public string LastName;
            public int Age;
            public int LuckyNumber;
            public int State; // Добавлено поле State
        }

        static void Main(string[] args)
        {
            var client = new TelegramBotClient("6254402236:AAF-lAzwr4E1XjicyVw_Y6ENLNsilvAZwJM");
            client.StartReceiving(Update, Error);
            Console.WriteLine("Бот запущен. Нажмите любую клавишу, чтобы остановить.");
            Console.ReadKey();
 
        }

        async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;

            Console.WriteLine($"{message.Chat.FirstName ?? "-no name-"}\t\t|\t{message.Text ?? "-no text-"}");

            if (message.Text != null)
            {
                switch (currentState)
                {
                    case BotState.Default:
                        await HandleDefaultState(client, message);
                        break;
                    case BotState.ChooseRange:
                        await HandleChooseRangeState(client, message);
                        break;
                    case BotState.Game:
                        await HandleGameState(client, message);
                        break;
                    case BotState.GameUpTo10:
                        await HandleGameUpTo10State(client, message);
                        break;
                    case BotState.MySurvey:
                        await HandleMySurveyState(client, message);
                        break;
                }
            }
        }

        async static Task HandleDefaultState(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    await client.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в наше казино! Введите /play, чтобы начать игру, или /survey, чтобы заполнить анкету.");
                    break;
                case "/play":
                    await client.SendTextMessageAsync(message.Chat.Id, "Выберите диапазон чисел:\n1. От 1 до 10\n2. От 1 до 100");
                    currentState = BotState.ChooseRange;
                    break;
                case "/survey":
                    await client.SendTextMessageAsync(message.Chat.Id, "Давайте заполним вашу анкету. Введите ваше имя:");
                    currentState = BotState.MySurvey;
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Я не понимаю вашей команды. Введите /start для начала, /play для игры, или /survey для заполнения анкеты.");
                    break;
            }
        }

        async static Task HandleChooseRangeState(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "1":
                    maxNumber = 10;
                    targetNumber = new Random().Next(1, maxNumber + 1);
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выбрали диапазон от 1 до {maxNumber}. Давайте начнем игру! Отгадайте число от 1 до {maxNumber}. Введите число:");
                    currentState = BotState.GameUpTo10;
                    break;
                case "2":
                    maxNumber = 100;
                    targetNumber = new Random().Next(1, maxNumber + 1);
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выбрали диапазон от 1 до {maxNumber}. Давайте начнем игру! Отгадайте число от 1 до {maxNumber}. Введите число:");
                    currentState = BotState.Game;
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1 или 2 для выбора диапазона.");
                    break;
            }
        }

        async static Task HandleGameState(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int guessedNumber))
            {
                if (guessedNumber == targetNumber)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число!");
                    currentState = BotState.Default;
                }
                else if (guessedNumber < targetNumber)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Нет, загаданное число больше {guessedNumber}. Попробуйте еще раз! Введите число:");
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Нет, загаданное число меньше {guessedNumber}. Попробуйте еще раз! Введите число:");
                }
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число.");
            }
        }

        async static Task HandleGameUpTo10State(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int guessedNumber))
            {
                if (guessedNumber == targetNumber)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число!");
                    currentState = BotState.Default;
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Увы, вы не угадали. Попробуйте еще раз! Введите число:");
                }
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число.");
            }
        }

        async static Task HandleMySurveyState(ITelegramBotClient client, Message message)
        {
            long chatId = message.Chat.Id;
            if (!userDataDict.ContainsKey(chatId))
            {
                userDataDict[chatId] = new UserData();
                userDataDict[chatId].State = 0;
            }

            switch (userDataDict[chatId].State)
            {
                case 0:
                    userDataDict[chatId].FirstName = message.Text;
                    await client.SendTextMessageAsync(message.Chat.Id, "Отлично! Теперь введите вашу фамилию:");
                    userDataDict[chatId].State = 1;
                    break;
                case 1:
                    userDataDict[chatId].LastName = message.Text;
                    await client.SendTextMessageAsync(message.Chat.Id, "Отлично! Теперь введите ваш возраст:");
                    userDataDict[chatId].State = 2;
                    break;
                case 2:
                    if (int.TryParse(message.Text, out int age))
                    {
                        userDataDict[chatId].Age = age;
                        userDataDict[chatId].LuckyNumber = new Random().Next(1, 101);
                        await client.SendTextMessageAsync(message.Chat.Id, "Отлично! Ваши данные сохранены.");
                        await client.SendTextMessageAsync(message.Chat.Id, $"Ваше имя: {userDataDict[chatId].FirstName}\nФамилия: {userDataDict[chatId].LastName}\nВозраст: {userDataDict[chatId].Age}\nВаше счастливое число: {userDataDict[chatId].LuckyNumber}");
                        currentState = BotState.Default;
                    }
                    else
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите ваш возраст числом.");
                    }
                    break;
            }
        }

        private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
