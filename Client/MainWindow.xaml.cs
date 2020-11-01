using Client.ClientService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DBConnectionClient client;
        public User user;
        ObservableCollection<Crossword> items;
        public delegate void DialogDelegate(User user);
        List<Button> pagination = new List<Button>();
        int currentPage = 1;
        int maxPage;
        int pageElementsCount = 6;

        public MainWindow()
        {
            client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            InitializeComponent();
            welcomeText.Visibility = Visibility.Visible;
            maxPage = client.getCrosswords(0, 100).Length/* / pageElementsCount + 1*/;
            createPagination();
        }

        private void paginationBtnClick(object sender, RoutedEventArgs e)
        {
            int page = Int32.Parse((e.OriginalSource as Button).Content.ToString());

            currentPage = page;
            createPagination();
            items = new ObservableCollection<Crossword>(client.getCrosswords((currentPage - 1) * pageElementsCount, pageElementsCount));
            crosswordItems.ItemsSource = items;
            Console.WriteLine(page);
        }

        private void createPagination()
        {
            PaginationPanel.Children.Clear();
            if(currentPage > 3)
            {
                PaginationPanel.Children.Add(CreatePaginationBtn(1));
                PaginationPanel.Children.Add(CreatePaginationInterval());
            }

            
            if(currentPage > 1 && currentPage != maxPage)
                for (int i = currentPage - 1; i < currentPage + 2; i++)
                {
                    PaginationPanel.Children.Add(CreatePaginationBtn(i));
                }
            else if (currentPage == maxPage)
                for (int i = currentPage - 2; i < currentPage + 1; i++)
                {
                    PaginationPanel.Children.Add(CreatePaginationBtn(i));
                }
            else 
                for (int i = currentPage; i < currentPage + 3; i++)
                {
                    PaginationPanel.Children.Add(CreatePaginationBtn(i));
                }


            if (currentPage + 1 < maxPage)
            {
                
                PaginationPanel.Children.Add(CreatePaginationInterval());
                PaginationPanel.Children.Add(CreatePaginationBtn(maxPage));
            }
        }

        private TextBox CreatePaginationInterval()
        {
            TextBox tb = new TextBox();
            tb.Foreground = new SolidColorBrush(Colors.White);
            tb.BorderThickness = new Thickness(0);
            tb.FontSize = 30;
            tb.Text = "...";
            tb.Background = Brushes.Transparent;
            tb.VerticalAlignment = VerticalAlignment.Bottom;
            return tb;
        }

        private Button CreatePaginationBtn(int content)
        {
            Button btn = new Button();
            btn.Width = 30;
            btn.Height = 30;
            btn.Content = content;
            btn.Click += paginationBtnClick;
            btn.Style = Resources["ModernButton"] as Style;

            return btn;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ModalWindow mw = new ModalWindow();
            mw.Visibility = Visibility.Visible;
        }

        private void menuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Visibility == Visibility.Visible)
            {
                Menu.Visibility = Visibility.Collapsed;
                CrosswordsTable.Margin = new Thickness(0, 0, -150, 0);
            }
            else
            {
                Menu.Visibility = Visibility.Visible;
                CrosswordsTable.Margin = new Thickness(0);
            }
        }
        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            client.Close();
        }

        private void crosswordsBtn_Click(object sender, RoutedEventArgs e)
        {
            welcomeText.Visibility = Visibility.Collapsed;
            items = new ObservableCollection<Crossword>(client.getCrosswords((currentPage-1) * pageElementsCount, pageElementsCount));
            CrosswordsTable.Visibility = Visibility.Visible;
            crosswordItems.ItemsSource = items;
        }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            if(user == null)
            {
                ModalWindow mw = new ModalWindow(new DialogDelegate(setUser));
                mw.Visibility = Visibility.Visible;
            }
            else
            {
                CreateCrosswordModal modal = new CreateCrosswordModal(user);
                modal.Visibility = Visibility.Visible;
            }
        }

        private void setUser(User user)
        {
            this.user = user;
            accountBtn.Content = "My Account";
            logOutBtn.Visibility = Visibility.Visible;
            CoinsPanel.Visibility = Visibility.Visible;
            CoinsTB.Text = user.Coinsk__BackingField.ToString();
        }

        private void accountBtn_Click(object sender, RoutedEventArgs e)
        {
            if(user == null)
            {
                ModalWindow mw = new ModalWindow(new DialogDelegate(setUser));
                mw.Visibility = Visibility.Visible;
            }
            else
            {
                currentPage = 1;
                items = new ObservableCollection<Crossword>(client.getUserCrosswords((int)user.IDk__BackingField,
                                                                                            (currentPage-1)*pageElementsCount,
                                                                                            pageElementsCount));
                createPagination();
                foreach (Crossword cw in items)
                {
                    cw.OwnerLogink__BackingField = user.Logink__BackingField;
                }
                crosswordItems.ItemsSource = items;
                welcomeText.Visibility = Visibility.Collapsed;
                CrosswordsTable.Visibility = Visibility.Visible;
            }
        }

        private void logOutBtn_Click(object sender, RoutedEventArgs e)
        {
            logOutBtn.Visibility = Visibility.Hidden;
            CoinsPanel.Visibility = Visibility.Hidden;
            user = null;
            accountBtn.Content = "Log In"; items = new ObservableCollection<Crossword>(client.getCrosswords(0, 0));
            
            crosswordItems.ItemsSource = items;
        }

        private void SolveBtn_Click(object sender, RoutedEventArgs e)
        {
            Crossword cw = (sender as Button)?.DataContext as Crossword;
            cw.questionsk__BackingField = client.getCrosswordQuestions((int)cw.IDk__BackingField);
            CrosswordSolver cs = new CrosswordSolver(cw, user);
            cs.Visibility = Visibility.Visible;
        }
    }
}
