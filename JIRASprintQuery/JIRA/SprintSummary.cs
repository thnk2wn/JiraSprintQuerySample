using System.Collections.Generic;

namespace JIRASprintQuery.JIRA
{
    public class SprintSummary
    {
        public int TotalPoints { get; set; }

        public int TotalDonePoints { get; set; }

        public int ItemsThisSprint { get; set; }

        public int ItemsPriorSprint { get; set; }

        public int ItemsDone { get; set; }

        public int ItemsRemaining { get; set; }

        public int BugsCreated { get; set; }

        public int BugsAddressed { get; set; }

        public Dictionary<string, int> StatusCounts { get; set; }
    }
}
