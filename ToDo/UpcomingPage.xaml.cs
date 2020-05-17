using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Core.Models;
using Core.Services;
using ToDo.Classes;

namespace ToDo
{
    public partial class UpcomingPage
    {
        private readonly List<UpcomingToDoItems> UpcomingItemsCollection;
        private readonly ToDoItemService ItemService;
        private readonly TagService TagService;
        private readonly MainWindow MainWindow;

        public UpcomingPage(MainWindow window)
        {
            InitializeComponent();

            UpcomingItemsCollection = new List<UpcomingToDoItems>();
            ItemService = ToDoItemService.GetInstance();
            TagService = TagService.GetInstance();
            MainWindow = window;

            FillCollection();

            UpcomingListView.ItemsSource = UpcomingItemsCollection;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var itemWindow = new ToDoItemWindow(null);

            itemWindow.ShowDialog(DateTime.Today.AddDays(1));

            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.ProjectId = null;

            ItemService.Add(itemWindow.Item);

            if (itemWindow.Item.Id == -1)
                return;

            TagService.ReplaceItemsTags(itemWindow.Item.Id, itemWindow.SelectedTagsId);

            MainWindow.UpdateUpcomingPage();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView) e.Source;

            if (listView.SelectedIndex == -1) return;

            var item = (ToDoItemModel) listView.SelectedItem;
            var itemWindow = new ToDoItemWindow(item.ProjectId, item);

            listView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                ItemService.Remove(item);
                MainWindow.UpdateUpcomingPage();

                return;
            }

            // Close window
            if (itemWindow.DialogResult == false) return;

            ItemService.Update(itemWindow.Item);
            TagService.ReplaceItemsTags(itemWindow.Item.Id, itemWindow.SelectedTagsId);
            MainWindow.UpdateUpcomingPage();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement) sender).DataContext;
            var toDoItem = (ToDoItemModel) item;
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 300)
            };

            timer.Tick += Timer_OnTick;
            timer.Tag = toDoItem;

            toDoItem.Timer = timer;

            timer.Start();
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement) sender).DataContext;
            var toDoItem = (ToDoItemModel) item;

            toDoItem.Timer.Stop();
        }

        private void Timer_OnTick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer) sender;
            var toDoItem = (ToDoItemModel) timer.Tag;

            toDoItem.CompleteDay = DateTime.UtcNow;

            ItemService.Update(toDoItem);

            toDoItem.Timer.Stop();

            MainWindow.UpdateUpcomingPage();
        }

        private void FillCollection()
        {
            var allItems = ItemService.Get(i => i.CompleteDay == DateTime.MinValue.AddYears(DateTime.Now.Year)).ToList();

            for (var i = 0; i <= 6; i++)
            {
                UpcomingItemsCollection.Add(new UpcomingToDoItems(ref i, allItems, true));
            }

            var iterationsCount = DateTime.Now.AddDays(8).Month != DateTime.Now.Month ? 6 : 5;

            for (var i = 0; i < iterationsCount; i++)
            {
                UpcomingItemsCollection.Add(new UpcomingToDoItems(ref i, allItems, false));
            }

            var yearsItems = allItems
                .Where(i => i.Date > DateTime.Today.AddMonths(5))
                .ToDictionary(item => item.Date.Year, item => allItems.Where(i => i.Date.Year == item.Date.Year));

            foreach (var i in yearsItems)
            {
                UpcomingItemsCollection.Add(new UpcomingToDoItems(i.Key, i.Value.ToList()));
            }
        }
    }
}