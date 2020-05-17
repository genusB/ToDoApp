using System.Data.Entity;

namespace Data
{
    public static class ContextSingleton
    {
        private static ToDoContext Context;

        static ContextSingleton()
        {
            Context = new ToDoContext();
        }

        public static void RefreshContext()
        {
            Context.Dispose();
            
            Context = new ToDoContext();
            
            Context.Tags.Load();
            Context.ToDoItems.Load();
            Context.Users.Load();
            Context.ItemsTags.Load();
            Context.Projects.Load();
            Context.ProjectsUsers.Load();
        }
        
        public static ToDoContext GetInstance()
        {
            return Context;
        }
    }
}
