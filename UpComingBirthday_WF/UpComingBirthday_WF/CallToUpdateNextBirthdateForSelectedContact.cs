using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;

namespace UpComingBirthday_WF
{
    public class CallToUpdateNextBirthdateForSelectedContact : CodeActivity
    {
        //Define the properties
        [RequiredArgument]
        [Input("Update Next Birthdate for")]
        [ReferenceTarget("contact")]
        public InArgument<EntityReference> Contact { get; set; }


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


                Guid Contactid = this.Contact.Get(context).Id;

                RetrieveRequest getContactBirthdayDate = new RetrieveRequest()
                {
                    ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "birthdate" }),
                    Target = new EntityReference(this.Contact.Get(context).LogicalName, Contactid)
                };
                Entity EntityResponse = (Entity)((RetrieveResponse)orgServiceConext.Execute(getContactBirthdayDate)).Entity;

                DateTime? birthdate;
                if (EntityResponse.Contains("birthdate"))
                {
                    birthdate = (DateTime?)EntityResponse["birthdate"];
                }
                else
                {
                    birthdate = null;
                }

                if (birthdate == null)
                {
                    return;
                }
                DateTime nextBirthdate = CalculateNextBirthday(birthdate.Value);

                //Update the next birthday field on the entity
                Entity updateEntity = new Entity(this.Contact.Get(context).LogicalName);
                updateEntity.Id = Contactid;
                updateEntity["new_nextbirthday"] = nextBirthdate;

                orgServiceConext.Update(updateEntity);

                traceObj.Trace("Workflow End successfully");
            }
            catch (Exception e)
            {
                traceObj.Trace("Workflow Ends with Error");
                throw new InvalidPluginExecutionException("Error it is " + e.Message);
            }
        }

        private DateTime CalculateNextBirthday(DateTime birthdate)
        {
            DateTime nextBirthday = new DateTime(birthdate.Year, birthdate.Month, birthdate.Day);

            //Check to see if this birthday occurred on a leap year
            bool leapYearAdjust = false;
            if (nextBirthday.Month == 2 && nextBirthday.Day == 29)
            {
                //Sanity check, was that year a leap year
                if (DateTime.IsLeapYear(nextBirthday.Year))
                {
                    //Check to see if the current year is a leap year
                    if (!DateTime.IsLeapYear(DateTime.Now.Year))
                    {
                        //Push the date to March 1st so that the date arithmetic will function correctly
                        nextBirthday = nextBirthday.AddDays(1);
                        leapYearAdjust = true;
                    }
                }
                else
                {
                    throw new Exception("Invalid Birthdate specified", new ArgumentException("Birthdate"));
                }
            }

            //Calculate the year difference
            nextBirthday = nextBirthday.AddYears(DateTime.Now.Year - nextBirthday.Year);

            //Check to see if the date was adjusted
            if (leapYearAdjust && DateTime.IsLeapYear(nextBirthday.Year))
            {
                nextBirthday = nextBirthday.AddDays(-1);
            }

            return nextBirthday;
        }
    }
}
