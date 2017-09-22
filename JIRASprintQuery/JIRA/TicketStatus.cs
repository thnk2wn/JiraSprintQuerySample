namespace JIRASprintQuery.JIRA
{
    public static class TicketStatus
    {
        public static bool IsComplete(string status)
        {
            return status == "Done" || status == "Ready For Demo";
        }
    }
}
