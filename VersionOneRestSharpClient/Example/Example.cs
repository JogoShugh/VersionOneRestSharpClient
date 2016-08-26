using System;
using System.Collections.Generic;
using VersionOneRestSharpClient.Client;
using static System.Console;
using static VersionOneRestSharpClient.Client.ComparisonFunctions;
using static VersionOneRestSharpClient.Client.ClientUtilities;
using System.Diagnostics;

namespace VersionOneRestSharpClient.Example
{

	public class Example
	{
		//private const string V1_REST_API_URL = "http://localhost/VersionOne.Web/rest-1.v1/Data";
		private const string V1_REST_API_URL = "https://www14.v1host.com/v1sdktesting/rest-1.v1/Data";
		//private const string V1_REST_API_URL = "https://www16.v1host.com/api-examples/rest-1.v1/Data";
		private const string USERNAME = "admin";
		private const string PASSWORD = "admin";
		private const string ACCESS_TOKEN = "1.EGwVOB/NsLKeTkRRJO+OpYhuFPo="; // for use with v1sdktesting site

		private static void Main(string[] args)
		{
			// Customer code:
			/*
            "from: Defect select: \n" +
            " - Name\n" +
            " - Number\n" +
            " - Description\n" +
            " - Resolution\n" +
            " - ResolutionReason.Name\n" +
            " - from: Children:Task\n" +
            " select:\n" +
            " - Name\n" +
            " - Description\n" +
            " where:\n" +
            " Name: Release Notes\n" +
            "where:\n" +
            " Scope.Name: " + v1ProjectName + "\n" +
            " Timebox.Name: " + v1Sprint;
            */
			// WIP for query.v1
			var v1ProjectName = "System (All Projects)";
			var v1Sprint = "Iteration 1";
			var query =
				From("Defect")
				.Select(
					"Name",
					"Number",
					"Description",
					"Resolution",
					"ResolutionReason.Name",
					From("Children:Task")
						.Select(
							"Name",
							"Description"
						)
						.Where(
							Equal("Name", "Release Notes")
						)
				)
				.Where(
					Equal("Scope.Name", v1ProjectName),
					Equal("Timebox.Name", v1Sprint)
				);

			var payload = query.ToString();
			Debug.Print(payload);

			// Create the rest client instance, which is a simple derived version of the RestSharp client:
			var client = new VersionOneRestClient(V1_REST_API_URL, USERNAME, PASSWORD);

			QueryForScopes();
			//CreateLinkOnStory();
			//Create(client);
			//CreateWithChildren();
			//QueryWithExists(client);
			//QueryWithNotExists(client);
			//QueryWithSimpleWhere(client);
			//QueryWithNotEqualsStringFormFilter(client);
			//QueryWithIsEqualOperatorConvenienceHelper(client);
			//QueryWithStronglyTypedQueryBuilderForScopeWithStringBasedFilter(client);
			//QueryWithStronglyTypedQueryBuilderForMemberWithLambdaBasedFilter(client);
			//Update(client);
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

			var scope = "Scope:86271";

			dynamic asset = client.Create("Story", new
			{
				Name = "Testing the client.Create method at " + DateTime.Now.ToLongTimeString(),
				Description = "Just playing around...",
				Scope = scope,
				Owners = Relation("Member:20")
			});

			var oidToken = asset.OidToken;

			asset = client.Create("Issue", new
			{
				Name = "My Issue",
				Scope = scope,
				PrimaryWorkitems = Relation(oidToken)
			});
			var tok1 = asset.OidToken;
			WriteLine(tok1);

			asset = client.Create("Issue", new
			{
				Name = "My Issue 2",
				Scope = scope,
				PrimaryWorkitems = Relation(oidToken)
			});
			var tok2 = asset.OidToken;
			WriteLine(tok2);

			asset = client.Query("Story")
				.Select("Name", "Issues", "Description")
				.Where("ID", oidToken)
				.RetrieveFirst();

			WriteLine(asset);

			var storyTasks = asset._links["Issues"];

			WriteLine(storyTasks);

			//results[0].RemoveRelatedAssets("Issues", "Issue:1", "Issue:2");
			//results[0].RemoveRelatedAssets("Issues", new string[] { tok1.ToString(), tok2.ToString() });
			//results[0].RemoveRelatedAssets("Issues", new [] { tok1, tok2 });
			asset.RemoveRelatedAssets("Issues", tok1, tok2);

			storyTasks = asset._links["Issues"];
			WriteLine(storyTasks);

			asset.AddRelatedAssets("Issues", tok1);

			storyTasks = asset._links["Issues"];
			WriteLine(storyTasks);

			asset.Name = "Newbie name";
			//results[0].NewProp = "New Prop";
			asset.Description = "Just playing around...";

			var changes = asset.GetChangesDto();

			var payload = RestApiPayloadBuilder.Build(changes);

			asset = client.Update(oidToken, new
			{
				Owners = Remove("Member:20")
			});
		}

