using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountTaskCreationWF
{
    public class CustomWorkflowActivity1 : CodeActivity
    {
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
