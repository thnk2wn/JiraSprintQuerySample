using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlassian.Jira;
using Serilog;
using JIRASprintQuery.Extensions.Atlassian.Jira_;
using RandomNameGeneratorLibrary;

namespace JIRASprintQuery.JIRA
{
    public class IssueTicketScrubConverter : IIssueTicketConverter
    {
        private readonly ILogger _logger;
        private readonly PersonNameGenerator _personNameGenerator;
        private readonly Dictionary<string, string> _epicMap;

        private static readonly List<string> _epics = new List<string>
        {
            "Ordering", "Tests", "Payments", "Personalization", "Security", "Discovery"
        };

        private readonly List<string> _assigneePeople;
        private readonly List<string> _reportedByPeople;
        private static readonly HashSet<int> _usedIssueKeys = new HashSet<int>();

        public IssueTicketScrubConverter(ILogger logger)
        {
            _logger = logger;
            _personNameGenerator = new PersonNameGenerator();
            _epicMap = new Dictionary<string, string>();
            _assigneePeople = RandomPeeps(8);
            _reportedByPeople = RandomPeeps(4);
        }

        public async Task<SprintTicket> CreateSprintTicket(Issue issue)
        {
            var issueKey = IssueId(issue);
            _logger.Information("Parsing {id} - {name}", issueKey.Id, issueKey.Name);

            var ticket = new SprintTicket
            {
                Id = issueKey.Id,
                Name = issueKey.Name,
                Assignee = _assigneePeople[LoremNET.RandomHelper.Instance.Next(0, _assigneePeople.Count - 1)],
                Created = issue.Created.GetValueOrDefault(),
                Updated = issue.Updated.GetValueOrDefault(),
                ReportedBy = _reportedByPeople[LoremNET.RandomHelper.Instance.Next(0, _reportedByPeople.Count - 1)],
                Status = issue.Status.Name,
                Type = issue.Type.Name,
                Priority = issue.Priority.Name,
                Sprints = 1,
                Url = $"https://project.atlassian.net/browse/{issueKey.Id}"
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

                // simulate fetching issue, keeping same epic name for same key
                if (!_epicMap.ContainsKey(epicKey))
                {
                    _logger.Information("Querying epic issue {epicKey}", $"XY-{epicKey.Substring(3)}");
                    var epicName = _epics[LoremNET.RandomHelper.Instance.Next(0, _epics.Count - 1)];
                    _epicMap.Add(epicKey, epicName);
                }

                ticket.Epic = _epicMap[epicKey];
            }

            return await Task.Run(() => ticket);
        }

        private (string Id, string Name) IssueId(Issue issue)
        {
            var name = LoremNET.Lorem.Words(8, 14);
            int idNumber;

            do 
            {
                idNumber = LoremNET.RandomHelper.Instance.Next(100, 700);
            } while (_usedIssueKeys.Contains(idNumber));

            _usedIssueKeys.Add(idNumber);

            return ($"XY-{idNumber}", name);
        }

        private List<string> RandomPeeps(int count)
        {
            var list = new List<string>();

            do  
            {
                var name = _personNameGenerator.GenerateRandomFirstName();
                if (!list.Contains(name)) list.Add(name);
            } while (list.Count < count);            

            return list;
        }
    }
}
