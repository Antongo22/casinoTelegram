using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using casinoTelegram.Games;
using Dice = casinoTelegram.Games.Dice;

namespace casinoTelegram
{
    internal class Program
    {   
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
                        await State.HandleDefaultState(client, message);
                        break;
                    case State.BotState.ChooseRange:
                        await GuessTheNumber.HandleChooseRangeState(client, message);
                        break;
                    case State.BotState.GameTo100:
                        await GuessTheNumber.HandleGameTo100State(client, message);
                        break;
                    case State.BotState.GameUpTo10:
                        await GuessTheNumber.HandleGameUpTo10State(client, message);
                        break;
                    case State.BotState.GameDiceChoose:
                        await Dice.HandleChooseDice(client, message);
                        break;
                    case State.BotState.DicePvE:
                        await Dice.HandleDicePvE(client, message);
                        break;
                    case State.BotState.DicePvP:
                        await Dice.HandleDicePvP(client, message);
                        break;
                    case State.BotState.DicePvPSearch:
                        await Dice.HandleDicePvPSearch(client, message);
                        break;
                    case State.BotState.CasinoRate:
                        await Casino.HandleChooseCasinoRate(client, message);
                        break;
                    case State.BotState.CasinoAllRate:
                        await Casino.HandleGameUpCasinoGame(client, message);
                        break;

                }
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
