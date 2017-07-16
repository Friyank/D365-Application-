using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.ObjectModel;

namespace AccountTaskCreationWF
{
    public class AccountTask01 : CodeActivity
    {
        //Account Name property 
        [Input("Account Name (String)")]
        [Default("Not Avaliable")]
        public InArgument<string> AccountName { get; set; }

        //Account Name property 
        [Input("Task Subject(String)")]
        [Default("No Subject")]
        public InArgument<string> TaskSubject { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region Tracing Object
            //Tracing Object
            ITracingService traceObj = context.GetExtension<ITracingService>();
            #endregion

            #region Workflow Instance
            //Workflow Context Object
            IWorkflowContext workflowConext = context.GetExtension<IWorkflowContext>();
            #endregion

            #region Organization Details
            //Organization Service Factory 
            IOrganizationServiceFactory orgServiceFactory = context.GetExtension<IOrganizationServiceFactory>();
            //ORganization Service Context 
            IOrganizationService orgServiceConext = orgServiceFactory.CreateOrganizationService(workflowConext.UserId);
            #endregion

            try
            {
                traceObj.Trace("Workflow Starts successfully");

                Account accountEntity = new Account()
                {
                    Name = AccountName.Get<string>(context),
                };
                Guid accountId = orgServiceConext.Create(accountEntity);

                Task accountsTask = new Task()
                {
                    Subject = TaskSubject.Get<string>(context),
                    RegardingObjectId = new EntityReference(accountEntity.LogicalName, accountId)
                };
                Guid taskId = orgServiceConext.Create(accountsTask);

                QueryByAttribute fetchTaskByAccounts = new QueryByAttribute(accountsTask.LogicalName);
                fetchTaskByAccounts.Attributes.AddRange(new string[] { "regardingobjectid" });
                fetchTaskByAccounts.ColumnSet = new ColumnSet(new string[] { "subject" });
                fetchTaskByAccounts.Values.AddRange(new object[] { accountId });

                RetrieveMultipleRequest FetchRequest = new RetrieveMultipleRequest() {Query = fetchTaskByAccounts };
                Collection<Entity> FetchedResponse = ((RetrieveMultipleResponse)orgServiceConext.Execute(FetchRequest)).EntityCollection.Entities;

                if (FetchedResponse.Count == 1)
                {
                    Task fetchTasked = (Task)FetchedResponse[0];
                    if (fetchTasked.ActivityId == taskId)
                    {
                        fetchTasked.Subject = fetchTasked.Subject + "Appended Subject!!!";
                        orgServiceConext.Update(fetchTasked);
                    }
                }

                traceObj.Trace("Workflow End successfully");
            }
            catch (Exception e)
            {
                traceObj.Trace("Workflow Ends with Error");
                throw new InvalidPluginExecutionException("Error it is " + e.Message);
            }
        }
    }
}