		private static void CreateLinkOnStory()
		{
			var client = new VersionOneRestClient(V1_REST_API_URL, USERNAME, PASSWORD);
			var response = client.Create("Link", new
			{
				Asset = "Story:90295",
				URL = "http://www.versionone.com",
				Name = "This is the link to VersionOne",
				OnMenu = false
			});

			WriteLine(response.OidToken);
		}

		private static void CreateWithChildren()
		{
			dynamic scope = Asset("Scope");
			scope.Name = "My Project";
			scope.Owner = "Member:20";
			scope.Parent = "Scope:0";
			scope.BeginDate = DateTime.Now;

			for (var i = 0; i < 5; i++)
			{
				dynamic story = Asset("Story");
				story.Name = $"Story {i}";
				story.Description = $"Description {i}";
				for (var j = 0; j < 3; j++)
				{
					story.CreateRelatedAsset("Children", Asset("Task", new
					{
						Name = $"Task {i}{j}",
						Description = $"Description for Task {i}{j}"
					}));
					story.CreateRelatedAsset("Children", Asset("Test", new
					{
						Name = $"Test {i}{j}",
						Description = $"Description for Test {i}{j}"
					}));
				}
				scope.CreateRelatedAsset("Workitems", story);
			}

			string yam = scope.GetYamlPayload();
		}

		private static void QueryWithExists(VersionOneRestClient client)
		{
			WriteIntro("QueryWithExists:");

			var results = client.Query("Scope")
				.Select("Parent")
				.Where(
					Exists("Parent")
				)
				.Paging(10)
				.Retrieve();

			WriteLine(results.ToString());

			foreach (var result in results)
			{
				WriteRawJson(result);
			}
		}

		private static void QueryWithNotExists(VersionOneRestClient client)
		{
			WriteIntro("QueryWithNotExists:");

			var results = client.Query("Scope")
				.Select("Parent", "Children")
				.Where(
					NotExists("Parent")
				)
				.Paging(10)
				.Retrieve();

			WriteLine(results.ToString());

			foreach (dynamic result in results)
			{
				WriteRawJson(result);

				result.Workitems.RemoveValue("Story:123");
			}
		}

