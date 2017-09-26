using JIRASprintQuery.Excel;
using JIRASprintQuery.IO;
using JIRASprintQuery.JIRA;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace JIRASprintQuery
{
    public class Program
    {
        // async main requires C# 7.1 in project file properties and Rosyln support (VS 2017 Preview 2 currently)
        public static async Task<int> Main(string[] args)
        {
            var consoleInput = new ConsoleInput().Get();
            if (!consoleInput.IsValid) return -1;

            var logger = LogFactory.Create();
            var jiraQuerySvc = new JiraQueryService(logger, consoleInput.Username, consoleInput.Password, consoleInput.JiraServer);
            var details = await jiraQuerySvc.QuerySprintTickets(consoleInput.SprintName);

            // Don't save credentials until they've been validated above.
            Credentials.Save(consoleInput.Username, consoleInput.Password, consoleInput.JiraServer);

            var excelWriter = new JiraExcelWriter(logger);
            excelWriter.Write(details, consoleInput.SprintName);

            #if DEBUG
            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
            #endif

            return 0;
        }
    }
}
