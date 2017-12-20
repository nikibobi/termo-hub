using System.Collections.Generic;

namespace TermoHub.Authorization
{
    public static class Role
    {
        public const string User = "User";
        public const string Admin = "Admin";

        private static string[] roles = new[] { User, Admin };

        public static IEnumerable<string> All => roles;
    }
}
