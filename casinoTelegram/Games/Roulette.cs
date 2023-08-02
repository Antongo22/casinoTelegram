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

        async public static Task HandleGameUpRouletteRate(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int rate) && rate > 0)
            {
                SetRateCasino(message.Chat.Id, rate);
                await client.SendTextMessageAsync(message.Chat.Id, $"Ваша ставка {Data.userStates[message.Chat.Id].rateCasino} принята.");
                await client.SendTextMessageAsync(message.Chat.Id, $"Выберите на что ставить:\n1. Конкретный номер (выгрыш в 37 больше ставки)\n2. " +
                    $"Чётное или нечётное (выигрыш в два раза больше ставки)");
                
                State.SetBotState(message.Chat.Id, State.BotState.RouletteChoose);
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только положительное, целое число. Ведите /cancel, чтобы отменить игру.");

        }

        async public static Task HandleChooseRangeState(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "1":
                    await client.SendTextMessageAsync(message.Chat.Id, $"Введите номер, на который вы ставите.");


                    break;
                case "2":
                    
                    break;
                case "/cancel":
                    await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1 или 2 для выбора игры. Ведите /cancel, чтобы отменить команду.");
                    break;
            }
        }

    }

}

