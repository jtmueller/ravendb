//-----------------------------------------------------------------------
// <copyright file="VersioningPutTrigger.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Bundles.Versioning.Data;
using Raven.Database.Plugins;
using System.Linq;
using Raven.Json.Linq;

namespace Raven.Bundles.Versioning.Triggers
{
	public class VersioningPutTrigger : AbstractPutTrigger
	{
		public const string RavenDocumentRevision = "Raven-Document-Revision";
		public const string RavenDocumentParentRevision = "Raven-Document-Parent-Revision"; 
		public const string RavenDocumentRevisionStatus = "Raven-Document-Revision-Status";

		public override VetoResult AllowPut(string key, RavenJObject document, RavenJObject metadata, TransactionInformation transactionInformation)
		{
			var jsonDocument = Database.Get(key, transactionInformation);
			if (jsonDocument == null)
				return VetoResult.Allowed;

			switch (jsonDocument.Metadata.Value<string>(RavenDocumentRevisionStatus))
			{
				case "Historical":
					return VetoResult.Deny("Modifying a historical revision is not allowed");
				default:
					return VetoResult.Allowed;
			}
		}

		public override void OnPut(string key, RavenJObject document, RavenJObject metadata, TransactionInformation transactionInformation)
		{
			if (key.StartsWith("Raven/", StringComparison.InvariantCultureIgnoreCase))
				return;

			if (metadata.Value<string>(RavenDocumentRevisionStatus) == "Historical")
				return;

			var versioningConfiguration = GetDocumentVersioningConfiguration(metadata);

			if (versioningConfiguration.Exclude)
				return;

			
			using(Database.DisableAllTriggersForCurrentThread())
			{
				var copyMetadata = new RavenJObject(metadata);
				copyMetadata[RavenDocumentRevisionStatus] = RavenJToken.FromObject("Historical");
				copyMetadata[Constants.RavenReadOnly] = true;
				copyMetadata.Remove(RavenDocumentRevision);
				var parentRevision = metadata.Value<string>(RavenDocumentRevision);
				if(parentRevision!=null)
				{
					copyMetadata[RavenDocumentParentRevision] = key + "/revisions/" + parentRevision;
					metadata[RavenDocumentParentRevision] = key + "/revisions/" + parentRevision;
				}

				PutResult newDoc = Database.Put(key + "/revisions/", null, document, copyMetadata,
											 transactionInformation);
				int revision = int.Parse(newDoc.Key.Split('/').Last());

				RemoveOldRevisions(key, revision, versioningConfiguration, transactionInformation);

				metadata[RavenDocumentRevisionStatus] = RavenJToken.FromObject("Current");
				metadata[RavenDocumentRevision] = RavenJToken.FromObject(revision);
			}
		}

		private VersioningConfiguration GetDocumentVersioningConfiguration(RavenJObject metadata)
		{
		    JsonDocument doc = null;

			var entityName = metadata.Value<string>("Raven-Entity-Name");
			if(entityName != null)
			{
				doc = Database.Get("Raven/Versioning/" + entityName, null);
			}

			if(doc == null)
			{
				doc = Database.Get("Raven/Versioning/DefaultConfiguration", null);
			}

		    if (doc != null)
		    {
		        return doc.DataAsJson.JsonDeserialization<VersioningConfiguration>();
		    }

		    return new VersioningConfiguration
		               {
		                   MaxRevisions = Int32.MaxValue,
		                   Exclude = false
		               };
		}

		private void RemoveOldRevisions(string key, int revision, VersioningConfiguration versioningConfiguration, TransactionInformation transactionInformation)
		{
			int latestValidRevision = revision - versioningConfiguration.MaxRevisions;
			if (latestValidRevision <= 0)
				return;

			Database.Delete(string.Format("{0}/revisions/{1}", key, latestValidRevision), null, transactionInformation);
		}
	}
}
