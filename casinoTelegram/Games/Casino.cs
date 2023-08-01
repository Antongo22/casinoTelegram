using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace casinoTelegram.Games
{
    internal static class Casino
    {
        static int CountIdenticalSymbols(char[] symbols)
        {
            if (symbols == null || symbols.Length == 0)
                return 0;

            // Создаем словарь для хранения символов и их количества
            Dictionary<char, int> characterCounts = new Dictionary<char, int>();

            // Проходим по каждому символу в массиве и подсчитываем их количество
            foreach (char c in symbols)
            {
                // Если символ уже есть в словаре, увеличиваем его количество на 1
                if (characterCounts.ContainsKey(c))
                {
                    characterCounts[c]++;
                }
                else
                {
                    // Иначе добавляем символ в словарь с количеством 1
                    characterCounts.Add(c, 1);
                }
            }

            int maxRepetitions = 0;

            // Находим максимальное количество повторений
            foreach (var kvp in characterCounts)
            {
                if (kvp.Value > maxRepetitions)
                {
                    maxRepetitions = kvp.Value;
                }
            }

            return maxRepetitions;
        }

        static void SetCasino(long chatID)
        {
            char[] symbols;
            switch (Data.userStates[chatID].rate)
            {
                case 1:
                    symbols = "ABC".ToCharArray();
                    break;
                case 2:
                    symbols = "ABCDE".ToCharArray();
                    break;
                case 3:
                    symbols = "ABCDEFG".ToCharArray();
                    break;
                default:
                    symbols = "ABCDEFG".ToCharArray();
                    break;
            }
            
            char[] result = new char[3];

            string win = "";

            Random random = new Random();

            for (int i = 0; i < 3; i++)
            {
                result[i] = symbols[random.Next(symbols.Length)];
                win += result[i] + " ";
            }

            int identicalCount = CountIdenticalSymbols(result);

            Data.userStates[chatID].resultCasinoSymb = win;            

            if (identicalCount == 3)
            {
                Data.userStates[chatID].resultCasino = Data.userStates[chatID].rate * Data.userStates[chatID].rateCasino * 3;
                Data.UpdatePointsInDB(chatID, Data.userStates[chatID].rate * Data.userStates[chatID].rateCasino);
            }
            if (identicalCount == 2)
            {
                Data.userStates[chatID].resultCasino = Data.userStates[chatID].rate * Data.userStates[chatID].rateCasino;
                Data.UpdatePointsInDB(chatID, Data.userStates[chatID].rate * Data.userStates[chatID].rateCasino);
            }
            else
            {
                Data.userStates[chatID].resultCasino = Data.userStates[chatID].rateCasino * -1;
                Data.UpdatePointsInDB(chatID, Data.userStates[chatID].rateCasino * -1);
            }
        }

        /// <summary>
        /// Задаём коэфицент ставки игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="rate"></param>
        static void SetRate(long chatID, int rate) => Data.userStates[chatID].rate = rate;

        /// <summary>
        /// Задаём ставку игрока
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="rate"></param>
        static void SetRateCasino(long chatID, int rateCasino) => Data.userStates[chatID].rateCasino = rateCasino;

        static string GetResultCasinoSymb(long chatID) => Data.userStates[chatID].resultCasinoSymb;

        static int GetResultCasino(long chatID) => Data.userStates[chatID].resultCasino;

        async public static Task HandleChooseCasinoRate(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "1":
                case "2":
                case "3":
                    SetRate(message.Chat.Id, int.Parse(message.Text));
                    await client.SendTextMessageAsync(message.Chat.Id, $"Ваш коэффицент - {Data.userStates[message.Chat.Id].rate}. Теперь, введите какое количество очков вы поставите.");          
                    State.SetBotState(message.Chat.Id, State.BotState.CasinoAllRate);                           
                    break;
                case "/cancel":
                    await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                    State.SetBotState(message.Chat.Id, State.BotState.Default);
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, выберите 1, 2 или 3 для выбора коэффицента ставки. Ведите /cancel, чтобы отменить команду.");
                    break;
            }
        }
      
        async public static Task HandleGameUpCasinoGame(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int rate) && rate > 0)
            {
                SetRateCasino(message.Chat.Id, rate);
                await client.SendTextMessageAsync(message.Chat.Id, $"Ваша ставка {Data.userStates[message.Chat.Id].rateCasino} принята. Крутим барабан");

                SetCasino(message.Chat.Id);
                await client.SendTextMessageAsync(message.Chat.Id, $"Вам выпало - {GetResultCasinoSymb(message.Chat.Id)}");

                if(GetResultCasino(message.Chat.Id) > 0)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы выиграли - {GetResultCasino(message.Chat.Id)}");
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы проиграли {GetResultCasino(message.Chat.Id)}");
                }

                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else if (message.Text == "/cancel")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Отмена");
                State.SetBotState(message.Chat.Id, State.BotState.Default);
            }
            else await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только положительное, целое число. Ведите /cancel, чтобы отменить игру.");

        }
    }
}
