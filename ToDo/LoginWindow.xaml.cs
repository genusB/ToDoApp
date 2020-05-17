using System.Configuration;
using System.Windows;
using Core.Services;

namespace ToDo
{
    public partial class LoginWindow
    {
        private readonly UserService Service;

        public LoginWindow()
        {
            InitializeComponent();
            Hide();

            Service = UserService.GetInstance();

            IsUserLoggedIn();
        }

        private void IsUserLoggedIn()
        {
            string loginStatus = ConfigurationManager.AppSettings["UserId"];

            if (loginStatus == "-1")
            {
                Show();
                return;
            }

            InitializeServices(int.Parse(loginStatus));

            var window = new MainWindow(this);

            window.Show();
        }

        private void EnterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("You have to fill all fields");
                return;
            }

            var id = Service.GetUserId(LoginTextBox.Text, PasswordBox.Password);

            if (id == -1)
            {
                MessageBox.Show("Wrong password. Try again.");
                return;
            }

            InitializeServices(id);

            var window = new MainWindow(this);

            window.Show();
            Hide();

            LoginTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;

            // Don't rememder password 
            if (RememberPasswordCheckBox?.IsChecked != null &&
                !(bool) RememberPasswordCheckBox.IsChecked)
                return;

            SetLoginInfo(id);

            if (RememberPasswordCheckBox != null)
                RememberPasswordCheckBox.IsChecked = false;
        }

        private static void InitializeServices(int id)
        {
            TagService.Initialize(id);
            ProjectService.Initialize(id);
            ToDoItemService.Initialize(id);
        }

        public void Refresh()
        {
            Service.RefreshRepositories();
        }

        private static void SetLoginInfo(int id)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["UserId"].Value = id.ToString();

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationWindow(Service);

            window.ShowDialog();
        }
    }
}