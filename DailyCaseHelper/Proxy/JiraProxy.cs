﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechTalk.JiraRestClient;

namespace com.smartwork.Proxy
{
    // http://stackoverflow.com/questions/31079195/adding-a-new-case-comment-via-salesforce-api-c-sharp
    public class JiraProxy
    {
        public static async Task<List<Issue>> GetIssueList(List<string> caseIdList)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");
            string sql = "(";
            bool isFirstOne = true;
            foreach(string caseId in caseIdList)
            {
                if(isFirstOne) 
                {
                    sql += " \"SalesForce ID\" ~  " + caseId;
                    isFirstOne = false;
                }
                else
                {
                    sql += " OR \"SalesForce ID\" ~  " + caseId;
                }                
            }
            sql += ")";

            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("ENGSUPP", "", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }

        public static async Task<List<Issue>> GetIssueList(string email)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");
            string sql = "assignee in (\"" + email  + "\") and status in (\"Open\", \"In Progress\", \"Reopened\", \"Pending\", \"Development in Progress\") ";

            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("ENGSUPP", "", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }

        public static async Task<List<Issue>> GetIssueListByLabel(string label)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");
            string sql = "labels = " + label + "";

            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("ENGSUPP", "", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }

        public static async Task<List<Issue>> GetIssueListByStatus(string status)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");
            string sql = "project = ENGSUPP AND status = \"" + status + "\" AND reporter in (\"Peter.peng@missionsky.com\")";

            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("ENGSUPP", "", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }


        public static async Task<Issue> CreateIssue(IssueFields fields)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            var issue = jira.CreateIssue("ENGSUPP", "Case", fields);

            return issue;
        }      

        public static async Task<Issue> UpdateIssue(Issue issue)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            var result = jira.UpdateIssue(issue);

            return result;
        }

        public static async Task<Comment> CreateComment(IssueRef issue, string caseComment)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            var jiraComments = jira.GetComments(issue);
            bool isFound = false;
            Comment jiraComment = null;
            foreach (Comment temp in jiraComments)
            {
                if (temp.body.Equals(caseComment, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    jiraComment = temp;
                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                jiraComment = jira.CreateComment(issue, caseComment);
            }

            return jiraComment;
        }

        public static async Task<List<Comment>> GetComments(IssueRef issue)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            var jiraComments = jira.GetComments(issue);


            return (jiraComments as List<Comment>);
        }

        public static async Task<List<Attachment>> GetAttachments(IssueRef issue)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            var jiraAttachments = jira.GetAttachments(issue);


            return (jiraAttachments as List<Attachment>);
        }

        public static bool UploadAttachment(IssueRef issue, string fileName, byte[] fileStream)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");
            jira.CreateAttachment(issue, fileStream, fileName);

            return true;
        }

        public static async Task<bool> UpdateJiraStatus(IssueRef issueRef, string jiraStatus, string jiraNextStatus)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            var transitions = jira.GetTransitions(issueRef);
            if ("Closed".Equals(jiraStatus, System.StringComparison.InvariantCultureIgnoreCase)
                && jiraNextStatus == "In Progress")
            {
                foreach (var transition in transitions)
                {
                    if ("Re-Open".Equals(transition.name, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        jira.TransitionIssue(issueRef, transition);
                        break;
                    }
                }
            }

            if ("Pending".Equals(jiraStatus, System.StringComparison.InvariantCultureIgnoreCase)
                && jiraNextStatus == "In Progress")
            {
                foreach (var transition in transitions)
                {
                    if ("Analysis In Progress".Equals(transition.name, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        jira.TransitionIssue(issueRef, transition);
                        break;
                    }
                }
            }

            if ("In Progress".Equals(jiraStatus, System.StringComparison.InvariantCultureIgnoreCase)
                && jiraNextStatus == "Pending")
            {
                foreach (var transition in transitions)
                {
                    if ("Commented".Equals(transition.name, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        jira.TransitionIssue(issueRef, transition);
                        break;
                    }
                }
            }

            if (("Pending".Equals(jiraStatus, System.StringComparison.InvariantCultureIgnoreCase)
                || "Resolved".Equals(jiraStatus, System.StringComparison.InvariantCultureIgnoreCase))
                && jiraNextStatus == "Closed")
            {
                foreach (var transition in transitions)
                {
                    if ("Closed".Equals(transition.name, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        jira.TransitionIssue(issueRef, transition);
                        break;
                    }
                }
            }
            //var issues = jira.TransitionIssue(issueRef, inProgressOrReopen);

            // "Open" -> "In Progress"
            // "In Progress" -> "Commented"
            // "In Progress" -> "Closed"
            return true;
        }

        public static async Task<List<Issue>> GetCaseListFromCrossProjects(DateTime startDate, DateTime endDate, List<string> projects)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            string sql = "resolutiondate >= " + String.Format("{0}-{1}-{2}", startDate.Year, startDate.Month, startDate.Day) + " AND " + " resolutiondate <= " + String.Format("{0}-{1}-{2}", endDate.Year, endDate.Month, endDate.Day);
            sql += " AND \"Salesforce ID\" !~ Empty ";
            sql += " AND project in ( ";

            bool isFirstOne = true;
            foreach (string project in projects)
            {
                if (isFirstOne)
                {
                    sql += project;
                    isFirstOne = false;
                }
                else
                {
                    sql += " , " + project;
                }
            }
            sql += ")";

            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("", "", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }

        public static async Task<List<Issue>> GetIssueListBySalesforceId(string salesforceId, List<string> projects)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            string sql = "\"Salesforce ID\" ~ " + salesforceId;
            sql += " AND project in ( ";

            bool isFirstOne = true;
            foreach (string project in projects)
            {
                if (isFirstOne)
                {
                    sql += project;
                    isFirstOne = false;
                }
                else
                {
                    sql += " , " + project;
                }
            }
            sql += ")";

            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("", "", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }

        public static async Task<List<Issue>> GetIssueListByCreatedDate(DateTime start, DateTime end)
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            // http://www.cnblogs.com/polk6/p/5465088.html
            string sql = "reporter in (\"Peter.peng@missionsky.com\") AND created >= " + start.ToString("yyyy-MM-dd") + " AND created < " + end.ToString("yyyy-MM-dd") + " ";
            
            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("ENGSUPP", "", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }

        public static async Task<List<Issue>> GetProductionIssueList()
        {
            IJiraClient jira = new JiraClient("https://accelaeng.atlassian.net/", "peter.peng@missionsky.com", "peter.peng");

            // http://www.cnblogs.com/polk6/p/5465088.html
            string sql = " status in (Open, \"In Progress\", Reopened, Pending, \"Development in Progress\") AND not \"Salesforce ID\" is EMPTY AND reporter in (\"Peter.peng@missionsky.com\") ";

            List<Issue> issueList = new List<Issue>();
            var issues = jira.GetIssuesByQuery("ENGSUPP", "Bug", sql);
            foreach (Issue issue in issues)
            {
                issueList.Add(issue);
            }

            return issueList;
        }
    }
}
