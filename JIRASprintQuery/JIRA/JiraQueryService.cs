using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using Serilog;
using JIRASprintQuery.IO;

namespace JIRASprintQuery.JIRA
{
    public class JiraQueryService
    {
        private readonly Jira _jira;
        private readonly ILogger _logger;
        private readonly IIssueTicketConverter _issueTicketConverter;

        public JiraQueryService(ILogger logger, string username, string password, string jiraServer)
        {
            _logger = logger;
            _jira = JiraClientFactory.Create(username, password, jiraServer);

#if !SCRUB_DATA
            _issueTicketConverter = new IssueTicketConverter(_logger);
#else
            _issueTicketConverter = new IssueTicketScrubConverter(_logger);
#endif
        }

        public async Task<SprintDetails> QuerySprintTickets(string sprintName)
        {
            var project = await GetTargetProject();
            _logger.Information("Querying issues for {sprintName}", sprintName);

            var allIssues = _jira.Issues.Queryable
                .Where(i => i.Project == project.Name
                            && i["Sprint"] == new LiteralMatch(sprintName))
                .ToList();

            // we could filter in server call but we might analyze tasks later.
            var nonSubTaskIssues = allIssues.Where(
                i => i.Type.Name != "Sub-task").ToList();

            _logger.Information("Found {allIssuesCount} issues, " +
                                "{nonSubTaskIssuesCount} non-subtask issues.",
                                allIssues.Count, nonSubTaskIssues.Count);

            var details = new SprintDetails { Tickets = new List<SprintTicket>() };

            foreach (var issue in nonSubTaskIssues)
            {
                var ticket = await CreateSprintTicket(issue);
                details.Tickets.Add(ticket);
            }

            details.Summary = new SprintSummary
            {
                ItemsThisSprint = details.Tickets.Count(t => t.Sprints == 1),
                ItemsPriorSprint = details.Tickets.Count(t => t.Sprints > 1),
                TotalPoints = details.Tickets.Sum(t => t.Points),
                TotalDonePoints = details.Tickets.Where(t => t.IsDone).Sum(t => t.Points),
                ItemsDone = details.Tickets.Count(t => t.IsDone),
                BugsCreated = details.Tickets.Count(t => t.Type == "Bug" && t.Sprints == 1),
                BugsAddressed = details.Tickets.Count(t => t.Type == "Bug" && t.IsDone)
            };

            details.Summary.ItemsRemaining = details.Tickets.Count - details.Summary.ItemsDone;

            return details;
        }

        private async Task<Project> GetTargetProject()
        {
            _logger.Information("Querying JIRA project");
            var project = await _jira.Projects.GetProjectAsync(AppSettings.TargetProjectKey);
            return project;
        }

        private async Task<SprintTicket> CreateSprintTicket(Issue issue)
        {
            var ticket = await _issueTicketConverter.CreateSprintTicket(issue);
            return ticket;
        }
    }
}
