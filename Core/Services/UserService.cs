using System.Linq;
using System.Windows.Forms;
using Data.Entities;
using Data.Repositories;

namespace Core.Services
{
    public class UserService : Service
    {
        private readonly IRepository<User> Repository;
        private static readonly UserService ThisUserService;

        static UserService()
        {
            ThisUserService = new UserService();
        }

        private UserService()
        {
            Repository = new ToDoRepository<User>();
        }

        public void RefreshRepositories()
        {
            Repository.Refresh();
        }

        public static UserService GetInstance()
        {
            return ThisUserService;
        }

        public bool Add(string login, string password)
        {
            if (Repository.GetByPredicate(i => i.Login == login).Any())
            {
                MessageBox.Show("This login already exists. Try another");
                return false;
            }

            return Repository.Add(new User
            {
                Login    = login,
                Password = password
            });
        }

        public int GetUserId(string login, string password)
        {
            return Repository.GetByPredicate(i => i.Login == login && i.Password == password)
                       .FirstOrDefault()?.Id ?? -1;
        }
    }
}