using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

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
            public State.BotState botState;
            public int targetNumber;
            public int maxNumber;
        }

        // Хранение состояния для каждого пользователя
        public static Dictionary<long, User> userStates = new Dictionary<long, User>();

        public static SqlConnection SQLconnection = null; // Ссылка на БД с очками

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
