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
            GameTo100, // процесс игры до 100
            GameUpTo10, // процесс игры до 10
        }

        // Текущее состояние бота (по умолчанию - Default)
        private static BotState currentState = BotState.Default;

        // Переменные для игры
        private static int targetNumber; // загаданное число
        private static int maxNumber; // максимальное число

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
                    case BotState.GameTo100:
                        await HandleGameTo100State(client, message);
                        break;
                    case BotState.GameUpTo10:
                        await HandleGameUpTo10State(client, message);
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
                    await client.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в наше казино! Введите /play, чтобы начать игру или /points для того, чтобы узнать своё количество очков.");
                    break;
                case "/play":
                    await client.SendTextMessageAsync(message.Chat.Id, "Выберите диапазон чисел:\n1. От 1 до 10\n2. От 1 до 100");
                    currentState = BotState.ChooseRange;
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
                    currentState = BotState.GameTo100;
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
        async static Task HandleGameTo100State(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int guessedNumber))
            {
                if (guessedNumber == targetNumber)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число! ");

                    long chatId = message.Chat.Id;
                    UpdatePointsInDB(chatId, 1);

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

                    long chatId = message.Chat.Id;
                    UpdatePointsInDB(chatId, 1);

                    currentState = BotState.Default;
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Увы, вы не угадали. Загаданное число было - {targetNumber}. Попробуйте еще раз!");
                    targetNumber = new Random().Next(1, maxNumber + 1);
                }
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число.");
            }
        }

        /// <summary>
        /// Пополнение очков пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="points"></param>
        private static void UpdatePointsInDB(long chatId, int points)
        {
            string query = $"IF EXISTS (SELECT * FROM [Points] WHERE [UserID] = '{chatId}') " +
                           $"UPDATE [Points] SET [Points] = [Points] + {points} WHERE [UserID] = '{chatId}' " +
                           $"ELSE INSERT INTO [Points] ([UserID], [Points]) VALUES ('{chatId}', {points})";

            using (SqlCommand command = new SqlCommand(query, SQLconnection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Получение баллов пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        private static int GetPointsFromDB(long chatId)
        {
            int points = 0;
            string query = $"SELECT [Points] FROM [Points] WHERE [UserID] = '{chatId}'";

            using (SqlCommand command = new SqlCommand(query, SQLconnection))
            using (SqlDataReader reader = command.ExecuteReader())
            {   
                if (reader.Read())
                {
                    points = reader.GetInt32(0);
                }
            }

            return points;
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
