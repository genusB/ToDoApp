using System.Windows;

namespace ToDo
{
    public partial class TagWindow
    {
        public string NewText { get; private set; }

        public TagWindow()
        {
            InitializeComponent();
        }

        private void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TagTextBox.Text))
            {
                MessageBox.Show("Fill tag name field");
                return;
            }

            DialogResult = true;
            NewText = TagTextBox.Text;

            Close();
        }
    }
}