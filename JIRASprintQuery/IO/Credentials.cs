using CredentialManagement;
using System.Reflection;

namespace JIRASprintQuery.IO
{
    public static class Credentials
    {
        private static string Target()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        public static void Save(string username, string password, string description)
        {
            using (var cred = new Credential())
            {
                cred.Username = username;
                cred.Password = password;
                cred.Description = description;
                cred.Target = Target();
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Save();
            }
        }

        public static (string User, string Pass, string Desc) Get()
        {
            using (var cred = new Credential())
            {
                cred.Target = Target();
                cred.Load();
                return (cred.Username, cred.Password, cred.Description);
            }
        }
    }
}
