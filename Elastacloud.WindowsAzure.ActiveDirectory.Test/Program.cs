using System;
using Elastacloud.WindowsAzure.ActiveDirectory;

namespace CloudZync.Common.ActiveDirectory.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string upn = "richard@adelastacloud.onmicrosoft.com";
            var user = new ActiveDirectoryUser(upn);
            var superuser = user.IsInGroup("superuser");
            var tools = user.IsInGroup("tools");

            Console.WriteLine("For user: {0}", upn);
            Console.WriteLine("Is in group \"superuser\": {0}", superuser);
            Console.WriteLine("Is in group \"tools\": {0}", tools);
            Console.WriteLine("Has name: {0}", user.DisplayName);
            Console.WriteLine("Has image here: {0}", user.Thumbnail != null);
            Console.WriteLine("User belongs to {0} groups", user.Groups.Count);

            Console.WriteLine("Press any key to exit ...");
            Console.Read();
        }
    }
}
