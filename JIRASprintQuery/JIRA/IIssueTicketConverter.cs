using System.Threading.Tasks;
using Atlassian.Jira;

namespace JIRASprintQuery.JIRA
{
    public interface IIssueTicketConverter
    {
        Task<SprintTicket> CreateSprintTicket(Issue issue);
    }
}