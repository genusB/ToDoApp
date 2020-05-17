using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using Data.Entities;
using Data.Repositories;

namespace Core.Services
{
    public class TagService : Service
    {
        private readonly IRepository<ProjectsUsers> ProjectsUsersRepository;
        private readonly IRepository<Tag> TagRepository;
        private readonly IRepository<ItemTag> ItemTagRepository;
        private readonly int UserId;
        private List<ProjectsUsers> ProjectsUsers;
        private List<Tag> Tags;
        private List<ItemTag> ItemTags;
        private static TagService ThisTagService;

        private TagService(int UserId)
        {
            ProjectsUsersRepository = new ToDoRepository<ProjectsUsers>();
            TagRepository = new ToDoRepository<Tag>();
            ItemTagRepository = new ToDoRepository<ItemTag>();
            this.UserId  = UserId;

            FillLists();
        }

        public void RefreshRepositories()
        {
            ProjectsUsersRepository.Refresh();
            TagRepository.Refresh();
            ItemTagRepository.Refresh();

            FillLists();
        }

        public static void Initialize(int userId)
        {
            ThisTagService = new TagService(userId);
        }

        public static TagService GetInstance()
        {
            return ThisTagService;
        }

        public void Add(TagModel tag)
        {
            var addingTag = new Tag
            {
                Text = tag.Text,
                UserId = UserId,
                ProjectId = tag.ProjectId
            };

            tag.Id = TagRepository.Add(addingTag) ? addingTag.Id : -1;
            Tags.Add(addingTag);
        }

        public bool Remove(TagModel tag)
        {
            var foundTag = TagRepository.GetByPredicate(i => i.Id == tag.Id).First();

            DeleteTagFromItemsTags(foundTag.Id);

            var deleteRes = TagRepository.Remove(foundTag);

            if (deleteRes)
                Tags.Remove(foundTag);

            return deleteRes;
        }

        public bool RemoveSharedTags(int projectId)
        {
            if (Tags.Where(i => i.ProjectId == projectId).Any(i => !TagRepository.Remove(i)))
            {
                return false;
            }

            Tags = FilterTags(TagRepository.Get().ToList());

            return true;
        }

        public bool RemoveTagsFromTask(int taskId)
        {
            if (ItemTags.Where(i => i.ItemId == taskId).Any(i => !ItemTagRepository.Remove(i)))
            {
                return false;
            }

            ItemTags = FilterTags(ItemTagRepository.Get().ToList());

            return true;
        }

        public void Update(TagModel tag)
        {
            var foundTag = TagRepository.Get().First(i => i.Id == tag.Id);

            Tags[Tags.IndexOf(foundTag)].Text = tag.Text;

            foundTag.Text = tag.Text;

            TagRepository.SaveChanges();
        }

        public void ReplaceItemsTags(int itemId, IEnumerable<int> tagId)
        {
            var itemsTags = ItemTags.Where(i => i.ItemId == itemId).ToList();

            foreach (var i in itemsTags)
            {
                ItemTagRepository.Remove(i);
            }

            foreach (var i in tagId)
            {
                ItemTagRepository.Add(new ItemTag
                {
                    ItemId = itemId,
                    TagId  = i
                });
            }

            ItemTags = FilterTags(ItemTagRepository.Get().ToList());
        }

        public IEnumerable<TagModel> Get(Func<TagModel, bool> predicate)
        {
            return Tags
                .Select(i => new TagModel
                {
                    Id = i.Id,
                    Text = i.Text,
                    ProjectId = i.ProjectId,
                    TagTextColor = i.ProjectId != null ? "#6B6B6B" : "Black"
                })
                .Where(predicate);
        }

        public IEnumerable<TagModel> GetSelected(int itemId)
        {
            return ItemTags
                .Where(i => i.ItemId == itemId)
                .Select(i => new TagModel
                {
                    Id = i.TagOf.Id,
                    Text = i.TagOf.Text,
                    ProjectId = i.TagOf.ProjectId,
                    TagTextColor = i.TagOf.ProjectId != null ? "#6B6B6B" : "Black"
                });
        }

