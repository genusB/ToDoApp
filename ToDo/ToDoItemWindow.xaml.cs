using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.Models;
using Core.Services;
using ToDo.Classes;

namespace ToDo
{
    public partial class ToDoItemWindow
    {
        private readonly DateTime MinDate = DateTime.MinValue.AddYears(DateTime.Now.Year);
        private readonly ObservableCollection<TagModel> TagsList;
        private readonly TagService TagsService;
        private readonly int? ProjectId;
        public IEnumerable<int> SelectedTagsId { get; set; }

        public ToDoItemView Item { get; }
        public bool ToDelete { get; private set; }

        public ToDoItemWindow(int? projectId)
        {
            InitializeComponent();

            if (projectId != null)
            {
                SharedTagButton.Visibility = Visibility.Visible;
                Height = 465;
            }

            ProjectId = projectId;

            PickedDate.DisplayDateStart = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;
            Item = new ToDoItemView();
            ToDelete = false;

            TagsService = TagService.GetInstance();
            TagsList = new ObservableCollection<TagModel>();


            TagsListBox.Items.Clear();
            FillTagsCollection();
            TagsListBox.ItemsSource = TagsList;
        }

        public void ShowDialog(DateTime dateTime)
        {
            PickedDate.SelectedDate = dateTime;

            ShowDialog();
        }

        public ToDoItemWindow(int? projectId, ToDoItemModel item) : this(projectId)
        {
            Item = new ToDoItemView {Id = item.Id};

            HeaderText.Text = item.Header;
            NotesText.Text = item.Notes;

            if (item.Date != MinDate)
                PickedDate.SelectedDate = item.Date;

            if (item.Deadline != MinDate)
                PickedDeadline.SelectedDate = item.Deadline;

            PickedDate.DisplayDateStart = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;

            SelectTags();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HeaderText.Text))
            {
                MessageBox.Show("Fill header");
                return;
            }

            Item.Header = HeaderText.Text;
            Item.Notes = NotesText.Text;

            if (PickedDate.SelectedDate != null)
                Item.Date = (DateTime) PickedDate.SelectedDate;
            else
                Item.Date = MinDate;

            if (PickedDeadline.SelectedDate != null)
                Item.Deadline = (DateTime) PickedDeadline.SelectedDate;
            else
                Item.Deadline = MinDate;

            Item.CompleteDay = MinDate;

            if (Item.Deadline <= DateTime.Today && Item.Deadline != MinDate)
            {
                Item.DeadlineColor = "Red";
                Item.DeadlineShort = "today";
            }
            else if (Item.Deadline != MinDate)
            {
                var remainingDays = (Item.Deadline - DateTime.Today).TotalDays;

                Item.DeadlineColor = "Gray";
                Item.DeadlineShort = $"{remainingDays}d left";
            }

            SelectedTagsId = GetTagsId();

            DialogResult = true;
            Close();
        }

        private IEnumerable<int> GetTagsId()
        {
            return from TagModel i in TagsListBox.SelectedItems select i.Id;
        }

        private void DeleteTaskButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this task?", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            ToDelete = true;

            Close();
        }

        private void FillTagsCollection()
        {
            // If projectId is null, getting only tags, that don't belong to any project.
            var collection = ProjectId != null
                ? TagsService.Get(i => i.ProjectId == ProjectId || i.ProjectId == null).ToList()
                : TagsService.Get(i => i.ProjectId == null).ToList();

            foreach (var i in collection)
            {
                TagsList.Add(i);
            }
        }

        private void SelectTags()
        {
            var collection = TagsService.GetSelected(Item.Id).ToList();

            foreach (var i in collection)
            {
                TagsListBox.SelectedItems.Add(i);
            }
        }

        private void TagsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagsListBox.SelectedIndex == -1)
            {
                EditButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;

                Height = ProjectId == null ? 420 : 465;
            }
            else
            {
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;

                Height = ProjectId == null ? 500 : 555;
            }
        }

        private void AddTagButton_OnCLick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();

            if (window.ShowDialog() == false) return;

            var tag = new TagModel
            {
                Text = window.NewText,
                TagTextColor = "Black"
            };

            TagsService.Add(tag);
            TagsList.Add(tag);
        }

        private void AddSharedTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();

            if (window.ShowDialog() == false) return;

            var tag = new TagModel
            {
                Text = window.NewText,
                ProjectId = ProjectId,
                TagTextColor = "#6B6B6B"
            };

            TagsService.Add(tag);
            TagsList.Add(tag);
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();
            var tag = (TagModel) TagsListBox.SelectedItems[TagsListBox.SelectedItems.Count - 1];

            if (window.ShowDialog() == false) return;

            tag.Text = window.NewText;

            TagsList[TagsListBox.SelectedIndex].Text = tag.Text;
            TagsService.Update(tag);
        }

        private void DeleteTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this tag? This will also delete" +
                                "this tag from all tasks.", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            var tag = TagsList[TagsListBox.SelectedIndex];

            TagsList.Remove(tag);

            if (TagsService.Remove(tag))
                TagsList.Remove(tag);
        }
    }
}