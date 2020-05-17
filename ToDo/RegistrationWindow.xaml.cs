using System.Windows;
using Core.Services;

namespace ToDo
{
    public partial class RegistrationWindow : Window
    {
        private readonly UserService Service;
        
        public RegistrationWindow(UserService service)
        {
            InitializeComponent();

            Service = service;
        }

        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(RepeatPasswordBox.Password))
            {
                MessageBox.Show("You have to fill all fields.");
                return;
            }

            if (LoginTextBox.Text.Contains(" ") ||
                PasswordBox.Password.Contains(" "))
            {
                MessageBox.Show("Spaces are not allowed. " +
                                "Instead use \'_\' symbol");
                return;
            }
            
            if (PasswordBox.Password != RepeatPasswordBox.Password)
            {
                MessageBox.Show("Passwords don\'t match");
                return;
            }

            if(Service.Add(LoginTextBox.Text, PasswordBox.Password))
            {
                MessageBox.Show("Successfully registered.");
                Close();
            }
        }
    }
}