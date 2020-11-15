using Client.ClientService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
        ObservableCollection<Crossword> items = new ObservableCollection<Crossword>();
        public delegate void DialogDelegate(User user);
        public delegate void SetCoinsDelegate(int coins);
        List<Button> pagination = new List<Button>();
        int currentPage = 1;
        long maxPage;
        const int pageElementsCount = 6;
        enum CrosswordsWindow { ALL_CROSSWORDS, USER_CROSSWORDS, SEARCH_CROSSWORDS, FILTER_CROSSWORDS };
        CrosswordsWindow crosswordsWindow;

        public MainWindow()
        {
            client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            InitializeComponent();
            welcomeText.Visibility = Visibility.Visible;
            setFilterBoxItems();
        }

        private void setFilterBoxItems()
        {
            string[] themes = client.getThemes();
            for (int i = 0; i < themes.Length; i++)
                FilterBox.Children.Add(createCheckBox(themes[i]));
        }
        

        private CheckBox createCheckBox(string filterItemName)
        {
            CheckBox cb = new CheckBox();
            cb.Content = filterItemName;
            cb.Margin = new Thickness(10, 0, 0, 0);
            cb.Foreground = new SolidColorBrush(Colors.White);
            return cb;
        }

        private void paginationBtnClick(object sender, RoutedEventArgs e)
        {
            int page = Int32.Parse((e.OriginalSource as Button).Content.ToString());
            long uid = user == null ? -1 : user.IDk__BackingField;
            currentPage = page;
            createPagination();
            int offset = (currentPage - 1) * pageElementsCount;
            switch (crosswordsWindow)
            {
                case CrosswordsWindow.ALL_CROSSWORDS:
                    items = new ObservableCollection<Crossword>(client.filterCrosswordsByThemeName(buildSearchRequest(-1, offset)));
                    break;
                case CrosswordsWindow.USER_CROSSWORDS:
                    items = loadUsersCrossword();
                    break;
                case CrosswordsWindow.SEARCH_CROSSWORDS:
                    items = items = new ObservableCollection<Crossword>(client.filterCrosswordsByThemeName(buildSearchRequest(-1, offset)));
                    break;
            }
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
            
            if(maxPage > 3)
            {
                if (currentPage > 1 && currentPage != maxPage)
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
            else
            {
                for (int i = 0; i < maxPage; i++)
                {
                    PaginationPanel.Children.Add(CreatePaginationBtn(i+1));
                }
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

        private Button CreatePaginationBtn(long content)
        {
            Button btn = new Button();
            btn.Width = 30;
            btn.Height = 30;
            btn.Content = content;
            btn.Click += paginationBtnClick;
            btn.Style = Resources["ModernButton"] as Style;
            if (content == currentPage)
                btn.Background = new SolidColorBrush(Colors.Coral);
            return btn;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public void SetCoins(int coins)
        {
            CoinsTB.Text = coins.ToString();
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
            if(user != null)
                client.saveCoins(user.IDk__BackingField, user.Coinsk__BackingField);
            Close();
            client.Close();
        }

        private void computeMaxPageNumber(long elements)
        {
            if (elements % pageElementsCount > 0)
            {
                maxPage = elements / pageElementsCount + 1;
            }
            else
            {
                maxPage = elements / pageElementsCount;
            }
            
        }

        private void crosswordsBtn_Click(object sender, RoutedEventArgs e)
        {
            NotFound.Visibility = Visibility.Collapsed;
            markCurrentMenuItem((sender as Button));
            string filterThemes = buildThemes();
            string searchCrosswordName = SearchBox.Text + "%";
            currentPage = 1;
            int offset = (currentPage - 1) * pageElementsCount;
            items = new ObservableCollection<Crossword>
                    (client.filterCrosswordsByThemeName(buildSearchRequest(-1, offset)));

            long elements = client.countFilteredCrosswords(buildSearchRequest(-1, offset, Int32.MaxValue));
            computeMaxPageNumber(elements);
            
            welcomeText.Visibility = Visibility.Collapsed;
            
            crosswordsWindow = CrosswordsWindow.ALL_CROSSWORDS;
            CrosswordsTable.Visibility = Visibility.Visible;
            crosswordItems.ItemsSource = items;
            createPagination();
            EditDelete.Visibility = Visibility.Collapsed;
        }

        private FilterRequest buildSearchRequest(long uid, int offset, int length = pageElementsCount)
        {
            FilterRequest fr = new FilterRequest();
            fr.Offsetk__BackingField = offset;
            fr.Lengthk__BackingField = length;
            fr.ThemeIdsk__BackingField = buildThemes();
            fr.CrosswordNamek__BackingField = SearchBox.Text + "%";
            fr.Uidk__BackingField = uid;
            return fr;
        }
       
        private void markCurrentMenuItem(Button clickedBtn)
        {
            int count = Menu.Children.Count;
            for(int i = 0; i < count; i++)
            {
                Button btn = (Button)Menu.Children[i];
                btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#330278"));
                btn.BorderThickness = new Thickness(0, 0, 0, 0);
                if (btn == clickedBtn)
                {
                    btn.BorderThickness = new Thickness(4, 0, 0, 0);
                    btn.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00ff55"));
                    btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00a4f0"));
                }
            }
        }

        private string buildThemes()
        {
            int count = FilterBox.Children.Count;
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < count; i++)//first el in stackpanel isnt checkbox
            {
                if ((((CheckBox)FilterBox.Children[i]).IsChecked ?? false))
                {
                    if (sb.Length > 0)
                        sb.Append(",");
                    sb.Append((i).ToString());
                }
            }
            if(sb.Length == 0)
            {
                for (int i = 0; i < count; i++)
                {
                        if (sb.Length > 0)
                            sb.Append(",");
                        sb.Append((i + 1).ToString());
                }
            }
            return sb.ToString();
        }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            markCurrentMenuItem((sender as Button));
            if (user == null)
            {
                ModalWindow mw = new ModalWindow(new DialogDelegate(setUser));
                mw.Visibility = Visibility.Visible;
            }
            else
            {
                CreateCrosswordModal modal = new CreateCrosswordModal(user, null);
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
            markCurrentMenuItem((sender as Button));
            if (user == null)
            {
                ModalWindow mw = new ModalWindow(new DialogDelegate(setUser));
                mw.Visibility = Visibility.Visible;
            }
            else
            {
                string filter = buildThemes();
                currentPage = 1;
                items = loadUsersCrossword();
               
                crosswordsWindow = CrosswordsWindow.USER_CROSSWORDS;
                long elements = client.countFilteredCrosswords(buildSearchRequest(user.IDk__BackingField, 0, Int32.MaxValue));
                computeMaxPageNumber(elements);
                createPagination();

                CrosswordsTable.Visibility = Visibility.Visible;
                crosswordItems.ItemsSource = items;
                if(items.Count < 1)
                {
                    CrosswordsTable.Visibility = Visibility.Collapsed;
                    NotFound.Visibility = Visibility.Visible;
                }
                else
                    CrosswordsTable.Visibility = Visibility.Visible;
                welcomeText.Visibility = Visibility.Collapsed;

                EditDelete.Visibility = Visibility.Visible;
            }
        }

        private ObservableCollection<Crossword> loadUsersCrossword()
        {
            string filter = buildThemes();
            int offset = (currentPage - 1) * pageElementsCount;
            ObservableCollection<Crossword> crosswords =  
                new ObservableCollection<Crossword>(client.filterCrosswordsByThemeName(buildSearchRequest(user.IDk__BackingField, offset)));
            foreach (Crossword cw in crosswords)
            {
                cw.OwnerLogink__BackingField = user.Logink__BackingField;

            }
            return crosswords;
        }

        private void logOutBtn_Click(object sender, RoutedEventArgs e)
        {
            string filterThemes = buildThemes();
            string searchCrosswordName = SearchBox.Text + "%";
            client.saveCoins(user.IDk__BackingField, user.Coinsk__BackingField);
            logOutBtn.Visibility = Visibility.Hidden;
            CoinsPanel.Visibility = Visibility.Hidden;
            user = null;
            accountBtn.Content = "Log In";
            currentPage = 1;
            int offset = (currentPage - 1) * pageElementsCount;
            items = new ObservableCollection<Crossword>
                    (client.filterCrosswordsByThemeName(buildSearchRequest(-1, offset)));

            long elements = client.countFilteredCrosswords(buildSearchRequest(-1, Int32.MaxValue));
            computeMaxPageNumber(elements);
            crosswordsWindow = CrosswordsWindow.ALL_CROSSWORDS;
            crosswordItems.ItemsSource = items;
        }

        private void SolveBtn_Click(object sender, RoutedEventArgs e)
        {
            markCurrentMenuItem((sender as Button));
            Crossword cw = (sender as Button)?.DataContext as Crossword;
            cw.questionsk__BackingField = client.getCrosswordQuestions((int)cw.IDk__BackingField);
            CrosswordSolver cs = new CrosswordSolver(cw, user, new SetCoinsDelegate(SetCoins));
            cs.Visibility = Visibility.Visible;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            string filterThemes = buildThemes();
            string searchCrosswordName = SearchBox.Text + "%";
            long uid = user == null ? -1 : user.IDk__BackingField;
            int offset = (currentPage - 1) * pageElementsCount;
            items = new ObservableCollection<Crossword>
                        (client.filterCrosswordsByThemeName(buildSearchRequest(uid , offset)));

            long elements = client.countFilteredCrosswords(buildSearchRequest(uid, 0, Int32.MaxValue));
            computeMaxPageNumber(elements);

            crosswordItems.ItemsSource = items;
            createPagination();
            welcomeText.Visibility = Visibility.Collapsed;
            CrosswordsTable.Visibility = Visibility.Visible;
            NotFound.Visibility = Visibility.Collapsed;
            if(uid != -1)
                EditDelete.Visibility = Visibility.Visible;
            else
                EditDelete.Visibility = Visibility.Collapsed;
        }

        private void DeleteBtnClick(object sender, RoutedEventArgs e)//DeleteBtnClick
        {
            if (crosswordItems.SelectedItem != null)
            {
                Crossword toDelete = (Crossword)crosswordItems.SelectedItem;
                MessageBoxResult res = MessageBox.Show("Are you shure you want to delete?", "Confirm deletion", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    client.deleteCrosswordById(toDelete.IDk__BackingField);
                    
                    long elements = client.countFilteredCrosswords(buildSearchRequest(user.IDk__BackingField, 0, Int32.MaxValue));
                    computeMaxPageNumber(elements);
                    if (maxPage < currentPage)
                        currentPage--;
                    int offset = (currentPage - 1) * pageElementsCount;
                    items = new ObservableCollection<Crossword>
                        (client.filterCrosswordsByThemeName(buildSearchRequest(user.IDk__BackingField, offset)));
                    crosswordItems.ItemsSource = items;
                    
                    
                    createPagination();
                }
            }
            else
                MessageBox.Show("Choose Crossword to delete", "Can not delete", MessageBoxButton.OK);
        }

        private void EditBtnClick(object sender, RoutedEventArgs e)
        {
            if (crosswordItems.SelectedItem != null)
            {
                Crossword toEdit = (Crossword)crosswordItems.SelectedItem;
                toEdit.questionsk__BackingField = client.getCrosswordQuestions(toEdit.IDk__BackingField);
                CreateCrosswordModal editModal = new CreateCrosswordModal(user, toEdit);
                editModal.Show();
            }
            else
                MessageBox.Show("Choose Crossword to delete", "Can not delete", MessageBoxButton.OK);
        }
    }
}
