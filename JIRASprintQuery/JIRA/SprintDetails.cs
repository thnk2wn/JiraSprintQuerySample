using System.Collections.Generic;

namespace JIRASprintQuery.JIRA
{
    public class SprintDetails
    {
        public List<SprintTicket> Tickets { get; set; }

        public SprintSummary Summary { get; set; }
    }
}
