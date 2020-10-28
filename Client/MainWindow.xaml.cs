using Client.ClientService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public MainWindow()
        {

            client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            InitializeComponent();
            welcomeText.Visibility = Visibility.Visible;

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
            if(Menu.Visibility == Visibility.Visible)
                Menu.Visibility = Visibility.Collapsed;
            else
                Menu.Visibility = Visibility.Visible;
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            client.Close();
        }

        private void crosswordsBtn_Click(object sender, RoutedEventArgs e)
        {
            welcomeText.Visibility = Visibility.Collapsed;
            items = new ObservableCollection<Crossword>(client.getCrosswords(0, 0));
            crosswordItems.Visibility = Visibility.Visible;
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
                items = new ObservableCollection<Crossword>(client.getUserCrosswords((int)user.IDk__BackingField));
                foreach(Crossword cw in items)
                {
                    cw.OwnerLogink__BackingField = user.Logink__BackingField;
                }
                crosswordItems.ItemsSource = items;
                welcomeText.Visibility = Visibility.Collapsed;
                crosswordItems.Visibility = Visibility.Visible;
            }
        }

        private void logOutBtn_Click(object sender, RoutedEventArgs e)
        {
            logOutBtn.Visibility = Visibility.Hidden;
            user = null;
            accountBtn.Content = "Log In"; items = new ObservableCollection<Crossword>(client.getCrosswords(0, 0));
            
            crosswordItems.ItemsSource = items;
        }

        private void SolveBtn_Click(object sender, RoutedEventArgs e)
        {
            Crossword cw = (sender as Button)?.DataContext as Crossword;
            cw.questionsk__BackingField = client.getCrosswordQuestions((int)cw.IDk__BackingField);
            CrosswordSolver cs = new CrosswordSolver(cw);
            cs.Visibility = Visibility.Visible;
        }
    }
}