        public IEnumerable<ToDoItemModel> GetItemsByTags(IEnumerable<string> tags, List<string> projectNames)
        {
            var predicates = GetPredicates(projectNames);
            var allItems = ItemTags.Where(i => predicates.Any(p => p.Invoke(i))).ToList();
            var itemsCount = new Dictionary<ToDoItem, int>();
            var tagTexts = tags.ToList();

            foreach (var text in tagTexts)
            {
                foreach (var item in allItems)
                {
                    if (itemsCount.ContainsKey(item.ItemOf) && item.TagOf.Text == text)
                        itemsCount[item.ItemOf]++;
                    else if (item.TagOf.Text == text)
                        itemsCount[item.ItemOf] = 1;
                }
            }


            var foundItems = itemsCount.Where(i => i.Value == tagTexts.Count).Select(i => i.Key).ToList();

            return foundItems.Select(i => new ToDoItemModel
            {
                Id = i.Id,
                Header = i.Header,
                Notes = i.Notes,
                Date = i.Date,
                Deadline = i.Deadline,
                CompleteDay = i.CompleteDate,
                ProjectName = i.ProjectOf?.Name ?? "",
                ProjectId = i.ProjectId
            });
        }

        private void FillLists()
        {
            ProjectsUsers = FilterProjects(ProjectsUsersRepository.Get().ToList());
            Tags = FilterTags(TagRepository.Get().ToList());
            ItemTags = FilterTags(ItemTagRepository.Get().ToList());
        }

        private void DeleteTagFromItemsTags(int tagId)
        {
            var itemsTags = ItemTags.Where(i => i.TagId == tagId);

            foreach (var i in itemsTags)
            {
                ItemTagRepository.Remove(i);
            }

            ItemTags = FilterTags(ItemTagRepository.Get().ToList());
        }

        private static List<Predicate<ItemTag>> GetPredicates(IEnumerable<string> projectNames)
        {
            var minDate    = DateTime.MinValue.AddYears(DateTime.Now.Year);
            var predicates = new List<Predicate<ItemTag>>();

            foreach (var name in projectNames)
            {
                switch (name)
                {
                    case "Inbox":
                        predicates.Add(i => i.ItemOf.Date == minDate &&
                                            i.ItemOf.CompleteDate == minDate &&
                                            i.ItemOf.ProjectOf == null);
                        break;

                    case "Today":
                        predicates.Add(i => (i.ItemOf.Date <= DateTime.Today && i.ItemOf.Date != minDate ||
                                             i.ItemOf.Deadline <= DateTime.Today && i.ItemOf.Deadline != minDate) &&
                                            i.ItemOf.CompleteDate == minDate);
                        break;

                    case "Upcoming":
                        predicates.Add(i => i.ItemOf.Date > DateTime.Today && i.ItemOf.CompleteDate == minDate);
                        break;

                    case "Logbook":
                        predicates.Add(i => i.ItemOf.CompleteDate != minDate);
                        break;

                    default:
                        predicates.Add(i => i.ItemOf.ProjectOf != null && i.ItemOf.ProjectOf.Name == name);
                        break;
                }
            }

            return predicates;
        }

        private List<Tag> FilterTags(IEnumerable<Tag> allTags)
        {
            return allTags
                .Where(i => i.UserId == UserId ||
                            i.ProjectId != null && ProjectsUsers.Any(p => p.ProjectId == i.ProjectId))
                .ToList();
        }

        private List<ItemTag> FilterTags(IEnumerable<ItemTag> itemTags)
        {
            return itemTags
                .Where(i => i.TagOf.UserId == UserId ||
                            i.TagOf.ProjectId != null &&
                            ProjectsUsers.Any(p => p.ProjectId == i.TagOf.ProjectId))
                .ToList();
        }

        private List<ProjectsUsers> FilterProjects(IEnumerable<ProjectsUsers> projectsUsers)
        {
            return projectsUsers.Where(p => p.UserId == UserId &&  p.IsAccepted).ToList();
        }
    }
}