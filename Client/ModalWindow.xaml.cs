using System.Windows;
using Client.ClientService;

namespace Client
{
    public partial class ModalWindow : Window
    {
        private DBConnectionClient client;
        MainWindow.DialogDelegate sender;

        public ModalWindow()
        {
            InitializeComponent();
            client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
        }

        public ModalWindow(MainWindow.DialogDelegate sender)
        {
            InitializeComponent();
            this.sender = sender;
            client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
        }

        private void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            int res = client.signUpUser("", LoginTb.Text, PwdTb.Password);
           
            if(res == 1)
            {
                MessageBox.Show("Registered Successfully!");
                this.Close();
            }
            else
            {
                MessageBox.Show("User exists");
            }
        }

        private void LogInBtn_Click(object sender, RoutedEventArgs e)
        {
            User user = client.SignInUser(LoginTb.Text, PwdTb.Password);

            if(user != null)
            {
                this.sender(user);
                Close();
            }
            else
            {
                MessageBox.Show("Incorrect login or password");
            }
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
