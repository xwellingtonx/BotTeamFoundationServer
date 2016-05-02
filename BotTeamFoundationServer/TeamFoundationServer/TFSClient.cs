using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using teamFoundation = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace BotTeamFoundationServer.TeamFoundationServer
{
    public class TFSClient
    {
        private WorkItemTrackingHttpClient _workItemClient;

        /// <summary>
        /// Class of communication with the team foundation server
        /// Documentation: https://www.visualstudio.com/en-us/integrate/get-started/client-libraries/samples
        /// Examples of the paths of the fields: https://www.visualstudio.com/integrate/api/wit/work-items#CreateaworkitemWithaworkitemlink
        /// </summary>
        public TFSClient()
        {
            //Connect with TFS with a Personal Access Token
            //Uri Example: https://{instance}.visualstudio.com/DefaultCollection
            VssConnection connection = new VssConnection(new Uri("https://{Your Instance}.visualstudio.com/DefaultCollection"), 
                new VssBasicCredential(string.Empty, "Your Personal Acess Token")); 

            _workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public bool CreateWorkItem(WorkItemForm form, string projectName)
        {
            try
            {
                var item = this._workItemClient.GetWorkItemAsync(23);


                var jsonPatch = new JsonPatchDocument();

                //Title
                var opTitle = new JsonPatchOperation();
                opTitle.Operation = Operation.Add;
                opTitle.Path = @"/fields/System.Title";
                opTitle.Value = form.Title;

                jsonPatch.Add(opTitle);


                string typeName = string.Empty;

                if (form.Type == WorkItemFormType.Bug)
                {
                    typeName = Enum.GetName(typeof(WorkItemFormType), WorkItemFormType.Bug);


                    //Repro Steps
                    var opReproSteps = new JsonPatchOperation();
                    opReproSteps.Operation = Operation.Add;
                    opReproSteps.Path = @"/fields/Microsoft.VSTS.TCM.ReproSteps";
                    opReproSteps.Value = form.Description;

                    jsonPatch.Add(opReproSteps);


                    //Initial state for the bug
                    var opBoard = new JsonPatchOperation();
                    opBoard.Operation = Operation.Add;
                    opBoard.Path = @"/fields/System.State";
                    opBoard.Value = "New";

                    jsonPatch.Add(opBoard);
                }
                else if (form.Type == WorkItemFormType.Task)
                {
                    typeName = Enum.GetName(typeof(WorkItemFormType), WorkItemFormType.Task);

                    //Description
                    var opDescription = new JsonPatchOperation();
                    opDescription.Operation = Operation.Add;
                    opDescription.Path = @"/fields/System.Description";
                    opDescription.Value = form.Description;

                    jsonPatch.Add(opDescription);

                    //Initial state for the task
                    var opBoard = new JsonPatchOperation();
                    opBoard.Operation = Operation.Add;
                    opBoard.Path = @"/fields/System.State";
                    opBoard.Value = "To Do";

                    jsonPatch.Add(opBoard);

                    //Inserts a relationship with a backlog previously created in TFS (backlogId = ?)
                    int backlogId = 0; //Change the task you want to make the relationship

                    var opParent = new JsonPatchOperation();
                    opParent.Operation = Operation.Add;
                    opParent.Path = @"/relations/-";
                    opParent.Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse", //Parent
                        url = this._workItemClient.BaseAddress + "/_apis/wit/workItems/"  + backlogId
                    };

                    jsonPatch.Add(opParent);

                }


                var result = this._workItemClient.CreateWorkItemAsync(jsonPatch, projectName, typeName).Result;

                if (result == null)
                {
                    return false;
                }

                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }
    }
}