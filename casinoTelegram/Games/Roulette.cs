using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace casinoTelegram.Games
{
    internal class Roulette
    {

        /// <summary>
        /// Задаём ставку игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="rate"></param>
        static void SetRateCasino(long chatID, int rateCasino) => Data.userStates[chatID].rateCasino = rateCasino;

        /// <summary>
        /// Задаём номер ставки игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="rate"></param>
        static void SetRate(long chatID, int rate) => Data.userStates[chatID].rate = rate;

        /// <summary>
        /// Рулетка с угадыванием конкретного номера
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static string GameNumber(long chatID)
        {
            Random rnd = new Random();
            Data.userStates[chatID].resultCasino = rnd.Next(0, 37);

            if (Data.userStates[chatID].resultCasino == Data.userStates[chatID].rate)
            {
                Data.UpdatePointsInDB(chatID, Data.userStates[chatID].rateCasino * 36);

                return $"Поздравляем, вы выиграли {Data.userStates[chatID].rateCasino * 36}";
            }
            else
            {
                Data.UpdatePointsInDB(chatID, Data.userStates[chatID].rateCasino * -1);

                return $"Вы проиграли, выпало число {Data.userStates[chatID].resultCasino}";
            }
        }

        /// <summary>
        /// Заносим ставку на чётность
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="rate"></param>
        static void SetParity(long chatID, string rate) => Data.userStates[chatID].rouletteParity = rate;     

        /// <summary>
        /// Процесс игры в чётность
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static string GameParity(long chatID)
        {
            Random rnd = new Random();
            Data.userStates[chatID].resultCasino = rnd.Next(0, 37);

            string text = $"Выпало число {Data.userStates[chatID].resultCasino}\n";

            string result;

            if (Data.userStates[chatID].resultCasino % 2 == 0)
            {
                result = "1";
            }
            else
            {
                result = "2";
            }

            if (result == Data.userStates[chatID].rouletteParity)
            {
                text += "Вы выиграли!";
                Data.UpdatePointsInDB(chatID, Data.userStates[chatID].rateCasino);
            }
            else
            {
                text += "Вы проиграли!";
                Data.UpdatePointsInDB(chatID, Data.userStates[chatID].rateCasino * -1);
            }

            return text ;
        }

        /// <summary>
        /// Вноос ставки
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleGameUpRouletteRate(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int rate) && rate > 0 && Data.GetPointsFromDB(message.Chat.Id) >= rate)
            {
                SetRateCasino(message.Chat.Id, rate);
                await client.SendTextMessageAsync(message.Chat.Id, $"Ваша ставка {Data.userStates[message.Chat.Id].rateCasino} принята.");
                await client.SendTextMessageAsync(message.Chat.Id, $"Выберите на что ставить:\n1. Конкретный номер (выгрыш в 37 больше ставки)\n2. " +
                    $"Чётное или нечётное (выигрыш в два раза больше ставки)", replyMarkup: Data.replyKeyboardMarkupCancel);
                
                State.SetBotState(message.Chat.Id, State.BotState.RouletteChoose);
            }
            else if (Data.GetPointsFromDB(message.Chat.Id) < rate)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Вы не можете поставить больше, чем у вас есть!", replyMarkup: Data.replyKeyboardMarkupCancel);
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена", replyMarkup: Data.replyKeyboardMarkupDefault);
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только положительное, целое число. " +
                "Ведите /cancel, чтобы отменить игру.");
        }

        /// <summary>
        /// Выбор режима рулетки
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleChooseRangeState(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "1":
                    await client.SendTextMessageAsync(message.Chat.Id, $"Введите номер, на который вы ставите (от 0 до 36)." , replyMarkup: Data.replyKeyboardMarkupCancel);
                    State.SetBotState(message.Chat.Id, State.BotState.RouletteChooseNumber);
                    break;
                case "2":
                    await client.SendTextMessageAsync(message.Chat.Id, $"Чтобы поставить на чёт/нечёт, введите \n1. Чётноеn\n2. Нечётное", replyMarkup: Data.replyKeyboardMarkupChoose12);
                    State.SetBotState(message.Chat.Id, State.BotState.RouletteChooseParity);
                    break;
                case "/cancel":
                    await client.SendTextMessageAsync(message.Chat.Id, "Отмена", replyMarkup: Data.replyKeyboardMarkupDefault);
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1 или 2 для выбора игры. " +
                        "Ведите /cancel, чтобы отменить команду.");
                    break;
            }
        }

        /// <summary>
        /// Обработчик игры с угадыванием числа
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleGameUpRouletteNumber(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int rate) && rate >= 0 && rate < 37)
            {
                SetRate(message.Chat.Id, rate);

                await client.SendTextMessageAsync(message.Chat.Id, GameNumber(message.Chat.Id));

                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена", replyMarkup: Data.replyKeyboardMarkupDefault);
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только целое число от 0 до 36. " +
                "Ведите /cancel, чтобы отменить игру.");
        }

        /// <summary>
        /// Обработчик игры с угадыванием чётности
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleGameUpRouletteParity(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "1":
                case "2":
                    SetParity(message.Chat.Id, message.Text);

                    await client.SendTextMessageAsync(message.Chat.Id, $"{GameParity(message.Chat.Id)}", replyMarkup: Data.replyKeyboardMarkupDefault);
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                case "/cancel":
                    await client.SendTextMessageAsync(message.Chat.Id, "Отмена", replyMarkup: Data.replyKeyboardMarkupDefault);
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1 или 2 для ставки." +
                        "Ведите /cancel, чтобы отменить команду.");
                    break;
            }

        }
    }

}

