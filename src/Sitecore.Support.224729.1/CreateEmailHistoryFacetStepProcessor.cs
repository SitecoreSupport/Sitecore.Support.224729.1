using Sitecore.DataExchange;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.DataExchange.Providers.XConnect.Models;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.XConnect.Collection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support
{
  public class CreateEmailHistoryFacetStepProcessor : BasePipelineStepProcessor


  {
    protected override void ProcessPipelineStep(PipelineStep pipelineStep, PipelineContext pipelineContext, ILogger logger)
    {
      ContactModel contactModel = ((BaseHasPlugins)pipelineContext).GetPlugin<SynchronizationSettings>().Target as ContactModel;
      if (contactModel == null)
      {
        Log(logger.Error, pipelineContext, "The target object is not a contact model so the email history facet cannot be set on it.", Array.Empty<string>());
      }
      else if (!contactModel.Facets.ContainsKey("Emails"))
      {
        Log(logger.Debug, pipelineContext, "The contact does not have an email address list facet assigned, so it does not have any email addresses, so the email history facet will not be set on the contact.", Array.Empty<string>());
      }
      else if (!contactModel.Facets.ContainsKey("EmailAddressHistory"))
      {
        EmailAddressList emailAddressList = contactModel.Facets["Emails"] as EmailAddressList;
        if (emailAddressList == null)
        {
          Log(logger.Error, pipelineContext, "The email address list facet exists, but it is not the expected type, so the email history facet will not be set on the contact.", string.Format("actual type: {0}", contactModel.Facets["Emails"].GetType().FullName), $"expected type: {typeof(EmailAddressList).FullName}");
        }
        else
        {
          string text = null;
          if (emailAddressList.PreferredEmail != null)
          {
            text = emailAddressList.PreferredEmail.SmtpAddress;
          }
          if (string.IsNullOrWhiteSpace(text))
          {
            Log(logger.Debug, pipelineContext, "The contact does not have a preferred email address assigned, so it does not have any email addresses, so the email history facet will not be set on the contact.", Array.Empty<string>());
          }
          else
          {
            EmailAddressHistoryEntry item = new EmailAddressHistoryEntry
            {
              EmailAddress = text,
              Id = 1,
              Timestamp = DateTime.UtcNow
            };
            EmailAddressHistory emailAddressHistory = new EmailAddressHistory();
            if (emailAddressHistory.History == null)
            {
              emailAddressHistory.History = new List<EmailAddressHistoryEntry>();
            }
            emailAddressHistory.History.Add(item);
            contactModel.Facets.Add("EmailAddressHistory", emailAddressHistory);
            Log(logger.Debug, pipelineContext, "The email address history facet was added to the contact.", Array.Empty<string>());
          }
        }
      }
    }
  }
}