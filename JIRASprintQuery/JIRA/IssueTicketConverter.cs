using Atlassian.Jira;
using JIRASprintQuery.Extensions.Atlassian.Jira_;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JIRASprintQuery.JIRA
{
    public class IssueTicketConverter : IIssueTicketConverter
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _epicMap;

        public IssueTicketConverter(ILogger logger)
        {
            _logger = logger;
            _epicMap = new Dictionary<string, string>();
        }

        public async Task<SprintTicket> CreateSprintTicket(Issue issue)
        {
            _logger.Information("Parsing {id} - {name}", issue.Key.Value, issue.Summary);

            var ticket = new SprintTicket
            {
                Assignee = NameOnly(issue.Assignee),
                Created = issue.Created.GetValueOrDefault(),
                Updated = issue.Updated.GetValueOrDefault(),
                Id = issue.Key.Value,
                Name = issue.Summary,
                Status = issue.Status.Name,
                Type = issue.Type.Name,
                ReportedBy = NameOnly(issue.Reporter),
                Priority = issue.Priority.Name,
                Sprints = 1,
                Url = $"{issue.Jira.Url}browse/{issue.Key.Value}"
            };

            if (string.IsNullOrEmpty(ticket.Assignee))
                ticket.Assignee = "Unassigned";

            var storyPointsField = issue.GetCustomField("Story Points");

            if (storyPointsField != null)
            {
                ticket.Points = storyPointsField.Values.Length > 0
                    ? Convert.ToInt32(storyPointsField.Values[0]) : 0;
            }

            var sprintField = issue.GetCustomField("Sprint");

            if (sprintField != null)
            {
                ticket.Sprints = sprintField.Values.Length;
            }

            var epicLinkField = issue.GetCustomField("Epic Link");

            if (epicLinkField != null && epicLinkField.Values.Length > 0)
            {
                var epicKey = epicLinkField.Values[0];

                // querying issues can be slow - don't fetch same epic issue more than once
                if (!_epicMap.ContainsKey(epicKey))
                {
                    _logger.Information("Querying epic issue {epicKey}", epicKey);
                    var epicIssue = await issue.Jira.Issues.GetIssueAsync(epicKey);
                    _epicMap.Add(epicKey, epicIssue.CustomFields["Epic Name"].Values[0]);
                }

                ticket.Epic = _epicMap[epicKey];
            }

            return ticket;
        }

        private static string NameOnly(string nameOrEmail)
        {
            var pos = nameOrEmail?.LastIndexOf("@") ?? -1;
            var nameOnly = pos > -1
                ? nameOrEmail.Substring(0, pos - 1)
                : nameOrEmail;
            return nameOnly;
        }
    }
}
