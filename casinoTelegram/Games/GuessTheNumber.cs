using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace casinoTelegram.Games
{
    internal static class GuessTheNumber
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

        /// <summary>
        /// Обработчик состояния ChooseRange
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleChooseRangeState(ITelegramBotClient client, Message message)
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
        async public static Task HandleGameTo100State(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int guessedNumber))
            {
                if (guessedNumber == GetTargetNumber(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число! ");

                    Data.UpdatePointsInDB(message.Chat.Id, 1);

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
        async public static Task HandleGameUpTo10State(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int guessedNumber))
            {
                if (guessedNumber == GetTargetNumber(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число!");

                    Data.UpdatePointsInDB(message.Chat.Id, 1);

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
    }
}
