using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.Services;
using ToDo.Classes;

namespace ToDo
{
    public partial class TodayPage
    {
        private readonly ObservableCollection<ToDoItemView> ToDoItemsCollection;
        private readonly TodayToDoItemOperations ToDoItemOperations;
        private readonly ToDoItemService Service;

        public TodayPage()
        {
            InitializeComponent();

            ToDoItemsCollection = new ObservableCollection<ToDoItemView>();
            Service = ToDoItemService.GetInstance();

            ToDoItemOperations = new TodayToDoItemOperations(ToDoItemsListView, ToDoItemsCollection, null);

            FillCollection();

            ToDoItemsListView.ItemsSource = ToDoItemsCollection;
        }

        private void FillCollection()
        {
            var minDate = DateTime.MinValue.AddYears(DateTime.Now.Year);

            var collection = Service.Get(item => (item.Date <= DateTime.Today && item.Date != minDate ||
                                                   item.Deadline <= DateTime.Today && item.Deadline != minDate) &&
                                                  item.CompleteDay == minDate).ToList();

            foreach (var i in collection)
            {
                ToDoItemsCollection.Add(ToDoItemOperations.ConvertToItemView(i));
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToDoItemOperations.Add();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToDoItemOperations.Selected();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            ToDoItemOperations.Checked(sender);
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ToDoItemOperations.Unchecked(sender);
        }
    }
}