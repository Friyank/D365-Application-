using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow_Test
{
    public class CustomWorkflowActivity1 : CodeActivity
    {
        #region Input Properties
        [Input("string input")]
        public InArgument<string> ContactFirstName { get; set; }

        [Input("string input")]
        public InArgument<string> ContactLastName { get; set; }
        #endregion

        #region Output properties
        public OutArgument<Guid> CreatedContactId{ get; set; }
        #endregion

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

                Entity CreateContactEntity = new Entity("contact");
                CreateContactEntity["firstname"] = ContactFirstName.Get(context);
                CreateContactEntity["lastname"] = ContactLastName.Get(context);

                CreatedContactId.Set(context, orgServiceConext.Create(CreateContactEntity);

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
