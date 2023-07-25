using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace casinoTelegram
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new TelegramBotClient("6254402236:AAF-lAzwr4E1XjicyVw_Y6ENLNsilvAZwJM");
            client.StartReceiving(Update, Error);
            Console.ReadKey();
        }

        async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;
            if(message.Text != null)
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Ты написал - {message.Text}");
                return;
            }
        }

        private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
