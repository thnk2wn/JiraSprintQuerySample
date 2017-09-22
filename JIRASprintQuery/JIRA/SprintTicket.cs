using JIRASprintQuery.Extensions.System_;
using System;

namespace JIRASprintQuery.JIRA
{
    public class SprintTicket
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string Assignee { get; set; }

        public string Type { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string Epic { get; set; }

        public string ReportedBy { get; set; }

        public string Priority { get; set; }

        public int Points { get; set; }

        public int Sprints { get; set; }

        public bool IsDone => TicketStatus.IsComplete(Status);

        public string Url {get; set;}

        public string StatusRowText => $"{Id} {Name.Left(60)} " +
            $"({(!string.IsNullOrEmpty(Assignee) ? Assignee : "Unassigned")})";

        public string AssigneeRowText => $"{Id} {Name.Left(60)} ({Status})";
    }
}
