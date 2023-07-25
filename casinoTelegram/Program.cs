using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace casinoTelegram
{
    internal class Program
    {
        private enum BotState
        {
            Default,
            Game
        }

        private static BotState currentState = BotState.Default;

        static void Main(string[] args)
        {
            var client = new TelegramBotClient("6254402236:AAF-lAzwr4E1XjicyVw_Y6ENLNsilvAZwJM");
            client.StartReceiving(Update, Error);
            Console.WriteLine("Бот запущен. Нажмите любую клавишу, чтобы остановить.");
            Console.ReadKey();

        }

        async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;

            Console.WriteLine($"{message.Chat.FirstName ?? "--no name--"}\t\t|\t{message.Text ?? "--no text--"}");

            if (message.Text != null)
            {
                switch (currentState)
                {
                    case BotState.Default:
                        await HandleDefaultState(client, message);
                        break;
                    case BotState.Game:
                        await HandleGameState(client, message);
                        break;
                }
            }
        }

        async static Task HandleDefaultState(ITelegramBotClient client, Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    await client.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в наше казино! Введите /play, чтобы начать игру.");
                    break;
                case "/play":
                    await client.SendTextMessageAsync(message.Chat.Id, "Давайте начнем игру! Отгадайте число от 1 до 10. Введите число:");
                    currentState = BotState.Game;
                    break;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Я не понимаю вашей команды. Введите /play для игры.");
                    break;
            }
        }

        async static Task HandleGameState(ITelegramBotClient client, Message message)
        {
            if (int.TryParse(message.Text, out int guessedNumber))
            {
                // Произвольное число от 1 до 10
                Random random = new Random();
                int targetNumber = random.Next(1, 11);

                if (guessedNumber == targetNumber)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю! Вы угадали число! Введите /play, чтобы сыграть еще раз.");
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Увы, вы не угадали. Загаданное число было: {targetNumber}. Попробуйте еще раз! Введите число:");
                }
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите только число от 1 до 10.");
            }
        }

        private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
