﻿using System;
using System.Collections.Generic;
using RestSharp;
using VersionOneRestSharpClient.Client;

namespace VersionOneRestSharpClient.Example
{
    public class Scope
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [Map("ScopeLabels.Name")]
        public List<string> Programs { get; set; }

        public Scope()
        {
            Programs = new List<string>();
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
        private const string V1_REST_API_URL = "http://eval.versionone.net/platformtest/rest-1.v1/Data";
        //private const string V1_REST_API_URL = "http://localhost/VersionOne.Web/rest-1.v1/Data";
        private const string USERNAME = "admin";
        private const string PASSWORD = "admin";
        
        private static void Main(string[] args)
        {
            int memberId = 20;
            if (args.Length > 0)
            {
                memberId = Convert.ToInt32(args[0]);
            }

            // Create the rest client instance, which is a simple derived version of the RestSharp client:
            var client = new VersionOneRestClient(V1_REST_API_URL, USERNAME, PASSWORD);

            Console.WriteLine("Approach #1 to Scope -- Completely Untyped, returns raw XML:");
            var query = new RestApiUriQueryBuilder("Scope")
                .Select("Name", "Description", "SecurityScope.Name")
                .Filter("SecurityScope.Name", "!=", "System (All Projects)"); // excludes top level one

            var request = new RestRequest(query.ToString());
            var responseRawXml = client.Get(request);
            Console.WriteLine(responseRawXml.Content + Environment.NewLine);

            /*---*/

            Console.WriteLine("Approach #1 to Member -- Completely Untyped, returns raw XML:");
            query = new RestApiUriQueryBuilder("Member")
                .Id(20)
                .Select("Name", "Nickname", "OwnedWorkitems.Name");

            request = new RestRequest(query.ToString());
            responseRawXml = client.Get(request);
            Console.WriteLine(responseRawXml.Content + Environment.NewLine);            

            /*------------------------*/

            Console.WriteLine("Approach #2 to Scope -- Strongly-typed to your own custom-defined DTO type:");
            var queryTypedScope = new RestApiUriQueryBuilderTyped<Scope>()
                .Select(m => m.Name, m => m.Description, m => m.Programs);
                //.Filter("SecurityScope.Name", "!=", "System (All Projects)"); // excludes top level one

            request = new RestRequest(queryTypedScope.ToString());
            var responseWithTypedScopeDto = client.Get<List<Scope>>(request);
            foreach (var result in responseWithTypedScopeDto.Data)
            {
                Console.WriteLine("Name:" + result.Name);
                Console.WriteLine("Description:" + result.Description);
                Console.WriteLine("Programs count: " + result.Programs.Count);
                foreach (var proj in result.Programs)
                {
                    Console.WriteLine("Program: " + proj);
                }
                Console.WriteLine(Environment.NewLine);
            }

            /*---*/

            Console.WriteLine("Approach #2 to Member -- Completely Untyped, returns raw XML:");
            var queryTypedMember = new RestApiUriQueryBuilderTyped<Member>()
                .Select(m => m.Name, m => m.Nickname, m => m.Workitems);
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