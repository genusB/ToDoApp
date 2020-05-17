using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Core.Models;
using Core.Services;
using ToDo.Classes;
using MaterialDesignColors;
using System.Windows.Media;

namespace ToDo
{
    public partial class MainWindow
    {
        private readonly LoginWindow LoginWindow;
        private readonly TagService TagService;
        private readonly ProjectService ProjectService;
        private readonly ObservableCollection<ProjectView> Projects;
        private readonly ObservableCollection<InvitationRequestModel> Invitations;
        private bool IsLogOut;

        public MainWindow(LoginWindow LoginWindow)
        {
            InitializeComponent();

            this.ProjectService = ProjectService.GetInstance();
            Invitations = new ObservableCollection<InvitationRequestModel>();
            Projects = new ObservableCollection<ProjectView>();

            ShowDefaultProjects();
            ShowSharedProjects();
            ShowInvitations();

            SearchConditionsComboBox.ItemsSource = Projects;
            ProjectsListView.ItemsSource = Projects;
            InvitationsListView.ItemsSource = Invitations;

            IsLogOut = false;
            this.LoginWindow = LoginWindow;
            this.TagService = TagService.GetInstance();
        }


        private void ShowDefaultProjects()
        {
            Projects.Add(new ProjectView
            {
                ImageSource = "Resources/inbox.png",
                Name = "Inbox"
            });

            Projects.Add(new ProjectView
            {
                ImageSource = "Resources/today.png",
                Name = "Today"
            });

            Projects.Add(new ProjectView
            {
                ImageSource = "Resources/upcoming.png",
                Name = "Upcoming"
            });

            Projects.Add(new ProjectView
            {
                ImageSource = "Resources/logbook.png",
                Name = "Logbook"
            });
        }

        private void ShowSharedProjects()
        {
            var foundProjects = ProjectService.GetProjects().ToList();
            foundProjects.ForEach(project =>
            {
                Projects.Add(new ProjectView
                {
                    Id = project.Id,
                    ImageSource = "Resources/shared.png",
                    Name = project.Name
                });
            });
            
        }

        private void ShowInvitations()
        {
            var Invitations = ProjectService.GetInvitations().ToList();

            Invitations.ForEach(i => Invitations.Add(i));

            InvitationsListView.Visibility = Invitations.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ProjectsListView.SelectedIndex;

            if (selectedIndex == -1)
            {
                PagesFrame.Content = null;
                return;
            }

            PagesFrame.NavigationService.RemoveBackEntry();

            switch (selectedIndex)
            {
                case 0:
                    PagesFrame.Content = new InboxPage();
                    break;

                case 1:
                    PagesFrame.Content = new TodayPage();
                    break;

                case 2:
                    PagesFrame.Content = new UpcomingPage(this);
                    break;

                case 3:
                    PagesFrame.Content = new LogbookPage(this);
                    break;

                default:
                    var project = (ProjectView) ProjectsListView.Items[selectedIndex];

                    PagesFrame.Content = new SharedProjectPage(this, project.Id, project.Name);
                    break;
            }
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this);
        }

        public void UpdateLogbookPage()
        {
            PagesFrame.Content = new LogbookPage(this);
        }

