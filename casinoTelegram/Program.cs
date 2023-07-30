﻿using System;
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
        /// Задаём максимальное значение в каждому пользователю для игры
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="maxNumber"></param>
        private static void SetMaxNumber(long chatID, int maxNumber) => Data.userStates[chatID].maxNumber = maxNumber;
        
        /// <summary>
        /// Получаем максимальное значение в каждому пользователю для игры
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        private static int GetMaxNumber(long chatID) => Data.userStates[chatID].maxNumber;
        
        /// <summary>
        /// Получаем загаданное число 
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        private static int GetTargetNumber(long chatID) => Data.userStates[chatID].targetNumber; 

        /// <summary>
        /// Задаём загаданное число 
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="targetNumber"></param>
        private static void SetTardetNumber(long chatID, int maxNumber) => Data.userStates[chatID].targetNumber = new Random().Next(1, maxNumber + 1);
        
        static void Main(string[] args)
        {
            Data.SQLconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PointsDB"].ConnectionString); // Подключение к базе
            Data.SQLconnection.Open(); // ОТкрытие для программмы базу
            
            // Проверка подключения
            if (Data.SQLconnection.State == ConnectionState.Open)
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
                switch (State.GetBotState(message.Chat.Id))
                {
                    case State.BotState.Default:
                        await HandleDefaultState(client, message);
                        break;
                    case State.BotState.ChooseRange:
                        await HandleChooseRangeState(client, message);
                        break;
                    case State.BotState.GameTo100:
                        await HandleGameTo100State(client, message);
                        break;
                    case State.BotState.GameUpTo10:
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
                    await client.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в наше казино! Введите /play, чтобы начать игру или /points для того, чтобы узнать своё количество очков. Также, для отмены действия введите /cancel. Если возникнут проблемы, можете прописать /help");
                    break;
                case "/help" :
                    await client.SendTextMessageAsync(message.Chat.Id, "Введите /play, чтобы начать игру или /points для того, чтобы узнать своё количество очков. Также, для отмены действия введите /cancel.");
                    State.SetBotState(message.Chat.Id, State.BotState.ChooseRange);
                    break;
                case "/play":
                    await client.SendTextMessageAsync(message.Chat.Id, "Выберите диапазон чисел:\n1. От 1 до 10\n2. От 1 до 100");
                    State.SetBotState(message.Chat.Id, State.BotState.ChooseRange);
                    break;
                case "/points":
                    long chatId = message.Chat.Id;
                    int points = Data.GetPointsFromDB(chatId);
                    await client.SendTextMessageAsync(chatId, $"У вас {points} балл(ов).");
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Я не понимаю вашей команды. Введите /play для игры или /points для того, чтобы узнать своё количество очков. Также, для отмены действия введите /cancel.");
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
                    SetMaxNumber(message.Chat.Id, 10);
                    SetTardetNumber(message.Chat.Id, GetMaxNumber(message.Chat.Id));
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выбрали диапазон от 1 до {GetMaxNumber(message.Chat.Id)}. Давайте начнем игру! Отгадайте число от 1 до {GetMaxNumber(message.Chat.Id)}. Введите число:");
                    State.SetBotState(message.Chat.Id, State.BotState.GameUpTo10);
                    break;
                case "2":
                    SetMaxNumber(message.Chat.Id, 100);
                    SetTardetNumber(message.Chat.Id, GetMaxNumber(message.Chat.Id));
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выбрали диапазон от 1 до {GetMaxNumber(message.Chat.Id)}. Давайте начнем игру! Отгадайте число от 1 до {GetMaxNumber(message.Chat.Id)}. Введите число:");
                    State.SetBotState(message.Chat.Id, State.BotState.GameTo100);
                    break;
                case "/cancel":
                    await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1 или 2 для выбора диапазона. Ведите /cancel, чтобы отменить команду.");
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
                if (guessedNumber == GetTargetNumber(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число! ");

                    long chatId = message.Chat.Id;
                    Data.UpdatePointsInDB(chatId, 1);

                    State.SetBotState(message.Chat.Id, State.BotState.Default);                 
                }
                else if (guessedNumber < GetTargetNumber(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Нет, загаданное число больше {guessedNumber}. Попробуйте еще раз! Введите число:");
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Нет, загаданное число меньше {guessedNumber}. Попробуйте еще раз! Введите число:");
                }
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число. Ведите /cancel, чтобы отменить игру.");
            
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
                if (guessedNumber == GetTargetNumber(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число!");

                    long chatId = message.Chat.Id;
                    Data.UpdatePointsInDB(chatId, 1);

                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Увы, вы не угадали. Загаданное число было - {GetTargetNumber(message.Chat.Id)}. Попробуйте еще раз!");
                    SetTardetNumber(message.Chat.Id, GetMaxNumber(message.Chat.Id));
                }
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число. Ведите /cancel, чтобы отменить игру.");
            
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
