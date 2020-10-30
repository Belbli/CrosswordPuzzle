﻿using Npgsql;
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

                            if (name != null)
                                user = new User(id, name, login);
                        }
                    }
                }
            }
            return user;
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

        public List<Crossword> getUserCrosswords(int id)
        {
            List<Crossword> crosswords = new List<Crossword>();
            using (var con = new NpgsqlConnection(connStr))
            {
                con.Open();

                var sql = String.Format("select * from get_user_crosswords({0})", id);

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Crossword cr = new Crossword(rdr.GetInt32(0), rdr.GetString(1), rdr.GetInt32(2));
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

                var sql = "select * from select_crosswords()";

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
                            crosswords.Add(new Crossword(id, name, theme, owner));
                        }
                    }
                }
            }
            return crosswords;
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
    }
}
