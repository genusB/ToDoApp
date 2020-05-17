using System.Data.Entity;
using Data.Entities;

namespace Data
{
    public class ToDoContext : DbContext
    {
        public ToDoContext() : base("ToDoDB")
        {
        }
        
        public DbSet<ToDoItem> ToDoItems { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ItemTag> ItemsTags { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectsUsers> ProjectsUsers { get; set; }
    }
}