using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using Data.Entities;
using Data.Repositories;

namespace Core.Services
{
    public class ToDoItemService : Service
    {
        private readonly IRepository<ProjectsUsers> ProjectsUsersRepository;
        private readonly IRepository<ToDoItem> ToDoItemRepositoty;
        private readonly int UserId;
        private static ToDoItemService ThisToDoItemService ;
        private readonly TagService TagService;

        private ToDoItemService(int userId)
        {
            ProjectsUsersRepository = new ToDoRepository<ProjectsUsers>();
            ToDoItemRepositoty = new ToDoRepository<ToDoItem>();
            UserId = userId;

            TagService = TagService.GetInstance();
        }

        public void RefreshRepositories()
        {
            ToDoItemRepositoty.Refresh();
            ProjectsUsersRepository.Refresh();
        }

        public static void Initialize(int userId)
        {
            ThisToDoItemService = new ToDoItemService(userId);
        }

        public static ToDoItemService GetInstance()
        {
            return ThisToDoItemService ;
        }

        public void Add(ToDoItemModel item)
        {
            var addingItem = new ToDoItem
            {
                Header = item.Header,
                Notes = item.Notes,
                Date = item.Date,
                Deadline = item.Deadline,
                CompleteDate = item.CompleteDay,
                UserId = this.UserId,
                ProjectId = item.ProjectId
            };

            item.Id = ToDoItemRepositoty.Add(addingItem) ? addingItem.Id : -1;
        }

        public bool Remove(ToDoItemModel item)
        {
            var foundItem = ToDoItemRepositoty.GetByPredicate(i => i.Id == item.Id).First();

            return TagService.RemoveTagsFromTask(foundItem.Id) && ToDoItemRepositoty.Remove(foundItem);
        }

        public void Update(ToDoItemModel item)
        {
            var foundItem = ToDoItemRepositoty.GetByPredicate(i => i.Id == item.Id).First();

            foundItem.Header = item.Header;
            foundItem.Notes = item.Notes;
            foundItem.Date  = item.Date;
            foundItem.Deadline = item.Deadline;
            foundItem.CompleteDate = item.CompleteDay;

            ToDoItemRepositoty.SaveChanges();
        }

        public IEnumerable<ToDoItemModel> Get(Func<ToDoItemModel, bool> predicate)
        {
            var projectsUsers = ProjectsUsersRepository.GetByPredicate(i => i.UserId == UserId && i.IsAccepted);

            return ToDoItemRepositoty.GetByPredicate(i => i.UserId == UserId && i.ProjectId == null ||
                                                   projectsUsers.Any(p => i.ProjectId == p.ProjectId))
                .Select(i => new ToDoItemModel
                {
                    Id = i.Id,
                    Header = i.Header,
                    Notes = i.Notes,
                    Date = i.Date,
                    Deadline = i.Deadline,
                    CompleteDay = i.CompleteDate,
                    ProjectName = i.ProjectId != null ? $"Project : {i.ProjectOf.Name}" : "",
                    ProjectId  = i.ProjectId
                })
                .Where(predicate);
        }

        public IEnumerable<ToDoItemModel> GetSharedProjectItems(int projectId)
        {
            var minDate = DateTime.MinValue.AddYears(DateTime.Now.Year);

            return ToDoItemRepositoty
                .GetByPredicate(i => i.ProjectId == projectId && i.CompleteDate == minDate)
                .Select(i => new ToDoItemModel
                {
                    Id = i.Id,
                    Header = i.Header,
                    Notes = i.Notes,
                    Date = i.Date,
                    Deadline = i.Deadline,
                    CompleteDay = i.CompleteDate,
                    ProjectId  = i.ProjectId,
                    ProjectName = i.ProjectId != null ? $"Project : {i.ProjectOf.Name}" : ""
                });
        }
    }
}