using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using BotTeamFoundationServer.TeamFoundationServer;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;

namespace BotTeamFoundationServer
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        
        private static OnCompletionAsyncDelegate<WorkItemForm> processWorkitem;
        private static TFSClient _tfsClient = new TFSClient();

        private static IForm<WorkItemForm> BuildForm()
        {
            processWorkitem = ProcessWorkItem;


            // As the form will be built
            return new FormBuilder<WorkItemForm>()
                .Message("Welcome to Bot Team Foundation Server!")
                .Field(nameof(WorkItemForm.Type))
                .Field(nameof(WorkItemForm.Title))
                .Field(nameof(WorkItemForm.Description))
                .Confirm("The data are correct to send?")
                .OnCompletionAsync(processWorkitem)
                .Build();
        }
        
        private async static Task ProcessWorkItem(IDialogContext context, WorkItemForm state)
        {
            var message = context.MakeMessage();
            message.Text = "We are currently processing your request.";
            
            await context.PostAsync(message);


            if (_tfsClient.CreateWorkItem(state, "Mirai"))
            {
                message = context.MakeMessage();
                message.Text = "Your request has been successfully completed.";

                await context.PostAsync(message);
            }
            else
            {
                message = context.MakeMessage();
                message.Text = "An error occurred while processing your request, try again!";

                await context.PostAsync(message);
            }
        }

        internal static IDialog<WorkItemForm> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(BuildForm));
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                return await Conversation.SendAsync(message, MakeRootDialog);
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}