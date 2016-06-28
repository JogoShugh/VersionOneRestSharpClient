using System;
using System.Collections.Generic;
using RestSharp;
using VersionOneRestSharpClient.Client;
using static System.Console;
using static VersionOneRestSharpClient.Client.ComparisonOperator;
using static VersionOneRestSharpClient.Client.ClientUtilities;

namespace VersionOneRestSharpClient.Example
{
    public class Example
    {
        //private const string V1_REST_API_URL = "http://localhost/VersionOne.Web/rest-1.v1/Data";
        private const string V1_REST_API_URL = "https://www14.v1host.com/v1sdktesting/rest-1.v1/Data";
        private const string USERNAME = "admin";
        private const string PASSWORD = "admin";
        private const string ACCESS_TOKEN = "1.EGwVOB/NsLKeTkRRJO+OpYhuFPo="; // for use with v1sdktesting site

        private static void Main(string[] args)
        {
            // Create the rest client instance, which is a simple derived version of the RestSharp client:
            var client = new VersionOneRestClient(V1_REST_API_URL, USERNAME, PASSWORD);

            Create(client);
            QueryWithSimpleWhere(client);
            QueryWithNotEqualsStringFormFilter(client);
            QueryWithIsEqualOperatorConvenienceHelper(client);
            QueryWithStronglyTypedQueryBuilderForScopeWithStringBasedFilter(client);
            QueryWithStronglyTypedQueryBuilderForMemberWithLambdaBasedFilter(client);
            Update(client);
            // TODO: Delete
            // TODO: other Operations...

            WriteLine("Press any key to exist");
            Read();
        }

        private static void WriteIntro(string description)
        {
            WriteLine(Environment.NewLine);
            WriteLine("####################");
            WriteLine(description);
            WriteLine();
        }

        private static void WriteRawJson(dynamic result)
        {
            WriteLine(Environment.NewLine);
            WriteLine("**********");
            WriteLine("JSON:");
            WriteLine(result.ToString());
            WriteLine(Environment.NewLine);
        }

        private static void Create(VersionOneRestClient client)
        {
            WriteIntro("Create:");

            var res = client.Create("Story", new
            {
                Name = "Testing the client.Create method at " + DateTime.Now.ToLongTimeString(),
                Description = "Just playing around...",
                Scope = "Scope:86271",
                //Owners = Relations(Add("Member:86309"), Remove("Member:20"))
                //Owners = Relations(Add("Member", 86309), Remove("Member", 20))
                // Other options:
                // Owners = Add("Member:20")
                // Owners = Relation("Member:20")
                // Owners = Relation(Add("Member:20"))
                Owners = Add("Member", 86309)
                // Owners = Remove("Member", 86309)
                // Or, the old school by hand way:
                /*
                Owners = new[]
                {
                    new { idref = "Member:20", act = "add" },
                    new { idref = "Member:86309", act = "add" }
                }
                */
            });

            var oidToken = res.Data[0]._links.self.oidToken.ToString();

            res = client.Create("Task", new
            {
                Name = "My Task 1",
                Parent = oidToken
            });
            WriteLine(res.Data[0]._links.self.oidToken);

            res = client.Create("Task", new
            {
                Name = "My Task 2",
                Parent = oidToken
            });
            WriteLine(res.Data[0]._links.self.oidToken);

            var results = client.Query("Story")
                .Select("Children:Task")
                .Where("ID", oidToken)
                .Execute();
            WriteLine(results);

            // Remove
            res = client.Update(oidToken, new
            {
                Owners = Remove("Member:20")
            });
        }

        private static void QueryWithSimpleWhere(VersionOneRestClient client)
        {
            WriteIntro("QueryWithSimpleWhere:");

            var results = client.Query("Member")
                //.Select("Name", "Nickname", "OwnedWorkitems.Name")
                .Where("Name", "Sample: Alfred Smith") // <-- Simple match
                .Paging(10)
                .Execute();

            WriteLine(results.ToString());

            foreach (var result in results)
            {
                WriteRawJson(result);

                WriteLine("Name:" + result.Name);
                WriteLine("Name:" + result.Nickname);

                if (result["OwnedWorkitems.Name"].HasValues == false) continue;

                WriteLine("Workitems: " + result["OwnedWorkitems.Name"]);
                WriteLine("Workitems count: " + result["OwnedWorkitems.Name"].Count);
                foreach (var proj in result["OwnedWorkitems.Name"])
                {
                    WriteLine("Program: " + proj);
                }
            }
        }

