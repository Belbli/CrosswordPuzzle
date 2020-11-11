using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DBService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "DBConnection" в коде и файле конфигурации.
    public class DBConnection : IDBConnection
    {
        private string connStr = String.Format(@"Server={0};Port={1};User Id={2};
                                                Password={3};Database={4};",
                                               "localhost", 5432, "postgres", "rootroot", "crossword");

        public int GetDiff(int a, int b)
        {
            return a - b;
        }

        public string GetTableData()
        {
            string data = "VALUE RETURNED";

            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = "SELECT * FROM users_data";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        Console.WriteLine($"{rdr.GetName(0),-4} {rdr.GetName(1),-10} {rdr.GetName(2),10}");

                        while (rdr.Read())
                        {
                            data += rdr.GetInt64(0).ToString() + " " + rdr.GetString(1) + " " + rdr.GetString(2) + "\n";
                        }
                    }
                }
            }
            
            return data;
        }

        public int GetSum(int a, int b)
        {
            return a + b;
        }

        public int signUpUser(string name, string login, string password)
        {
            int success = 0;
            if (findUserByLogin(login) != -1)
            {
                return 0;
            }
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from insert_user('{0}', '{1}', '{2}');", name, login, password);

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if(rdr.Read())
                            success = rdr.GetInt32(0);
                    }
                }
            }
            return success;
        }

        public long findUserByLogin(string login)
        {
            long userId = -1;
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();
                var sql = String.Format("select user_id from users_data where user_login = '{0}'", login);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            userId = rdr.GetInt64(0);
                    }
                }
            }
            return userId;
        }

        public User SignInUser(string login, string password)
        {
            User user = null;
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = "select * from validate_user('" + login + "', '" + password + "')";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if(rdr.Read())
                        {
                            long id = rdr.GetInt32(0);
                            string name = rdr.GetString(1);
                            int coins = rdr.GetInt32(2);
                            if (name != null)
                                user = new User(id, name, login, coins);
                        }
                    }
                }
            }
            return user;
        }

        public void saveCoins(long uid, int coins)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("UPDATE users_data SET user_coins = " + coins + "where user_id = " + uid);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        
                    }
                }
            }
        }

        public int createCrossword(Crossword cw)
        {
            int crosswordID = -1;
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("SELECT * FROM insert_crossword('{0}', {1}, {2})",
                                            cw.GetName(),
                                            cw.GetTheme(),
                                            cw.GetOwnerID());
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if(rdr.Read())
                            crosswordID = rdr.GetInt32(0);
                    }
                }
            }
            return crosswordID;
        }

        public List<string> getThemes()
        {
            List<string> themes = new List<string>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = "select * from get_themes()";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            themes.Add(rdr.GetString(0));
                        }
                    }
                }
            }
            return themes;
        }

        public int insertQuestions(List<QuestionAnswer> items, int owner_id)
        {
            int insertedItems = 0;
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                foreach (QuestionAnswer qa in items)
                {
                    var sql = String.Format("select * from insert_question('{0}', '{1}', {2})", qa.Question, qa.Answer, owner_id);

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            insertedItems++;
                        }
                    }
                }
            }
            return insertedItems;
        }
       

        public List<Crossword> getUserCrosswords(int id, int offset, int length)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from find_user_crosswords({0}, {1}, {2})", id, offset, length);

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Crossword cr = new Crossword(rdr.GetInt32(0), rdr.GetString(1), rdr.GetInt32(2), rdr.GetDouble(3));
                            crosswords.Add(cr);
                        }
                    }
                }
            }
            return crosswords;
        }

        public List<Crossword> getCrosswords(int offset, int count)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from find_crosswords({0}, {1})", offset, count);

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            long id = rdr.GetInt32(0);
                            string name = rdr.GetString(1);
                            int theme = rdr.GetInt32(2);
                            string owner = rdr.GetString(3);
                            double rathing = rdr.GetDouble(4);
                            crosswords.Add(new Crossword(id, name, theme, owner, rathing));
                        }
                    }
                }
            }
            return crosswords;
        }

        public void saveUsersCoins(long userId, int coins)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();
                var sql = String.Format("UPDATE users_data SET user_coins = {0} WHERE user_id = {1}", coins, userId);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                    }
                }
            }
        }

        public List<QuestionAnswer> getCrosswordQuestions(int crosswirdID)
        {
            List<QuestionAnswer> questions = new List<QuestionAnswer>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from select_crossword_questions({0})", crosswirdID);

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            QuestionAnswer cr = new QuestionAnswer(rdr.GetString(0), rdr.GetString(1));
                            questions.Add(cr);
                        }
                    }
                }
            }
            return questions;
        }

        public List<Crossword> filterCrosswordsByTheme(int offset, int count, int themeId)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from find_crosswords({0}, {1}, {2})", offset, count, themeId);

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            long id = rdr.GetInt32(0);
                            string name = rdr.GetString(1);
                            int theme = rdr.GetInt32(2);
                            string owner = rdr.GetString(3);
                            float rathing = rdr.GetFloat(4);
                            crosswords.Add(new Crossword(id, name, theme, owner, rathing));
                        }
                    }
                }
            }
            return crosswords;
        }

        public List<Crossword> filterUserCrosswordsByTheme(int id, int offset, int length, int themeId)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from get_user_crosswords({0}, {1}, {2}, {3})", id, offset, length, themeId);

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Crossword cr = new Crossword(rdr.GetInt32(0), rdr.GetString(1), rdr.GetInt32(2), rdr.GetFloat(3));
                            crosswords.Add(cr);
                        }
                    }
                }
            }
            return crosswords;
        }

        public List<Crossword> findCrosswords(int offset, int count, string crosswordName)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from find_crosswords_by_name({0}, {1}, '{2}%')", offset, count, crosswordName);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            long id = rdr.GetInt32(0);
                            string name = rdr.GetString(1);
                            int theme = rdr.GetInt32(2);
                            string owner = rdr.GetString(3);
                            double rathing = rdr.GetDouble(4);
                            Console.WriteLine(id);
                            crosswords.Add(new Crossword(id, name, theme, owner, rathing));
                        }
                    }
                }
            }
            return crosswords;
        }

        public int countCrosswords()
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = "SELECT COUNT(*) from crosswords";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
                        return rdr.GetInt32(0);
                    }
                }
            }
            return 0;
        }

        public int countFoundedCrosswords(string crosswodName)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = "SELECT COUNT(*) from crosswords where LOWER(crossword_name) LIKE LOWER('" + crosswodName + "%')";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
                        return rdr.GetInt32(0);
                    }
                }
            }
            return 0;
        }
    }
}
