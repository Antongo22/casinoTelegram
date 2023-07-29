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
        // Определение возможных состояний бота
        private enum BotState
        {
            Default, // стандартное значение
            ChooseRange, // выбор диапазона для игры
            GameTo100, // процесс игры до 100
            GameUpTo10, // процесс игры до 10
        }

        // Текущее состояние бота (по умолчанию - Default)
        private static BotState defaultState = BotState.Default;
        private static Dictionary<long, BotState> userStates = new Dictionary<long, BotState>();

        // Переменные для игры
        private static int targetNumber; // загаданное число
        private static int maxNumber; // максимальное число

        private static SqlConnection SQLconnection = null; // Ссылка на БД с очками

        static void Main(string[] args)
        {
            SQLconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PointsDB"].ConnectionString); // Подключение к базе
            SQLconnection.Open(); // ОТкрытие для программмы базу

            // Проверка подключения
            if (SQLconnection.State == ConnectionState.Open)
            {
                Console.WriteLine("База данных \"PointsDB\" подключена!");
            }

            var client = new TelegramBotClient("6254402236:AAF-lAzwr4E1XjicyVw_Y6ENLNsilvAZwJM"); // создание бота с нашим токеном
            client.StartReceiving(Update, Error); // запуск бота
            Console.WriteLine("Бот запущен. Нажмите любую клавишу, чтобы остановить.");
            Console.ReadKey(); // бот работает, пока не будет нажата любая кнопка в консоле 
        }

        async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;

            Console.WriteLine($"{message.Chat.FirstName ?? "-no name-"}\t\t|\t{message.Text ?? "-no text-"}");

            if (message.Text != null)
            {
                // Получаем текущее состояние пользователя из словаря или устанавливаем состояние по умолчанию
                var currentState = userStates.TryGetValue(message.Chat.Id, out var state) ? state : defaultState;

                switch (currentState)
                {
                    case BotState.Default:
                        await HandleDefaultState(client, message);
                        break;
                    case BotState.ChooseRange:
                        await HandleChooseRangeState(client, message);
                        break;
                    case BotState.GameTo100:
                        await HandleGameTo100State(client, message);
                        break;
                    case BotState.GameUpTo10:
                        await HandleGameUpTo10State(client, message);
                        break;
                }
            }
        }

        async static Task HandleDefaultState(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    await client.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в наше казино! Введите /play, чтобы начать игру или /points для того, чтобы узнать своё количество очков.");
                    break;
                case "/play":
                    await client.SendTextMessageAsync(message.Chat.Id, "Выберите диапазон чисел:\n1. От 1 до 10\n2. От 1 до 100");
                    userStates[message.Chat.Id] = BotState.ChooseRange;
                    break;
                case "/points":
                    long chatId = message.Chat.Id;
                    int points = GetPointsFromDB(chatId);
                    await client.SendTextMessageAsync(chatId, $"У вас {points} балл(ов).");
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Я не понимаю вашей команды. Введите /play для игры или /points для того, чтобы узнать своё количество очков.");
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
                    userStates[message.Chat.Id] = BotState.GameUpTo10;
                    break;
                case "2":
                    maxNumber = 100;
                    targetNumber = new Random().Next(1, maxNumber + 1);
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выбрали диапазон от 1 до {maxNumber}. Давайте начнем игру! Отгадайте число от 1 до {maxNumber}. Введите число:");
                    userStates[message.Chat.Id] = BotState.GameTo100;
                    break;
                case "/cancel":
                    await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                    userStates[message.Chat.Id] = BotState.Default;
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1 или 2 для выбора диапазона.");
                    break;
            }
        }

        async static Task HandleGameTo100State(ITelegramBotClient client, Message message)
        {
            // ... (остальной код без изменений)
        }

        async static Task HandleGameUpTo10State(ITelegramBotClient client, Message message)
        {
            // ... (остальной код без изменений)
        }

        // ... (остальной код без изменений)

        private static void SetUserState(long chatId, BotState state)
        {
            userStates[chatId] = state;
        }
    }
}