		private static void QueryWithSimpleWhere(VersionOneRestClient client)
		{
			WriteIntro("QueryWithSimpleWhere:");

			var results = client.Query("Member")
				//.Select("Name", "Nickname", "OwnedWorkitems.Name")
				.Where("Name", "Sample: Alfred Smith") // <-- Simple match
				.Paging(10)
				.Retrieve();

			WriteLine(results.ToString());

			foreach (dynamic result in results)
			{
				WriteRawJson(result);

				WriteLine("Name:" + result.Name);
				WriteLine("Name:" + result.Nickname);

				result.Parent = "Epic:5555";
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
				.Retrieve();
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
				.Filter("ID", ComparisonOperator.Equals, "Defect:64939")
				.Paging(10)
				.Retrieve();

			foreach (dynamic result in results)
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
				.Filter("SecurityScope.Name", ComparisonOperator.NotEquals, "System (All Projects)")
				.Paging(1)
				.Retrieve();

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
				.Filter(m => m.Name, ComparisonOperator.NotEquals, "Administrator")
				.Paging(1)
				.Retrieve();

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

		private static void QueryForScopes()
		{
			var client = new VersionOneRestClient(V1_REST_API_URL, USERNAME, PASSWORD);

			var featureResult = client
				.Query("Epic")
				.Select("Scope", "Scope.Name", "Name", "ID", "Description", "Status", "Priority", "Source") // params array of attribute names
				// However, you could still do this if you really need the array to hang around:
				// var attrNames = new[] { "Scope", "Scope.Name", "Name", "ID", "Description", "Status", "Priority", "Source" };
				// .Select(attrNames)
				.Where(
					Equal("Scope", "Scope:1121"), // Implicit AND when terms separated by commas
					Equal("Scope.Name", "Sample: Release 1.0")
					// Other functions thanks to C# 6 "using static" free-standing functions:
					//,NotEqual
					//,GreaterThan
					//,GreaterThanOrEqual
					//,LessThan
					//,LessThanOrEqual
					//,Exists
					//,NotExists
				)
				.Paging(10, 0) // size 10, start with page 0
				.Retrieve();

			Debug.WriteLine($"Count = {featureResult.Count}");

			var x = 0;
			foreach (var feature in featureResult)
			{
				Debug.WriteLine($"============#: {++x}");
				Debug.WriteLine($"         Oid : {feature.OidToken}");

				foreach(var attr in feature)
				{
					if (attr.Type == Newtonsoft.Json.Linq.JTokenType.String)
					{
						var value = attr.ToString();
						if (value != null && value.Length > 80) value = value.Substring(1, 80) + ".....";
						Debug.WriteLine($"--{attr.Name} : {value}");
					}
					else
					{
						Debug.WriteLine($"--{attr.Name} : {attr}");
					}

				}
			}

			var scopeResult = client.Query("Scope")
				.Select("Name", "Schedule.Name", "Schedule", "Owner")
				.Where(
					Equal("Name", "Sample: Release 1.0")
				)
				.Retrieve();

			Debug.WriteLine($"Count = {scopeResult.Count}");
			x = 0;
			foreach (dynamic scope in scopeResult) // With dynamic, we can just have scope.Name, etc, etc
			{
				Debug.WriteLine($"------------#: {++x}");
				Debug.WriteLine($"          Oid: {scope.OidToken}");
				Debug.WriteLine($"--Name       : {scope.Name}");
				Debug.WriteLine($"--Schedule Name : {scope["Schedule.Name"]}"); // Using [] syntax to dot-notation into child attributes
				Debug.WriteLine($"--Schedule: {scope.Schedule}"); // Broken at the moment
				Debug.WriteLine($"--Owner      : {scope.Owner}");
			}
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

// ALTERNATIVE APPROACHES

//dynamic asset = client.Create("Story", new
//{
//    Name = "Testing the client.Create method at " + DateTime.Now.ToLongTimeString(),
//    Description = "Just playing around...",
//    Scope = scope,
//    //Owners = Relations(Add("Member:86309"), Remove("Member:20"))
//    //Owners = Relations(Add("Member", 86309), Remove("Member", 20))
//    // Other options:
//    // Owners = Add("Member:20")
//    // Owners = Relation("Member:20")
//    // Owners = Relation(Add("Member:20"))
//    Owners = Relation("Member:20") // Add("Member", 86309)
//                                   // Owners = Remove("Member", 86309)
//                                   // Or, the old school by hand way:
//                                   /*
//                                   Owners = new[]
//                                   {
//                                       new { idref = "Member:20", act = "add" },
//                                       new { idref = "Member:86309", act = "add" }
//                                   }
//                                   */
//});