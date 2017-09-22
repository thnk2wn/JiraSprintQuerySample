using Atlassian.Jira;
using JIRASprintQuery.IO;

namespace JIRASprintQuery.JIRA
{
    public static class JiraClientFactory
    {
        public static Jira Create(string username, string password, string jiraServer)
        {
            var jira = Jira.CreateRestClient(jiraServer, username, password);
            jira.Issues.MaxIssuesPerRequest = AppSettings.MaxIssuesPerRequest;
            return jira;
        }
    }
}
