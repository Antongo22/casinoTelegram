namespace casinoTelegram
{
    internal static class Data
    {
        /// <summary>
        /// Данные, которые хранятся у юзера
        /// </summary>
        public class User
        {
            public User(State.BotState botState)
            {
                this.botState = botState;
            }
            public State.BotState botState; // состояние 
            public int targetNumber; // загаданное чилсло
            public int maxNumber; // максимальное число для загадывание
            public int diceP; // счёт костей игрока
            public int diceE; // счёт костей бота
            public long opponentID; // айли противника
            public int rate; // ставка для игры
            public string allDice; // полный бросок костей
            public bool findGame; // состояние поиска игры
            public int rateCasino; // ставка для казино в денежном эквиваленте
            public int resultCasino; // результат с казино
            public string resultCasinoSymb; // результат казино с отображением игруку
            public string rouletteParity; // чётность ставки
            public string crashCof; // коэффицент игрока
            public string crashGameCof; // коэффицент игры
        }

        // Хранение состояния для каждого пользователя
        public static Dictionary<long, User> userStates = new Dictionary<long, User>();

        public static SqlConnection SQLconnection = null; // Ссылка на БД с очками

        // Клавиатура для ответов
        public static ReplyKeyboardMarkup replyKeyboardMarkupDefault = new(new[]
        {
                new KeyboardButton[] { "/points", "/help", "/play" },
        })
        {
            ResizeKeyboard = true
        };

        // Клавиатура для ответов
        public static ReplyKeyboardMarkup replyKeyboardMarkupPlay = new(new[]
        {
                new KeyboardButton[] { "/number", "/dice", "/casino",},
                new KeyboardButton[] { "/roulette", "/points", "/cancel" },
        })
        {
            ResizeKeyboard = true
        };

        // Клавиатура для ответов
        public static ReplyKeyboardMarkup replyKeyboardMarkupChoose12 = new(new[]
        {
                new KeyboardButton[] { "1", "2" },
                new KeyboardButton[] { "/cancel" },
        })
        {
            ResizeKeyboard = true
        };

        // Клавиатура для ответов
        public static ReplyKeyboardMarkup replyKeyboardMarkupChoose123 = new(new[]
        {
                new KeyboardButton[] { "1", "2", "3" },
                new KeyboardButton[] { "/cancel" },
        })
        {
            ResizeKeyboard = true
        };

        // Клавиатура для ответов
        public static ReplyKeyboardMarkup replyKeyboardMarkupCancel = new(new[]
        {
                new KeyboardButton[] { "/cancel" },
        })
        {
            ResizeKeyboard = true
        };


        /// <summary>
        /// Пополнение очков пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="points"></param>
        public static void UpdatePointsInDB(long chatId, int points)
        {
            string query = $"IF EXISTS (SELECT * FROM [Points] WHERE [UserID] = '{chatId}') " +
                           $"UPDATE [Points] SET [Points] = [Points] + {points} WHERE [UserID] = '{chatId}' " +
                           $"ELSE INSERT INTO [Points] ([UserID], [Points]) VALUES ('{chatId}', {points})";

            using (SqlCommand command = new SqlCommand(query, SQLconnection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Получение баллов пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public static int GetPointsFromDB(long chatId)
        {
            int points = 0;
            string query = $"SELECT [Points] FROM [Points] WHERE [UserID] = '{chatId}'";

            using (SqlCommand command = new SqlCommand(query, SQLconnection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    points = reader.GetInt32(0);
                }
            }

            return points;
        }
    }
}
