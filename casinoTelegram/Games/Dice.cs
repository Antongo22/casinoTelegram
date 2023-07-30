using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace casinoTelegram.Games
{
    internal static class Dice
    {
        /// <summary>
        /// Задаём кости игроку
        /// </summary>
        /// <param name="chatID"></param>
        static string SetDiceP(long chatID, int countOfDice)
        {
            string dice = "| ";
            Random rnd = new Random();  
            Data.userStates[chatID].diceP = 0;

            for (int i = 0; i < countOfDice; i++)
            {
                int diceP = rnd.Next(1, 7);
                Data.userStates[chatID].diceP += diceP;
                dice += diceP + " | ";
            }

            return dice;
        }

        /// <summary>
        /// Задаём кости бота
        /// </summary>
        /// <param name="chatID"></param>
        static string SetDiceE(long chatID, int countOfDice)
        {
            string dice = "| ";
            Random rnd = new Random();
            Data.userStates[chatID].diceE = 0;

            for (int i = 0; i < countOfDice; i++)
            {
                int diceE = rnd.Next(1, 7);
                Data.userStates[chatID].diceE += diceE;
                dice += diceE + " | ";
            }

            return dice;
        }

        /// <summary>
        /// Возвращает сумму костей игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static int GetDiceP(long chatID)
        {
            return Data.userStates[chatID].diceP;
        }

        /// <summary>
        /// Возвращает сумму костей бота
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static int GetDiceE(long chatID)
        {
            return Data.userStates[chatID].diceE;
        }

        /// <summary>
        /// Обработчик состояния ChooseRange
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleChooseDice(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "1":
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выбрали игру против бота. Выбирите сколько костей кидать, 1, 2 или 3. Количество костей соответствуют ставке.");
                    State.SetBotState(message.Chat.Id, State.BotState.DicePvE);
                    break;
                case "2":
                    await client.SendTextMessageAsync(message.Chat.Id, $"Игра проив игрока пока не работает");
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                case "/cancel":
                    await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1 или 2 для выбора режима. Ведите /cancel, чтобы отменить команду.");
                    break;
            }
        }

        async public static Task HandleDicePvE(ITelegramBotClient client, Message message)
        {
            if (message.Text == "1" ||  message.Text == "2" || message.Text == "3")
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Вам выпало - {SetDiceP(message.Chat.Id, int.Parse(message.Text))}");
                await client.SendTextMessageAsync(message.Chat.Id, $"Боту выпало выпало - {SetDiceE(message.Chat.Id, int.Parse(message.Text))}");
                await client.SendTextMessageAsync(message.Chat.Id, $"Вы - {GetDiceP(message.Chat.Id)}\nБот - {GetDiceE(message.Chat.Id)}");

                if (GetDiceP(message.Chat.Id) > GetDiceE(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Поздравляю! Вы победили!");
                    Data.UpdatePointsInDB(message.Chat.Id, int.Parse(message.Text));
                }
                else if (GetDiceP(message.Chat.Id) == GetDiceE(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Ничья!");
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы проиграли!");
                    Data.UpdatePointsInDB(message.Chat.Id, int.Parse(message.Text) * -1);
                }

                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число от 1 до 3. Ведите /cancel, чтобы отменить игру.");
        }
    }
}
