using HRM_WPF_CNPM.Models;

namespace HRM_WPF_CNPM.Helpers
{
    public static class UserSession
    {
        public static User? CurrentUser { get; set; }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn => CurrentUser != null;
    }
}
