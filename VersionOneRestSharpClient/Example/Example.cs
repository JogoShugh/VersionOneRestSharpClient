using System;
using System.Collections.Generic;
using RestSharp;
using VersionOneRestSharpClient.Client;

namespace VersionOneRestSharpClient.Example
{
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


    public class Example
    {
        //private const string V1_REST_API_URL = "http://localhost/VersionOne.Web/rest-1.v1/Data";
        private const string V1_REST_API_URL = "https://www14.v1host.com/v1sdktesting/rest-1.v1/Data";
        private const string USERNAME = "admin";
        private const string PASSWORD = "admin";
        private const string ACCESS_TOKEN = "1.EGwVOB/NsLKeTkRRJO+OpYhuFPo="; // for use with v1sdktesting site
        
        private static void Main(string[] args)
		{
			int memberId = 20;
			if (args.Length > 0)
			{
				memberId = Convert.ToInt32(args[0]);
			}

			// Create the rest client instance, which is a simple derived version of the RestSharp client:
			var client = new VersionOneRestClient(V1_REST_API_URL, USERNAME, PASSWORD);

			/*------------------------*/

			Console.WriteLine("Update with an anonymous object");
			var res = client.Update("Story:1083", new
			{
				Name = "It's a test from VersionOneRestSharpClient",
				Description = "And this is a description from VersionOneRestSharpClient",
				ToDo = 9.5
			});

			/*---*/

			Console.WriteLine("Approach #1 to Scope -- Completely Untyped, returns raw XML:");
			var query = new RestApiUriQueryBuilder("Scope")
				.Select("Name", "Description", "Workitems.Name")
				.Filter("SecurityScope.Name", "!=", "System (All Projects)") // excludes top level one
				.Paging(10);

			var request = new RestRequest(query.ToString());
			var responseRawXml = client.Get(request);
			Console.WriteLine(responseRawXml.Content + Environment.NewLine);

			/*---*/

			Console.WriteLine("Approach #1 to Member -- Completely Untyped, returns raw XML:");
			query = new RestApiUriQueryBuilder("Member")
				.Select("Name", "Nickname", "OwnedWorkitems.Name")
				.Paging(10);

			request = new RestRequest(query.ToString());
			responseRawXml = client.Get(request);
			Console.WriteLine(responseRawXml.Content + Environment.NewLine);

			/*------------------------*/

			//Console.WriteLine("Approach #2 to Scope -- -- Untyped, but with dynamic to ease reading:");
			//query = new RestApiUriQueryBuilder("Defect")
			//	.Select("Name", "Description", "Children")
			//	//.Filter("ID", "=", "Defect:64939")
			//	.Paging(10);

			//foreach (var result in responseDynamic)
			//{
			//	// Show JSON representation:
			//	Console.WriteLine(result.ToString());

			//	// Now, display items:
			//	Console.WriteLine("Name:" + result.Name);
			//	Console.WriteLine("Description:" + result.Description);
			//	Console.WriteLine("Chilren count: " + result.Children.Count);
			//	foreach (var item in result.Children)
			//	{
			//		Console.WriteLine("Child: " + item);
			//	}
			//	Console.WriteLine(Environment.NewLine);
			//}

			///*---*/

			Console.WriteLine("Approach #2.1 to Member -- Untyped, but with dynamic to ease reading (using Access Token):");
			query = new RestApiUriQueryBuilder("Member")
				.Select("Name", "Nickname", "OwnedWorkitems.Name")
				.Paging(10);
			request = new RestRequest(query.ToString());
			var client2 = new VersionOneRestClient(V1_REST_API_URL, ACCESS_TOKEN);

			var responseDynamic = client2.Get<List<dynamic>>(request);
			foreach (var result in responseDynamic.Data)
			{
				// Show JSON representation:
				Console.WriteLine(result.ToString());

				// Now, display items:
				Console.WriteLine("Name:" + result.Name);
				Console.WriteLine("Name:" + result.Nickname);
				Console.WriteLine("Workitems: " + result["OwnedWorkitems.Name"]);
				/*
				Console.WriteLine("Workitems count: " + result["OwnedWorkitems.Name"].Count);
				foreach (var proj in result["OwnedWorkitems.Name"])
				{
					Console.WriteLine("Program: " + proj);
				}
				*/
				Console.WriteLine(Environment.NewLine);
			}

			Console.WriteLine(responseRawXml.Content + Environment.NewLine);

			/*------------------------*/

			Console.WriteLine("Approach #3 to Scope -- Strongly-typed to your own custom-defined DTO type:");
			var queryTypedScope = new RestApiUriQueryBuilderTyped<Scope>()
				.Select(m => m.Name, m => m.Description, m => m.Workitems)
				.Paging(10);
			//.Filter("SecurityScope.Name", "!=", "System (All Projects)"); // excludes top level one

			request = new RestRequest(queryTypedScope.ToString());
			var responseWithTypedScopeDto = client.Get<List<Scope>>(request);
			foreach (var result in responseWithTypedScopeDto.Data)
			{
				Console.WriteLine("Name:" + result.Name);
				Console.WriteLine("Description:" + result.Description);
				Console.WriteLine("Workitems count: " + result.Workitems.Count);
				foreach (var item in result.Workitems)
				{
					Console.WriteLine("Workitem: " + item);
				}
				Console.WriteLine(Environment.NewLine);
			}

			/*---*/

			Console.WriteLine("Approach #3 to Member -- Strongly-typed to your own custom-defined DTO type:");
			var queryTypedMember = new RestApiUriQueryBuilderTyped<Member>()
				.Select(m => m.Name, m => m.Nickname, m => m.Workitems)
				.Paging(10);
			//.Filter(m => m.Name, ComparisonOperator.NotEquals, "Administrator");

			Console.WriteLine(queryTypedMember.ToString());

			request = new RestRequest(queryTypedMember.ToString());
			var responseWithTypedMemberDto = client.Get<List<Member>>(request);
			foreach (var result in responseWithTypedMemberDto.Data)
			{
				Console.WriteLine("Name:" + result.Name);
				Console.WriteLine("Description:" + result.Nickname);
				Console.WriteLine("Workitems count: " + result.Workitems.Count);
				foreach (var workItemName in result.Workitems)
				{
					Console.WriteLine("Work item name: " + workItemName);
				}
				Console.WriteLine(Environment.NewLine);
			}

			Console.WriteLine("Type anything to exit...");
			Console.ReadKey();

		}
    }
}