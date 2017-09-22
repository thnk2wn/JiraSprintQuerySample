using System;
using System.Configuration;

namespace JIRASprintQuery.IO
{
    public static class AppSettings
    {
        public static int MaxIssuesPerRequest => Convert.ToInt32(
            ConfigurationManager.AppSettings["MaxIssuesPerRequest"]);

        public static string SprintNamePattern
            => ConfigurationManager.AppSettings["SprintNamePattern"];

        public static string TargetProjectKey
            => ConfigurationManager.AppSettings["TargetProjectKey"];
    }
}
