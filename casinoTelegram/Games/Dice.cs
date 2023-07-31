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

            Data.userStates[chatID].allDice = dice;
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
        /// Задаём ставку игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="rate"></param>
        static void SetRate(long chatID, int rate)
        {
            Data.userStates[chatID].rate = rate;
        }

        /// <summary>
        /// Получаем ставку игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static int GetRate(long chatID)
        {
            return Data.userStates[chatID].rate;
        }

        /// <summary>
        /// Функция поиска противника
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static bool FindOpponent(long chatID)
        {
            foreach (var key in Data.userStates.Keys)
            {
                if (key != chatID && Data.userStates[key].botState == State.BotState.DicePvPSearch && Data.userStates[key].rate == Data.userStates[chatID].rate)
                {
                    // Найден оппонент с той же ставкой и состоянием DicePvPSearch
                    // Устанавливаем состояние DicePvP для обоих игроков
                    Data.userStates[key].botState = State.BotState.DicePvPSearch;
                    Data.userStates[chatID].botState = State.BotState.DicePvPSearch;

                    // Сохраняем идентификатор оппонента для каждого игрока
                    Data.userStates[key].opponentID = chatID;
                    Data.userStates[chatID].opponentID = key;

                    return true; // Завершаем поиск оппонента
                }
            }
            return false;
            // Если не найден оппонент, игрок будет ожидать другого игрока с той же ставкой
        }

        /// <summary>
        /// Получение костей противника
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static int GetDiceOpponent(long chatID)
        {
            return Data.userStates[Data.userStates[chatID].opponentID].diceP;
        }

        /// <summary>
        /// Получение визуального выпадения костей противника
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static string GetAllDiceOpponent(long chatID)
        {
            return Data.userStates[Data.userStates[chatID].opponentID].allDice;
        }

        /// <summary>
        /// Получение айди противника
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        static long GetOpponentID(long chatID)
        {
            return Data.userStates[chatID].opponentID;
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
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выбрали игру против игрока. Выбирите сколько костей кидать, 1, 2 или 3. Количество костей соответствуют ставке.");
                    State.SetBotState(message.Chat.Id, State.BotState.DicePvP);
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

        /// <summary>
        /// Обработчик состояния DicePvE
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Обработчик состояния DicePvP
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleDicePvP(ITelegramBotClient client, Message message)
        {
            if (message.Text == "1" || message.Text == "2" || message.Text == "3")
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Ожидание игрока, как только он найдётся, игра произойдёт автоматически.");
                SetRate(message.Chat.Id, int.Parse(message.Text));

                State.SetBotState(message.Chat.Id, State.BotState.DicePvPSearch);

                await HandleDicePvPSearch(client, message);
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число от 1 до 3. Ведите /cancel, чтобы отменить игру.");
        }

        /// <summary>
        /// Обработчик состояния DicePvPSearch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async public static Task HandleDicePvPSearch(ITelegramBotClient client, Message message)
        {
            if (FindOpponent(message.Chat.Id))
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Противник найден!");
                await client.SendTextMessageAsync(GetOpponentID(message.Chat.Id), $"Противник найден!");

                await client.SendTextMessageAsync(message.Chat.Id, $"Вам выпало - {SetDiceP(message.Chat.Id, GetRate(message.Chat.Id))}");
                await client.SendTextMessageAsync(GetOpponentID(message.Chat.Id), $"Вам выпало - {SetDiceP(GetOpponentID(message.Chat.Id), GetRate(message.Chat.Id))}");

                await client.SendTextMessageAsync(message.Chat.Id, $"Противнику выпало - {GetAllDiceOpponent(message.Chat.Id)}");
                await client.SendTextMessageAsync(GetOpponentID(message.Chat.Id), $"Противнику выпало - {GetAllDiceOpponent(GetOpponentID(message.Chat.Id))}");

                await client.SendTextMessageAsync(message.Chat.Id, $"Вы - {GetDiceP(message.Chat.Id)}\nПротивник - {GetDiceOpponent(message.Chat.Id)}");
                await client.SendTextMessageAsync(GetOpponentID(message.Chat.Id), $"Вы - {GetDiceP(GetOpponentID(message.Chat.Id))}\nПротивник - {GetDiceOpponent(GetOpponentID(message.Chat.Id))}");


                if (GetDiceP(message.Chat.Id) > GetDiceOpponent(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Поздравляю! Вы победили!");
                    await client.SendTextMessageAsync(GetOpponentID(message.Chat.Id), $"Вы проиграли!");

                    Data.UpdatePointsInDB(message.Chat.Id, GetRate(message.Chat.Id));
                    Data.UpdatePointsInDB(GetOpponentID(message.Chat.Id), GetRate(message.Chat.Id) * -1);
                }
                else if (GetDiceP(message.Chat.Id) == GetDiceOpponent(message.Chat.Id))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Ничья!");
                    await client.SendTextMessageAsync(GetOpponentID(message.Chat.Id), $"Ничья!");
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы проиграли!");
                    await client.SendTextMessageAsync(GetOpponentID(message.Chat.Id), $"Поздравляю! Вы победили!");

                    Data.UpdatePointsInDB(message.Chat.Id, GetRate(message.Chat.Id) *-1);
                    Data.UpdatePointsInDB(GetOpponentID(message.Chat.Id), GetRate(message.Chat.Id));
                }

                State.SetBotState(message.Chat.Id, State.BotState.Default);
                State.SetBotState(GetOpponentID(message.Chat.Id), State.BotState.Default);
                return;
            }
            if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Подбор противника. Ожидайте или выйдите из игры с помощью /cancel.");
            }
        }
    }
}
