using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotTeamFoundationServer.TeamFoundationServer
{
    [Serializable]
    public class WorkItemForm
    {
        [Prompt("What you want to create? {||}")]
        public WorkItemFormType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

    }


    public enum WorkItemFormType
    {
        Bug = 1,
        Task = 2
    }

}