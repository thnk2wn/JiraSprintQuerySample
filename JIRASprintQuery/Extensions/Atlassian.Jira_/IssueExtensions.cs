using Atlassian.Jira;
using System.Linq;

namespace JIRASprintQuery.Extensions.Atlassian.Jira_
{
    public static class IssueExtensions
    {
        public static CustomFieldValue GetCustomField(this Issue issue, string key)
        {
            return issue.CustomFields.Any(f => f.Name == key)
                ? issue.CustomFields[key] : null;
        }
    }
}
