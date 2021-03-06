﻿using Client.ClientService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для CreateCrosswordModal.xaml
    /// </summary>
    public partial class CreateCrosswordModal : Window
    {
        ObservableCollection<QuestionAnswer> questionsAnsrews = new ObservableCollection<QuestionAnswer>();
        ObservableCollection<QuestionAnswer> reserveWords = new ObservableCollection<QuestionAnswer>();
        User creator;
        Crossword editCrossword;
        string prevWord = "";

        public CreateCrosswordModal(User user, Crossword crossword)
        {
            InitializeComponent();
            creator = user;

            DBConnectionClient client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            CrosswordThemes.ItemsSource = client.getThemes();
            if (crossword != null)
            {
                editCrossword = crossword;
                setFieldsData();
                Title = "Edit Crossword";
                CreateCrosswordBtn.Content = "Save Changes";
            }

            questionsLB.ItemsSource = questionsAnsrews;
            ReserveWordsLB.ItemsSource = reserveWords;
            
            client.Close();

        }

        private void setFieldsData()
        {
            CrosswordName.Text = editCrossword.Namek__BackingField;
            CrosswordThemes.Text = editCrossword.Themek__BackingField;
            DBConnectionClient client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            questionsAnsrews = new ObservableCollection<QuestionAnswer>(editCrossword.questionsk__BackingField);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void AddRow(object sender, RoutedEventArgs e)
        {
            bool correct = checkWordToInsert(answerBox.Text);
            if(answerBox.Text == "")
            {
                return;
            }
            if(!correct)
            {
                reserveWords.Add(new QuestionAnswer()
                {
                    Answerk__BackingField = answerBox.Text,
                    Questionk__BackingField = questionBox.Text
                });
                MessageBox.Show("Current word is baad(");
            }
            if (correct)
            {
                
                QuestionAnswer qa = new QuestionAnswer();
                qa.Answerk__BackingField = answerBox.Text;
                qa.Questionk__BackingField = questionBox.Text;
                prevWord = answerBox.Text;
                if (questionBox.Text != "" && answerBox.Text != "")
                {
                    if (questionsLB.SelectedIndex > -1)
                    {
                        questionsAnsrews[questionsLB.SelectedIndex] = qa;
                        questionsLB.SelectedIndex = -1;
                    }
                    else
                    {
                        questionsAnsrews.Add(qa);
                    }
                    questionBox.Text = "";
                    answerBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Question or answer field is empty");
                }
                for (int i = 0; i < reserveWords.Count; i++)
                {
                    if (checkWordToInsert(reserveWords[i].Answerk__BackingField))
                    {
                        questionsAnsrews.Add(reserveWords[i]);
                        prevWord = reserveWords[i].Answerk__BackingField;
                        reserveWords.Remove(reserveWords[i]);
                    }
                }
            }
        }

        private bool checkWordToInsert(string text)
        {
            bool correct = false;
            if(prevWord.Length == 0)
            {
                return true;
            }
            string firstWord = prevWord.Substring(prevWord.Length / 2+1);
            string secondWord = text.Substring(0, text.Length / 2+1);
            for (int i = 0; i < firstWord.Length; i++)
            {
                for (int j = 0; j  < secondWord.Length; j++)
                {
                    if(firstWord[i] == secondWord[j])
                    {
                        return true;
                    }
                }
            }

            return correct;
        }

        private bool hasEmptyCells(ItemCollection rows)
        {
            foreach(QuestionAnswer qa in rows)
            {
                if(qa.Questionk__BackingField == "" || qa.Answerk__BackingField == "")
                {
                    return true;
                }
            }
            return false;
        }

        private void CloseModalBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateCrosswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if(questionsLB.Items.Count < 0)
            {
                MessageBox.Show("It's too few crossword items(8 is the minimum)");
            }
            else if(questionsLB.Items.Count > 18)
            {
                MessageBox.Show("You've created too much items(18 is maximum)");
            }
            else if(CrosswordName.Text == "")
            {
                MessageBox.Show("You need to name your crossword");
            }
            else if(CrosswordThemes.SelectedIndex == -1)
            {
                MessageBox.Show("You need to choose your crossword theme");
            }
            else if (hasEmptyCells(questionsLB.Items))
            {
                MessageBox.Show("Some rows are empty");
            }
            else
            {
                createCrossword();
                saveCrossword();
            }
            
        }

        private void createCrossword()
        {
            if (editCrossword == null)
                editCrossword = new Crossword();
            editCrossword.Namek__BackingField = CrosswordName.Text;
            editCrossword.theme = (Crossword.ThemeE)CrosswordThemes.SelectedIndex + 1;
            editCrossword.OwnerIDk__BackingField = creator.IDk__BackingField;
            editCrossword.Rathingk__BackingField = 0;
            QuestionAnswer[] qa = convertToArray(questionsLB.Items);
            editCrossword.questionsk__BackingField = qa;
        }

        private void saveCrossword()
        {
            DBConnectionClient client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            if(editCrossword.IDk__BackingField != 0)
            {
                client.editCrossword(editCrossword);
                MessageBox.Show("Crossword updated successfully!");
            }
            else
            {
                int index = client.createCrossword(editCrossword);
                int insertedItems = client.insertQuestions(editCrossword.questionsk__BackingField, index);
                MessageBox.Show("Crossword created successfully!", "Success", MessageBoxButton.OK);
            }
            
            client.Close();
        }

        private QuestionAnswer[] convertToArray(ItemCollection items)
        {
            QuestionAnswer[] qa = new QuestionAnswer[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                qa[i] = items[i] as QuestionAnswer;
            }
            return qa;
        }

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {

            QuestionAnswer qa = questionsLB.SelectedItem as QuestionAnswer;
            if(qa != null)
            {
                questionBox.Text = qa.Questionk__BackingField;
                answerBox.Text = qa.Answerk__BackingField;
                if (questionsLB.SelectedIndex > 0)
                    prevWord = (questionsLB.Items[questionsLB.SelectedIndex - 1] as QuestionAnswer).Answerk__BackingField;
                else
                    prevWord = "";
                questionsAnsrews.RemoveAt(questionsLB.SelectedIndex);
                checkCorrect();
            }
        }

        private void checkCorrect()
        {
            if(questionsAnsrews.Count > 0)
                prevWord = questionsAnsrews[0].Answerk__BackingField;
            for (int i = 0; i < questionsAnsrews.Count-1; i++)
            {
                if (!checkWordToInsert(questionsAnsrews[i + 1].Answerk__BackingField))
                {
                    reserveWords.Add(questionsAnsrews[i + 1]);
                    questionsAnsrews.RemoveAt(i + 1);
                }
                else
                {
                    prevWord = questionsAnsrews[i + 1].Answerk__BackingField;
                }
            }
        }

        private void CrosswordName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(CrosswordName.Text == "")
            {
                CrosswordName.BorderThickness = new Thickness(2);
            }
            else
            {
                CrosswordName.BorderThickness = new Thickness(0);
            }
        }
    }
}
