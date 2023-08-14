using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace casinoTelegram
{
    internal static class State
    {

        /// <summary>
        /// Определение возможных состояний бота
        /// </summary>
        public enum BotState
        {
            Default, // стандартное значение
            ChooseRange, // выбор диапозона для игры
            GameTo100, // процесс игры до 100
            GameUpTo10, // процесс игры до 10
            GameDiceChoose, // выбор игры в кости (пве/пвп)
            DicePvE, // игра в кости пве
            DicePvP, // игра в кости пап
            DicePvPSearch, // ожидание противника
            CasinoRate, // коэфицент ставки
            CasinoAllRate, // ставка в денежном эквиваленте
            RouletteRate, // вносим ставку
            RouletteChoose, // выбор на что ставить номер или цвет
            RouletteChooseNumber, // рулетка пря выборе номера
            RouletteChooseParity, // рулетка пря выборе чётности

        }

        /// <summary>
        /// Передача текущего состояния для каждого пользователя
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        public static BotState GetBotState(long chatID)
        {
            if (!Data.userStates.ContainsKey(chatID))
            {
                Data.userStates.Add(chatID, new Data.User(BotState.Default));
            }
            return Data.userStates[chatID].botState;
        }

        /// <summary>
        /// Получение текущего состояния для каждого пользователя
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="botState"></param>
        public static void SetBotState(long chatID, BotState botState) => Data.userStates[chatID].botState = botState;

        /// <summary>
        /// Обработчик состояния Default
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleDefaultState(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    await client.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в наше казино! " +
                        "Введите /play, чтобы начать игру или /points для того, чтобы узнать своё количество очков. " +
                        "Также, для отмены действия введите /cancel. Если возникнут проблемы, можете прописать /help");
                    break;
                case "/help":
                    await client.SendTextMessageAsync(message.Chat.Id, "Введите /play, чтобы начать игру или /points для того, " +
                        "чтобы узнать своё количество очков. Также, для отмены действия введите /cancel.");
                    SetBotState(message.Chat.Id, BotState.ChooseRange);
                    break;
                case "/play":
                    await client.SendTextMessageAsync(message.Chat.Id, "Вот список игр - \n/number - игра в угадай число.\n" +
                        "/dice - игра в кости\n/casino - для игры в казино\n/roulette - для игры в рулетку");
                    break;
                case "/number":
                    await client.SendTextMessageAsync(message.Chat.Id, "Выберите диапазон чисел:\n1. От 1 до 10\n2. От 1 до 100");
                    SetBotState(message.Chat.Id, BotState.ChooseRange);
                    break;
                case "/dice":
                    await client.SendTextMessageAsync(message.Chat.Id, "Выберите режим:\n1. PvE\n2. PvP");
                    SetBotState(message.Chat.Id, BotState.GameDiceChoose);
                    break;
                case "/casino":
                    await client.SendTextMessageAsync(message.Chat.Id, "Выберите коэффицент ставки:\n1. 1\n2. 2\n3. 3");
                    SetBotState(message.Chat.Id, BotState.CasinoRate);
                    break;
                case "/roulette":
                    await client.SendTextMessageAsync(message.Chat.Id, "Внесите вашу ставку");
                    SetBotState(message.Chat.Id, BotState.RouletteRate);
                    break;
                case "/points":
                    int points = Data.GetPointsFromDB(message.Chat.Id);
                    await client.SendTextMessageAsync(message.Chat.Id, $"У вас {points} балл(ов).");
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Я не понимаю вашей команды. Введите /play для игры или /points для того, " +
                        "чтобы узнать своё количество очков. Также, для отмены действия введите /cancel. Если возникнут проблемы, " +
                        "можете прописать /help.");
                    break;
            }
        }

    }
}
