using Client;
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
            createRathing(crosswordID);
            return crosswordID;
        }

        private void createRathing(long crosswordId)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("INSERT INTO crossword_rathing (crossword_id, people_solved, rathing_count) VALUES({0}, {1}, {2})",
                    crosswordId, 0, 0);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {

                    }
                }
            }
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

        public int editCrossword(Crossword crossword)
        {
            deleteCrosswordQuestions(crossword.GetID());
            updateCrossword(crossword);
            return insertQuestions(crossword.GetQuestions(), crossword.GetID());
        }

        private void updateCrossword(Crossword crossword)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("UPDATE crosswords SET crossword_name='{0}', crossword_theme={1} WHERE crossword_id={2}",
                    crossword.GetName(), crossword.GetTheme(), crossword.GetID());
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {

                    }
                }
            }
        }

        public int insertQuestions(List<QuestionAnswer> items, long owner_id)
        {
            int insertedItems = 0;
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                foreach (QuestionAnswer qa in items)
                {
                    var sql = String.Format("select * from insert_question('{0}', '{1}', {2})", qa.Question, qa.Answer, owner_id);
                    Console.WriteLine(sql);
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
       
        public List<QuestionAnswer> getCrosswordQuestions(long crosswirdID)
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
    
        public long countFilteredCrosswords(FilterRequest filter)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select COUNT(*) from filter('{0}', '{1}', {2}, {3}, {4})",
                    filter.ThemeIds, filter.CrosswordName, filter.Offset, filter.Length, filter.Uid);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
                        return rdr.GetInt64(0);
                    }
                }
            }
        }

        private void deleteRathing(long crosswordId)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("DELETE from crossword_rathing where crossword_id={0}", crosswordId);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {

                    }
                }
            }
        }

        public List<Crossword> filterCrosswordsByThemeName(FilterRequest filter)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from filter('{0}', '{1}', {2}, {3}, {4})",
                    filter.ThemeIds, filter.CrosswordName, filter.Offset, filter.Length, filter.Uid);
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
                            double rathing = Math.Round(countRathing(id), 2);
                            crosswords.Add(new Crossword(id, name, theme, owner, rathing));
                        }
                    }
                }
            }
            return crosswords;
        }

        private float countRathing(long crosswordId)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("select * from countCrosswordRathing({0})", crosswordId);
                //Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
                        int peopleSolved = rdr.GetInt32(0);
                        int rathingSum = rdr.GetInt32(1);
                        if (peopleSolved == 0 || rathingSum == 0)
                            return 0;
                        else
                            return (float)Math.Round(((float)rathingSum / (float)peopleSolved), 2);
                    }
                }
            }
        }

        public void updateRathing(long crosswordId, int rathing)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("select * from updateRathing({0}, {1})", crosswordId, rathing);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        
                    }
                }
            }
        }

        private void deleteCrosswordQuestions(long id)
        {
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("DELETE from crosswords_questions where crossword_id = " + id);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {

                    }
                }
            }
        }

        public void deleteCrosswordById(long id)
        {
            deleteRathing(id);
            deleteCrosswordQuestions(id);
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                string sql = String.Format("DELETE from crosswords where crossword_id = " + id);
                Console.WriteLine(sql);
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {

                    }
                }
            }
        }
    }

}