        private void SearchConditionsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchConditionsComboBox.SelectedIndex = -1;
        }

        private void ConditionChanged()
        {
            if (!FoundToDoItems.Items.IsEmpty)
            {
                SearchExpander.IsExpanded = false;
                FoundToDoItems.ItemsSource = null;
            }
        }

        private void ConditionCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            ConditionChanged();
        }

        private void ConditionCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConditionChanged();
        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            if (Projects.All(p => !p.IsSelected))
            {
                MessageBox.Show("Pick Projects to search in");
                return;
            }

            var enteredTags = SearchTextBox.Text.Split(new[] {' '},
                StringSplitOptions.RemoveEmptyEntries);

            if (enteredTags.Length == 0)
            {
                MessageBox.Show("Enter at least one tag");
                return;
            }

            var searchingProjectsNames = Projects.Where(p => p.IsSelected).Select(p => p.Name).ToList();

            FoundToDoItems.ItemsSource = null;

            SearchExpander.IsExpanded = FillFoundListBox(enteredTags, searchingProjectsNames);
            SearchExpander.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private bool FillFoundListBox(IEnumerable<string> tags, List<string> ProjectsNames)
        {
            var foundItems = TagService.GetItemsByTags(tags, ProjectsNames).ToList();

            if (foundItems.Count == 0)
                return false;

            FoundToDoItems.ItemsSource = foundItems;

            return true;
        }

        private void FoundToDoItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = FoundToDoItems.SelectedIndex;

            if (index == -1) return;

            var selectedToDoItem = (ToDoItemModel) FoundToDoItems.Items[index];
            var minDate = DateTime.MinValue.AddYears(DateTime.Now.Year);

            if (Projects.Any(p => p.Name == selectedToDoItem.ProjectName && p.IsSelected) &&
                selectedToDoItem.CompleteDay == minDate)
            {
                var projectView = Projects.First(i => i.Id == (selectedToDoItem.ProjectId ?? -1));

                ProjectsListView.SelectedIndex = Projects.IndexOf(projectView);

                FoundToDoItems.SelectedIndex = -1;
                return;
            }

            // Inbox page.
            if (selectedToDoItem.Date == minDate && selectedToDoItem.ProjectName == "" &&
                selectedToDoItem.CompleteDay == minDate)
                ProjectsListView.SelectedIndex = 0;
            // Today page.
            else if ((selectedToDoItem.Date <= DateTime.Today && selectedToDoItem.Date != minDate ||
                      selectedToDoItem.Deadline <= DateTime.Today && selectedToDoItem.Deadline != minDate) &&
                     selectedToDoItem.CompleteDay == minDate)
                ProjectsListView.SelectedIndex = 1;
            // Upcoming page.
            else if (selectedToDoItem.CompleteDay == minDate)
                ProjectsListView.SelectedIndex = 2;
            // Logbook page.
            else
                ProjectsListView.SelectedIndex = 3;

            FoundToDoItems.SelectedIndex = -1;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            if (!IsLogOut)
               LoginWindow.Close();
        }

        private void LogOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["UserId"].Value = "-1";

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            IsLogOut = true;
            Close();
           LoginWindow.Show();
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            Service.RefreshContext();

           LoginWindow.Refresh();

            TagService.GetInstance().RefreshRepositories();
            ProjectService.GetInstance().RefreshRepositories();
            ToDoItemService.GetInstance().RefreshRepositories();

            PagesListView_OnSelectionChanged(null, null);

            ShowInvitations();
        }

        private void AddSharedProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewProjectStackPanel.Visibility = Visibility.Visible;
            AddProjectButton.Visibility     = Visibility.Collapsed;
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                MessageBox.Show("You need to fill project name field");
                return;
            }

            var userLogins = InvitedUsersTextBox.Text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var projectId = ProjectService.InviteUsers(new ProjectModel {Name = ProjectNameTextBox.Text},
                userLogins, true);

            if (projectId == -1)
                return;

            Projects.Add(new ProjectView
            {
                Id          = projectId,
                ImageSource = "Resources/shared.png",
                Name        = ProjectNameTextBox.Text
            });

            CancelButton_OnClick(null, null);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewProjectStackPanel.Visibility = Visibility.Collapsed;
            AddProjectButton.Visibility     = Visibility.Visible;

            ProjectNameTextBox.Text  = "";
            InvitedUsersTextBox.Text = "";
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            var item       = ((FrameworkElement) sender).DataContext;
            var invitation = Invitations[InvitationsListView.Items.IndexOf(item)];

            if (ProjectService.AcceptInvitation(invitation))
            {
                Invitations.Remove(invitation);

                if (Invitations.Count == 0)
                    InvitationsListView.Visibility = Visibility.Collapsed;

                Projects.Add(new ProjectView
                {
                    Id          = invitation.ProjectId,
                    Name        = invitation.ProjectName,
                    ImageSource = "Resources/shared.png"
                });

                TagService.GetInstance().RefreshRepositories();
            }
            else
                MessageBox.Show("Can't accept invitation");
        }

        private void DeclineButton_OnClick(object sender, RoutedEventArgs e)
        {
            var item       = ((FrameworkElement) sender).DataContext;
            var invitation = Invitations[InvitationsListView.Items.IndexOf(item)];

            if (ProjectService.DeclineInvitation(invitation))
            {
                Invitations.Remove(invitation);

                if (Invitations.Count == 0)
                    InvitationsListView.Visibility = Visibility.Collapsed;
            }
            else
                MessageBox.Show("Can't decline invitation");
        }

        public void RemoveProject()
        {
            Projects.RemoveAt(ProjectsListView.SelectedIndex);
        }
    }
}