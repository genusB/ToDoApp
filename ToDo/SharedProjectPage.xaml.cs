using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Core.Models;
using Core.Services;
using ToDo.Classes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ToDo
{
    public partial class SharedProjectPage
    {
        private readonly ObservableCollection<ToDoItemView> ToDoItemsCollection;
        private readonly ToDoItemService ItemService;
        private readonly ProjectService ProjectService;
        private readonly ToDoItemOperations ToDoItemOperations;
        private readonly int ProjectId;
        private readonly MainWindow MainWindow;
        private bool MembersIsLoaded;

        public SharedProjectPage(MainWindow window, int projectId, string projectName)
        {
            InitializeComponent();

            ProjectNameLabel.Content = projectName;

            MainWindow = window;
            MembersIsLoaded = false;
            ProjectId = projectId;
            ToDoItemsCollection = new ObservableCollection<ToDoItemView>();
            ItemService = ToDoItemService.GetInstance();
            ProjectService = ProjectService.GetInstance();
            ToDoItemOperations = new SharedToDoItemOperations(ToDoItemsListView, ToDoItemsCollection, ProjectId);

            FillCollection();

            ToDoItemsListView.ItemsSource = ToDoItemsCollection;
        }

        private void FillCollection()
        {
            var collection = ItemService.GetSharedProjectItems(ProjectId).ToList();

            foreach (var i in collection)
            {
                ToDoItemsCollection.Add(ToDoItemOperations.ConvertToItemView(i));
            }
        }

        private void FillMembersExpander()
        {
            var members = ProjectService.GetProjectMembers(ProjectId).ToList();

            foreach (var i in members)
            {
                MembersExpander.Content += i + "\n";
            }
        }

        private void MembersExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (!MembersIsLoaded)
            {
                FillMembersExpander();
               MembersIsLoaded = true;
            }
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToDoItemOperations.Selected();
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToDoItemOperations.Add();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            ToDoItemOperations.Checked(sender);
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ToDoItemOperations.Unchecked(sender);
        }

        private void LeaveProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure? All tasks will be deleted",
                "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == DialogResult.No)
                return;

            ProjectService.LeaveProject(ProjectId);
            MainWindow.RemoveProject();
        }

        private void InviteButton_OnCLick(object sender, RoutedEventArgs e)
        {
            InvitedUsersTextBox.Visibility = Visibility.Visible;
            ConfirmButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;

            InviteButton.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            InvitedUsersTextBox.Text = "";
            InvitedUsersTextBox.Visibility = Visibility.Collapsed;

            ConfirmButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;

            InviteButton.Visibility = Visibility.Visible;
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            var userLogins = InvitedUsersTextBox.Text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (userLogins.Length == 0)
            {
                MessageBox.Show("Enter logins of users you want to invite");
                return;
            }

            // An error occured.
            if (ProjectService.InviteUsers(new ProjectModel
            {
                Id = ProjectId,
                Name = (string)ProjectNameLabel.Content
            }, userLogins, false) == -1)
            {
                return;
            }

            MessageBox.Show("Invitation is successfully completed.");

            CancelButton_OnClick(null, null);
        }
    }
}