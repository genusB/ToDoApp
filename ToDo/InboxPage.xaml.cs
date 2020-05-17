using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.Services;
using ToDo.Classes;

namespace ToDo
{
    public partial class InboxPage
    {
        private readonly ObservableCollection<ToDoItemView> ToDoItemsCollection;
        private readonly ToDoItemService ItemService;
        private readonly ToDoItemOperations ItemOperations;

        public InboxPage()
        {
            InitializeComponent();

            ToDoItemsCollection = new ObservableCollection<ToDoItemView>();
            ItemService = ToDoItemService.GetInstance();
            ItemOperations = new InboxToDoItemOperations(ToDoItemsListView, ToDoItemsCollection, null);

            FillCollection();

            ToDoItemsListView.ItemsSource = ToDoItemsCollection;
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            ItemOperations.Checked(sender);
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ItemOperations.Unchecked(sender);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            ItemOperations.Add();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemOperations.Selected();
        }

        private void FillCollection()
        {
            var minDate = DateTime.MinValue.AddYears(DateTime.Now.Year);
            
            var collection = ItemService.Get(item =>
                item.Date == minDate &&
                item.CompleteDay == minDate &&
                item.ProjectName == "").ToList();

            foreach (var i in collection)
            {
                var item = ItemOperations.ConvertToItemView(i);

                if (item == null) continue;

                ToDoItemsCollection.Add(item);
            }
        }
    }
}