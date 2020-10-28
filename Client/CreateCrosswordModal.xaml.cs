using Client.ClientService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для CreateCrosswordModal.xaml
    /// </summary>
    public partial class CreateCrosswordModal : Window
    {
        DBConnectionClient client;
        ObservableCollection<QuestionAnswer> list = new ObservableCollection<QuestionAnswer>();
        User creator;

        public CreateCrosswordModal(User user)
        {
            InitializeComponent();
            creator = user;
            client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            CrosswordThemes.ItemsSource = client.getThemes();
            questionsLB.ItemsSource = list;
        }

        private void AddRow(object sender, RoutedEventArgs e)
        {
            QuestionAnswer qa = new QuestionAnswer();
            qa.Answerk__BackingField = answerBox.Text;
            qa.Questionk__BackingField = questionBox.Text;

            if(questionBox.Text != "" && answerBox.Text != "")
            {
                if (questionsLB.SelectedIndex > -1)
                {
                    list[questionsLB.SelectedIndex] = qa;
                    questionsLB.SelectedIndex = -1;
                }
                else
                {
                    list.Add(qa);
                }
                questionBox.Text = "";
                answerBox.Text = "";
            }
            else
            {
                MessageBox.Show("Question or answer field is empty");
            }
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
                Crossword cw = createCrossword();
                saveCrossword(cw);
            }
            
        }

        private Crossword createCrossword()
        {
            Crossword cw = new Crossword();
            cw.Namek__BackingField = CrosswordName.Text;
            cw.theme = (Crossword.ThemeE)CrosswordThemes.SelectedIndex;
            cw.OwnerIDk__BackingField = creator.IDk__BackingField;
            QuestionAnswer[] qa = convertToArray(questionsLB.Items);
            cw.questionsk__BackingField = qa;
            return cw;
        }

        private void saveCrossword(Crossword cw)
        {            
            int index = client.createCrossword(cw);
            int insertedItems = client.insertQuestions(cw.questionsk__BackingField, index);
            MessageBox.Show(index.ToString());
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