        private static void QueryWithNotEqualsStringFormFilter(VersionOneRestClient client)
        {
            WriteIntro("QueryWithNotEqualsStringFormFilter:");

            var results = client.Query("Scope")
                .Select("Name", "Description", "Workitems.Name")
                .Filter("SecurityScope.Name", "!=", "System (All Projects)") // excludes top level one
                .Paging(5, 2)
                .Execute();
            foreach (var result in results)
            {
                WriteRawJson(result);
                // Lazy!
            }
        }

        private static void QueryWithIsEqualOperatorConvenienceHelper(VersionOneRestClient client)
        {
            WriteIntro("QueryWithIsEqualOperatorConvenienceHelper:");

            var results = client.Query("Defect")
                .Select("Name", "Description", "Children")
                .Filter("ID", IsEqual, "Defect:64939")
                .Paging(10)
                .Execute();

            foreach (var result in results)
            {
                WriteRawJson(result);

                WriteLine("Name:" + result.Name);
                WriteLine("Description:" + result.Description);

                if (result.Children == null) continue;

                WriteLine("Chilren count: " + result.Children.Count);
                foreach (var item in result.Children)
                {
                    WriteLine("Child: " + item);
                }
            }
        }

        private static void QueryWithStronglyTypedQueryBuilderForScopeWithStringBasedFilter(VersionOneRestClient client)
        {
            WriteIntro("QueryWithStronglyTypedQueryBuilderForScopeWithStringBasedFilter:");

            var results = client.Query<Scope>()
                .Select(s => s.Name, s => s.Description, s => s.Workitems)
                .Filter("SecurityScope.Name", NotEqual, "System (All Projects)")
                .Paging(1)
                .Execute();

            foreach (Scope result in results)
            {
                WriteLine("Name:" + result.Name);
                WriteLine("Description:" + result.Description);
                WriteLine("Workitems count: " + result.Workitems.Count);
                foreach (var item in result.Workitems)
                {
                    WriteLine("Workitem: " + item);
                }
                WriteLine(Environment.NewLine);
            }
        }

        private static void QueryWithStronglyTypedQueryBuilderForMemberWithLambdaBasedFilter(VersionOneRestClient client)
        {
            WriteIntro("QueryWithStronglyTypedQueryBuilderForMemberWithLambdaBasedFilter:");

            var results = client.Query<Member>()
                .Select(m => m.Name, m => m.Nickname, m => m.Workitems)
                .Filter(m => m.Name, NotEqual, "Administrator")
                .Paging(1)
                .Execute();

            foreach (Member result in results)
            {
                WriteLine("Name:" + result.Name);
                WriteLine("Description:" + result.Nickname);
                WriteLine("Workitems count: " + result.Workitems.Count);
                foreach (var workItemName in result.Workitems)
                {
                    WriteLine("Work item name: " + workItemName);
                }
                WriteLine(Environment.NewLine);
            }
        }

        private static void Update(VersionOneRestClient client)
        {
            WriteIntro("Update with an anonymous object");

            var res = client.Update("Story:1083", new
            {
                Name = "It's a test from VersionOneRestSharpClient",
                Description = "And this is a description from VersionOneRestSharpClient",
                ToDo = 9.5
            });
        }
    }

    public class Scope
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [Map("Workitems.Name")]
        public List<string> Workitems { get; set; }

        public Scope()
        {
            Workitems = new List<string>();
        }
    }

    public class Member
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        [Map("OwnedWorkitems.Name")]
        public List<string> Workitems { get; set; }

        public Member()
        {
            Workitems = new List<string>();
        }
    }
}