using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Core.Models;
using Data.Entities;
using Data.Repositories;

namespace Core.Services
{
    public class ProjectService : Service
    {
        private readonly IRepository<User> UserRepository;
        private readonly IRepository<Project> ProjectRepository;
        private readonly IRepository<ProjectsUsers> ProjectsUsersRepository;
        private readonly IRepository<ToDoItem> ToDoItemRepositoty;
        private readonly int UserId;
        private static ProjectService ThisProjectService;
        private readonly TagService TagService;

        private ProjectService(int userId)
        {
            UserRepository = new ToDoRepository<User>();
            ProjectRepository = new ToDoRepository<Project>();
            ProjectsUsersRepository = new ToDoRepository<ProjectsUsers>();
            ToDoItemRepositoty = new ToDoRepository<ToDoItem>();
            UserId = userId;
            TagService = TagService.GetInstance();
        }

        public void RefreshRepositories()
        {
            ToDoItemRepositoty.Refresh();
            UserRepository.Refresh();
            ProjectRepository.Refresh();
            ProjectsUsersRepository.Refresh();
        }

        public static void Initialize(int userId)
        {
            ThisProjectService = new ProjectService(userId);
        }

        public static ProjectService GetInstance()
        {
            return ThisProjectService;
        }

        public int InviteUsers(ProjectModel project, IEnumerable<string> userLogins, bool isNewProject)
        {
            var users = UserRepository.Get().ToList();
            var logins = userLogins.ToList();

            if (!logins.All(i => users.Any(user => user.Login == i)))
            {
                MessageBox.Show("At least one login doesn't exist");
                return -1;
            }

            if (!isNewProject)
            {
                AddInvitedUsers(logins, users, project.Id);
                return project.Id;
            }

            if (logins.Contains(users.First(user => user.Id == UserId).Login))
            {
                MessageBox.Show("You are already there, invite somebody else");
                return -1;
            }

            var newProject = new Project {Name = project.Name};

            if (!ProjectRepository.Add(newProject))
            {
                return -1;
            }

            // Adding creator to project
            ProjectsUsersRepository.Add(new ProjectsUsers
            {
                ProjectId = newProject.Id,
                InviterId = UserId,
                UserId = UserId,
                IsAccepted = true
            });

            return AddInvitedUsers(logins, users, newProject.Id) ? newProject.Id : -1;
        }

        private bool AddInvitedUsers(IEnumerable<string> logins, List<User> users, int projectId)
        {
            var allProjects = ProjectsUsersRepository.Get().ToList();

            foreach (var login in logins)
            {
                // This user is already invited.
                if (allProjects.Any(i => i.UserOf.Login == login && i.ProjectId == projectId))
                {
                    continue;
                }

                if (!ProjectsUsersRepository.Add(new ProjectsUsers
                {
                    ProjectId  = projectId,
                    InviterId  = UserId,
                    UserId = users.Find(user => user.Login == login).Id,
                    IsAccepted = false
                }))
                {
                    return false;
                }
            }

            return true;
        }

        public bool AcceptInvitation(InvitationRequestModel invitation)
        {
            var invite = ProjectsUsersRepository
                .GetByPredicate(i => i.ProjectId == invitation.ProjectId && i.UserId == UserId)
                .FirstOrDefault();

            if (invite == null)
                return false;

            invite.IsAccepted = true;

            ProjectsUsersRepository.SaveChanges();

            return true;
        }

        public bool DeclineInvitation(InvitationRequestModel invitation)
        {
            var invite = ProjectsUsersRepository
                .GetByPredicate(i => i.ProjectId == invitation.ProjectId && i.UserId == UserId)
                .First();

            return ProjectsUsersRepository.Remove(invite);
        }

        public void LeaveProject(int projectId)
        {
            var record = ProjectsUsersRepository
                .GetByPredicate(i => i.ProjectId == projectId && i.UserId == UserId)
                .First();

            //Delete users from this project
            ProjectsUsersRepository.Remove(record);

            if (ProjectsUsersRepository.GetByPredicate(i => i.ProjectId == projectId).Any())
                return;

            var projectItems = ToDoItemRepositoty.GetByPredicate(i => i.ProjectId == projectId).ToList();

            //Delete all associated tags
            foreach (var i in projectItems)
            {
                if (!TagService.RemoveTagsFromTask(i.Id) ||
                    !TagService.RemoveSharedTags(projectId))
                {
                    MessageBox.Show("Can't delete tags from a task");
                    return;
                }

                ToDoItemRepositoty.Remove(i);
            }

            ProjectRepository.Remove(ProjectRepository.GetByPredicate(i => i.Id == projectId).First());
        }

        public IEnumerable<ProjectModel> GetProjects()
        {
            return ProjectsUsersRepository
                .GetByPredicate(i => i.UserId == UserId && i.IsAccepted)
                .ToList()
                .Select(i => new ProjectModel
                {
                    Id = i.ProjectId,
                    Name = i.ProjectOf.Name
                });
        }

        public IEnumerable<InvitationRequestModel> GetInvitations()
        {
            return ProjectsUsersRepository
                .GetByPredicate(i => i.UserId == UserId && !i.IsAccepted)
                .ToList()
                .Select(i =>
                    new InvitationRequestModel
                    {
                        InviterName = i.InviterOf.Login,
                        ProjectId = i.ProjectId,
                        ProjectName = i.ProjectOf.Name
                    });
        }

        public IEnumerable<string> GetProjectMembers(int projectId)
        {
            return ProjectsUsersRepository
                .GetByPredicate(i => i.ProjectId == projectId && i.IsAccepted).ToList()
                .Select(i => i.UserOf.Login);
        }
    }
}