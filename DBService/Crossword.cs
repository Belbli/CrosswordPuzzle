﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBService
{
    [Serializable]
    public class Crossword
    {
        enum ThemeE { Sport = 1, Animals, Other, Auto, Georaphy, History, Music, Informatic }
        long ID { get; }
        string Name { get; set; }
        string Theme { get; set; }

        double Rathing { get; set; }

        List<QuestionAnswer> questions { get; set; }

        ThemeE theme;
        
        string OwnerLogin { get; set; }

        long OwnerID { get; set; }

        public string GetName()
        {
            return Name;
        }

        public List<QuestionAnswer> GetQuestions()
        {
            return questions;
        }

        public int GetTheme()
        {
            return (int)theme;
        }

        public long GetID()
        {
            return ID;
        }

        public long GetOwnerID()
        {
            return OwnerID;
        }

        public Crossword(long id, string name, int themeIndx, double rathing)
        {
            ID = id;
        
            Name = name;
            theme = (ThemeE)themeIndx;
            Theme = theme.ToString("g");

            Rathing = rathing;
        }

        public Crossword(long id, string name, int themeIndx, string owner, double rathing)
        {
            ID = id;
            Name = name;
            theme = (ThemeE)themeIndx;
            Theme = theme.ToString("g");
            OwnerLogin = owner;
            Rathing = rathing;
        }

        public Crossword(string name, int themeIndx, int ownerID)
        {
            Name = name;
            theme = (ThemeE)themeIndx;
            Theme = theme.ToString("g");
            OwnerID = ownerID;
        }

        public override string ToString()
        {
            return ID.ToString() + ", " + Name + ", " + Theme;
        }
    }
}
