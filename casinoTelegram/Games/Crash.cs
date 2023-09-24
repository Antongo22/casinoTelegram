namespace casinoTelegram.Games
{
    internal class Crash
    {

        /// <summary>
        /// Задаём коэфицент ставки игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="rate"></param>
        static void SetRate(long chatID, int rate) => Data.userStates[chatID].rate = rate;



        /// <summary>
        /// Обработчик состояния HandleChooseCasinoRate
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleChooseCrashRate(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int rate) && rate > 0 && Data.GetPointsFromDB(message.Chat.Id) >= rate)
            {
                SetRate(message.Chat.Id, rate);
                await client.SendTextMessageAsync(message.Chat.Id, $"Ваша ставка {Data.userStates[message.Chat.Id].rate} принята.");
                SetRate(message.Chat.Id, rate);
                await client.SendTextMessageAsync(message.Chat.Id, $"Ввндите коэфицен ставки");
                State.SetBotState(message.Chat.Id, State.BotState.CrashSetCof);
            }
            else if (Data.GetPointsFromDB(message.Chat.Id) < rate)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Вы не можете поставить больше, чем у вас есть!");
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена", replyMarkup: Data.replyKeyboardMarkupDefault);
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только положительное, целое число. Ведите /cancel, чтобы отменить игру.");

        }
    }
}
