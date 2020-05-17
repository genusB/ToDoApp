using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.Models;
using Core.Services;

namespace ToDo
{
    public partial class LogbookPage
    {
        private readonly ObservableCollection<LogbookToDoItem> ToDoItemsCollection;
        private readonly ToDoItemService ItemService;
        private readonly TagService TagService;
        private readonly MainWindow MainWindow;
        private readonly DateTime MinDate;

        public LogbookPage(MainWindow window)
        {
            InitializeComponent();

            ToDoItemsCollection = new ObservableCollection<LogbookToDoItem>();
            ItemService = ToDoItemService.GetInstance();
            TagService = TagService.GetInstance();
            MainWindow  = window;
            MinDate = DateTime.MinValue.AddYears(DateTime.Now.Year);

            FillCollection();

            ToDoItemsListView.ItemsSource = ToDoItemsCollection;
        }

        private class LogbookToDoItem : ToDoItemModel
        {
            public string CompleteDateStr { get; set; }
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ToDoItemsListView.SelectedIndex;

            if (index == -1) return;

            var item  = ToDoItemsCollection[index];
            var itemWindow = new ToDoItemWindow(item.ProjectId, item);

            ToDoItemsListView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                ItemService.Remove(item);
                MainWindow.UpdateLogbookPage();

                return;
            }

            // Closed window
            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.CompleteDay = item.CompleteDay;

            ItemService.Update(itemWindow.Item);
            TagService.ReplaceItemsTags(itemWindow.Item.Id, itemWindow.SelectedTagsId);
            MainWindow.UpdateLogbookPage();
        }

        private void FillCollection()
        {
            var items = ItemService.Get(i => i.CompleteDay != MinDate).ToList();

            items.Sort((a, b) => -a.CompleteDay.CompareTo(b.CompleteDay));

            foreach (var i in items)
            {
                ToDoItemsCollection.Add(new LogbookToDoItem
                {
                    Id = i.Id,
                    Header = i.Header,
                    Notes = i.Notes,
                    Date = i.Date,
                    Deadline = i.Deadline,
                    CompleteDay = i.CompleteDay,
                    CompleteDateStr = i.CompleteDay.ToShortDateString(),
                    ProjectName = i.ProjectName,
                    ProjectId = i.ProjectId
                });
            }
        }

        // Recover ToDoItem from Logbook page.
        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("You've already done this task. " +
                                         "Are you sure you want to make active?",
                "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                ((CheckBox) sender).IsChecked = true;
                return;
            }

            var selectedItem = ((FrameworkElement) sender).DataContext;
            var index = ToDoItemsListView.Items.IndexOf(selectedItem);
            var toDoItem = ToDoItemsCollection[index];

            toDoItem.CompleteDay = MinDate;

            ItemService.Update(toDoItem);
            ToDoItemsCollection.RemoveAt(index);
        }
    }
}