using System;
using System.Collections.Generic;
using System.Linq;

namespace JIRASprintQuery.IO
{
    public class ConsoleInput
    {
        public bool IsValid { get; private set; }
        public string JiraServer { get; private set;}
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string SprintName { get; private set; }

        public ConsoleInput Get()
        {
            // If we have saved Windows credentials in Credential Manager, use those, else prompt.
            var cred = Credentials.Get();

            if (cred.User == null || cred.Pass == null || cred.Desc == null)
            {
                JiraServer = ReadLine("JIRA Server (i.e. https://project.atlassian.net)");
                if (string.IsNullOrEmpty(JiraServer)) return this;

                Username = ReadLine("JIRA Username");
                if (string.IsNullOrEmpty(Username)) return this;

                Password = ReadPassword("JIRA Password");
                if (string.IsNullOrEmpty(Password)) return this;
            }
            else
            {
                Username = cred.User;
                Password = cred.Pass;
                JiraServer = cred.Desc;
            }

            SprintName = GetSprintName(ReadLine("Sprint name or number"));
            if (string.IsNullOrWhiteSpace(SprintName)) return this;

            IsValid = true;
            return this;
        }

        private string ReadLine(string label, bool trim = true)
        {
            Console.WriteLine($"{label}:");
            var input = Console.ReadLine();
            Console.WriteLine();
            if (trim) input = input?.Trim();
            
            if (string.IsNullOrEmpty(input))
            {
                IsValid = false;
                Console.WriteLine($"{label} is required, exiting.");
            }            

            return input;
        }

        private static string GetSprintName(string sprint)
        {
            if (int.TryParse(sprint, out int sprintNumber))
            {
                // i.e. <add key="SprintNamePattern" value="Project - Sprint {SprintNumber}" />
                var template = AppSettings.SprintNamePattern;
                sprint = template.Replace("{SprintNumber}", sprintNumber.ToString());
            }

            return sprint?.Trim();
        }

        public static string ReadPassword(string label, char mask = '*')
        {
            const int enter = 13, backsp = 8, ctrlbacksp = 127;
            int[] filtered = { 0, 27, 9, 10 /*, 32 space, if you care */ }; // const
            var pass = new Stack<char>();
            char chr;
            Console.WriteLine($"{label}:");

            while ((chr = Console.ReadKey(true).KeyChar) != enter)
            {
                if (chr == backsp)
                {
                    if (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (chr == ctrlbacksp)
                {
                    while (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (filtered.Count(x => chr == x) > 0) { }
                else
                {
                    pass.Push(chr);
                    Console.Write(mask);
                }
            }

            Console.WriteLine();
            Console.WriteLine();

            return new string(pass.Reverse().ToArray());
        }
    }
}
