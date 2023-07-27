using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace casinoTelegram
{
    internal class Program
    {
        /// <summary>
        /// Определение возможных состояний бота
        /// </summary>
        private enum BotState
        {
            Default, // стандартное значение
            ChooseRange, // выбор диапозона для игры
            Game, // процесс игры до 100
            GameUpTo10, // процесс игры до 10
            MySurvey // процесс заполнения данных о пользователе
        }

        // Текущее состояние бота (по умолчанию - Default)
        private static BotState currentState = BotState.Default;

        // Переменные для игры
        private static int targetNumber; // загаданное число
        private static int maxNumber; // максимальное число

        // Словарь для хранения данных пользователей (айди пользователя и его состояния )
        private static Dictionary<long, UserData> userDataDict = new Dictionary<long, UserData>();

        /// <summary>
        /// Класс для хранения данных пользователя
        /// </summary>
        private class UserData
        {
            public string FirstName; // имя
            public string LastName; // фамилия
            public int Age; // возраст
            public int LuckyNumber; // счастливое число
            public int State; // Поле для отслеживания состояния анкеты
        }

        private static SqlConnection SQLconnection = null;

        static void Main(string[] args)
        {
            SQLconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PointsDB"].ConnectionString); 
            SQLconnection.Open();
            
            if (SQLconnection.State == ConnectionState.Open)
            {
                Console.WriteLine("Подключено");
            }
            Console.ReadKey();

            var client = new TelegramBotClient("6254402236:AAF-lAzwr4E1XjicyVw_Y6ENLNsilvAZwJM"); // создание бота с нашим токеном
            client.StartReceiving(Update, Error); // запуск бота
            Console.WriteLine("Бот запущен. Нажмите любую клавишу, чтобы остановить.");
            Console.ReadKey(); // бот работает, пока не будет нажата любая кнопка в консоле 
        }

        /// <summary>
        /// Основной метод отлова и обработки сообщений
        /// </summary>
        /// <param name="client"></param>
        /// <param name="update"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Обработчик состояния Default
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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
                    await client.SendTextMessageAsync(message.Chat.Id, "Я не понимаю вашей команды. Введите /play для игры или /survey для заполнения анкеты.");
                    break;
            }
        }

        /// <summary>
        /// Обработчик состояния ChooseRange
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Обработчик состояния Game
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async static Task HandleGameState(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int guessedNumber))
            {
                if (guessedNumber == targetNumber)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число! ");
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

        /// <summary>
        /// Обработчик состояния GameUpTo10
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Обработчик состояния MySurvey
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод отлова ошибок
        /// </summary>
        /// <param name="client"></param>
        /// <param name="exception"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
