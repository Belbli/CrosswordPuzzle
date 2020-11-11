using Client.ClientService;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace Client
{
    public partial class CrosswordSolver : Window
    {
        List<CrossField.Word> labels = new List<CrossField.Word>();
        ObservableCollection<string> questions = new ObservableCollection<string>();
        int selectedRow = -1;
        public static ListBox lb;
        CrossField field;
        public User user;
        public MainWindow.SetCoinsDelegate setCoinsDelegate;

        public static int cellSize = 25;

        public CrosswordSolver(Crossword cw, User user, MainWindow.SetCoinsDelegate setCoinsDelegate)
        {
            field = new CrossField(cw, this);
            this.setCoinsDelegate = setCoinsDelegate;
            this.user = user;
            field.createCrossFields();
           
            labels = field.getWords();
            InitializeComponent();
            
            for (int i = 0; i < labels.Count; i++)
            {
                for (int j = 0; j < labels[i].getWord().Count; j++)
                {
                    labels[i].getWord()[j].Style = Resources["CrosswordCell"] as Style;
                    CrosswordPanel.Children.Add(labels[i].getWord()[j]);
                }
            }

            for (int i = 0; i < cw.questionsk__BackingField.Length; i++)
            {
                questions.Add((i+1) + ". " + cw.questionsk__BackingField[i].Questionk__BackingField);
            }
            lb = crosswordQuestions;
            crosswordQuestions.ItemsSource = questions;
            if(user != null)
            {
                CoinsPanel.Visibility = Visibility.Visible;
                CoinsTb.Text = user.Coinsk__BackingField.ToString();
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public void SetCoins(int coins)
        {
            CoinsTb.Text = coins.ToString();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void crosswordQuestions_Selected(object sender, RoutedEventArgs e)
        {
            if(selectedRow != -1 && !labels[selectedRow].solved)
            {
                colorWord(new SolidColorBrush(Colors.DarkBlue), selectedRow, 0);
            }
            selectedRow = crosswordQuestions.SelectedIndex;
            
            
            colorWord((SolidColorBrush)(new BrushConverter().ConvertFrom("#b2c91c")), selectedRow, 100);
            
        }

        private void colorWord(SolidColorBrush brush, int row, int zIndex)
        {
            if(!labels[row].solved)
            foreach (TextBox tb in labels[row].getWord())
            {
                tb.Background = brush;
                Canvas.SetZIndex(tb, zIndex);
            }
        }

        private void LetterHint_Click(object sender, RoutedEventArgs e)
        {
            if(field.solvedWords < labels.Count)
            {
                Random rand = new Random();
                int wordIndex;
                int letterIndex;
                do
                {
                    wordIndex = rand.Next(0, labels.Count);
                    letterIndex = (int)rand.Next(0, labels[wordIndex].getWord().Count);

                } while (labels[wordIndex].getWord()[letterIndex].Text != "");

                if (labels[wordIndex].getWord()[letterIndex].Text == "")
                {

                    labels[wordIndex].getWord()[letterIndex].Text = labels[wordIndex].answer[letterIndex].ToString();
                    
                    System.Drawing.Point p = labels[wordIndex].crossedLettersIndexes;
                    
                    if (p.X == letterIndex && wordIndex + 1 < labels.Count)
                    {
                        Console.WriteLine(wordIndex + " : " + p.X + ", " + p.Y);
                        labels[wordIndex + 1].getWord()[p.Y].Text = labels[wordIndex].answer[letterIndex].ToString();
                    }
                    if (wordIndex - 1 > 0)
                    {
                        System.Drawing.Point p2 = labels[wordIndex - 1].crossedLettersIndexes;
                        if (p2.Y == letterIndex)
                        {
                            labels[wordIndex - 1].getWord()[p2.X].Text = labels[wordIndex].answer[letterIndex].ToString();
                        }
                    }
                }
            }
        }

        private void WordHint_Click(object sender, RoutedEventArgs e)
        {
            if (field.solvedWords < labels.Count)
            {
                Random rand = new Random();
                int wordIndex;
                do
                {
                    wordIndex = rand.Next(0, labels.Count);

                } while (labels[wordIndex].solved);
                for (int i = 0; i < labels[wordIndex].getWord().Count; i++)
                {
                    labels[wordIndex].getWord()[i].Text = labels[wordIndex].answer[i].ToString();
                }
            }
        }
    }



    public class CrossField
    {
        int xStart = 60;
        int yStart = 60;
        int step = 25;
        List<Word> words = new List<Word>();
        Crossword crossword;
        private int wordCount = 0;
        Label fieldRow;
        public int solvedWords = 0;
        CrosswordSolver parent;

        public CrossField(Crossword cw, CrosswordSolver solver)
        {
            crossword = cw;
            parent = solver;
        }

        public class Word
        {
            
            List<TextBox> word = new List<TextBox>();
            public string answer;
            public Label rowNumber = new Label();
            public bool solved = false;
            CrossField parent;
            public System.Drawing.Point crossedLettersIndexes;


            public List<TextBox> getWord()
            {
                return word;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < word.Count; i++)
                {
                    sb.Append(word[i].Text);
                }
                return sb.ToString();
            }

            public Word(string answerWord, CrossField parent)
            {
                this.parent = parent;
                for (int i = 0; i < answerWord.Length; i++)
                {
                    TextBox tb = new TextBox();
                    tb.Width = CrosswordSolver.cellSize;
                    tb.Height = CrosswordSolver.cellSize;
                    tb.MaxLength = 1;
                    tb.FontSize = 18;
                    tb.TextAlignment = TextAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.TextChanged += TbTextChanged;
                    word.Add(tb);

                }
                answer = answerWord;
            }

            private void TbTextChanged(object sender, TextChangedEventArgs e)
            {
                TextBox tb = e.Source as TextBox;
                SetFocus(tb);
                string s = ToString();
                if (answer == s)
                {
                    solved = true;
                    
                    if(parent.parent.user != null)
                    {
                        parent.parent.user.Coinsk__BackingField += answer.Length * 20;
                        parent.parent.setCoinsDelegate(parent.parent.user.Coinsk__BackingField);
                        parent.parent.SetCoins(parent.parent.user.Coinsk__BackingField);
                    }
                    parent.solvedWords++;
                    MarkCorrectAnswer();
                    int index = FindCorrectWordIndex();
                    if (index + 1 < parent.words.Count)
                    {
                        SetUpperCellValue(index);
                    }

                    if (index - 1 > 0)
                    {
                        SetUnderCellValue(index);
                    }

                    if (parent.solvedWords == parent.words.Count)
                    {
                        MessageBox.Show("Congrats! You've solved puzzle");
                    }
                }
            }

            private int FindCorrectWordIndex()
            {
                for (int i = 0; i < parent.crossword.questionsk__BackingField.Length; i++)
                {
                    if (this.answer == parent.words[i].answer)
                    {
                        return i;
                    }
                }
                return -1;
            }

            private void MarkCorrectAnswer()
            {
                for (int i = 0; i < word.Count; i++)
                {
                    if (word[i].Text != "")
                    {
                        word[i].Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00b831"));
                        word[i].IsReadOnly = true;
                        word[i].Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }

            private void SetFocus(TextBox tb)
            {
                for (int i = 0; i < word.Count - 1; i++)
                {
                    if (word[i] == tb)
                    {
                        word[i + 1].Focus();
                        if (i + 2 < word.Count && word[i + 1].Text != "")
                        {
                            word[i + 2].Focus();
                            i++;
                        }
                        Canvas.SetZIndex(word[i + 1], 100);
                    }
                }
            }

            private void SetUpperCellValue(int index)
            {
                System.Drawing.Point point = parent.findSameLetterIndexes(parent.words[index], parent.words[index + 1]);
                parent.words[index + 1].getWord()[point.Y].Text = parent.words[index].getWord()[point.X].Text;
                parent.words[index + 1].getWord()[point.Y].IsReadOnly = true;
                parent.words[index + 1].getWord()[point.Y].Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00b831"));
            }

            private void SetUnderCellValue(int index)
            {
                System.Drawing.Point point = parent.findSameLetterIndexes(parent.words[index-1], parent.words[index]);
                parent.words[index-1].getWord()[point.X].Text = parent.words[index].getWord()[point.Y].Text;
                parent.words[index-1].getWord()[point.X].IsReadOnly = true;
                parent.words[index-1].getWord()[point.X].Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00b831"));
            }
        }



        public List<Word> getWords()
        {
            return words;
        }

        public Label getFieldRow()
        {
            return fieldRow;
        }
        public void createCrossFields()
        {
            for(int i = 0; i < crossword.questionsk__BackingField.Length; i++)
            {
                QuestionAnswer qa = crossword.questionsk__BackingField[i];
                words.Add(new Word(qa.Answerk__BackingField, this));
            }
            setPositions();
        }

        private bool setPositions()
        {
            
            for (int i = 0; i < words.Count; i++)
            {
                wordCount++;
                if(i == 0)
                {
                    setFirstWordPosition(words[i], xStart, yStart, step);
                }
                else
                {
                    System.Drawing.Point p = findSameLetterIndexes(words[i - 1], words[i]);

                    words[i-1].crossedLettersIndexes = p;
                    if (i % 2 == 1)
                    {
                        setVerticalWordPosition(words[i], p);
                    }
                    else
                    {
                        setHorizontalPosition(words[i], p);
                    }
                }
            }
            return true;
        }

        private void setHorizontalPosition(Word word, System.Drawing.Point p)
        {
            int x = xStart - p.Y * step;
            int y = yStart + p.X * step;
            xStart = x;
            yStart = y;
            
            List<TextBox> w = word.getWord();
            word.rowNumber = createRowLabel(x, y);
            for (int i = 0; i < w.Count; i++)
            {
                Canvas.SetLeft(w[i], x); // x coordinate
                Canvas.SetTop(w[i], y);
                x += step;
            }
        }

        private Label createRowLabel(int x, int y)
        {
            Label rowNum = new Label();
            rowNum.Content = (wordCount + 1).ToString();
            rowNum.Width = 15;
            rowNum.Height = 15;
            rowNum.Foreground = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(rowNum, x - 30);
            Canvas.SetTop(rowNum, y - 30);
            
            fieldRow = rowNum;
            rowNum.Background = new SolidColorBrush(Colors.Red);
            return rowNum;
        }

        private void setVerticalWordPosition(Word word, System.Drawing.Point p)
        {
            int x = xStart + p.X * step;
            int y = yStart - p.Y * step;
            xStart = x;
            yStart = y;
            List<TextBox> w = word.getWord();
            word.rowNumber = createRowLabel(x, y);
            for (int i = 0; i < w.Count; i++)
            {
                Canvas.SetLeft(w[i], x); // x coordinate
                Canvas.SetTop(w[i], y);
                y += step;
            }
        }

        private System.Drawing.Point findSameLetterIndexes(Word word1, Word word2)
        {
            
            System.Drawing.Point p = new System.Drawing.Point();
            string w1 = word1.answer;
            string w2 = word2.answer;
            for (int i = w1.Length / 2+1; i < w1.Length; i++)
            {
                for (int j = 0; j < w2.Length / 2+1; j++)
                {
                    if(w2[j] == w1[i])
                    {
                        return new System.Drawing.Point(i, j);
                    }
                }
            }
            return p;
        }

        private void setFirstWordPosition(Word word, int xStart, int yStart, int step)
        {
            word.rowNumber = createRowLabel(xStart, yStart);
            for (int i = 0; i < word.getWord().Count; i++)
            {
                
                Canvas.SetLeft(word.getWord()[i], xStart);
                Canvas.SetTop(word.getWord()[i], yStart);
                xStart += step;
            }
        }
    }
    
}