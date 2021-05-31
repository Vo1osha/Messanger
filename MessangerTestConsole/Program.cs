using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace MessangerTestConsole
{
    class Program
    {

        /// <summary>
        /// более медленный но быстрый вариант как узнать имя пользователя;
        /// </summary>
        private static string _username;

        static void Main(string[] args)
        {
            foreach (var p in Process.GetProcessesByName("explorer"))
            {
                _username = GetProcessOwner(p.Id);
            }

            // remove the domain part from the username
            var usernameParts = _username.Split('\\');

            _username = usernameParts[usernameParts.Length - 1];

            Console.WriteLine(_username);
            Console.ReadLine();
        }

        public static string GetProcessOwner(int processId)
        {
            var query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectCollection processList;

            using (var searcher = new ManagementObjectSearcher(query))
            {
                processList = searcher.Get();
            }

            foreach (var mo in processList.OfType<ManagementObject>())
            {
                object[] argList = { string.Empty, string.Empty };
                var returnVal = Convert.ToInt32(mo.InvokeMethod("GetOwner", argList));

                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[1] + "\\" + argList[0];
                }
            }

            return "NO OWNER";
        }
    }
    }

