using Data;

namespace Core.Services
{
    public abstract class Service
    {
        public static void RefreshContext()
        {
            ContextSingleton.RefreshContext();
        }
    }
}