//-----------------------------------------------------------------------
// <copyright file="RemoveConflictOnAttachmentPutTrigger.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.ComponentModel.Composition;
using System.IO;
using Raven.Database.Plugins;
using Raven.Json.Linq;

namespace Raven.Bundles.Replication.Triggers
{
	[ExportMetadata("Order", 10000)]
	public class RemoveConflictOnAttachmentPutTrigger : AbstractAttachmentPutTrigger
	{
		public override void OnPut(string key, Stream data, RavenJObject metadata)
		{
			using (Database.DisableAllTriggersForCurrentThread())
			{
				metadata.Remove(ReplicationConstants.RavenReplicationConflict);// you can't put conflicts

				var oldVersion = Database.GetStatic(key);
				if (oldVersion == null)
					return;
				if (oldVersion.Metadata[ReplicationConstants.RavenReplicationConflict] == null)
					return;
				// this is a conflict document, holding document keys in the 
				// values of the properties
				foreach (var prop in oldVersion.Metadata.Value<RavenJArray>("Conflicts"))
				{
					Database.DeleteStatic(prop.Value<string>(), null);
				}
			}
		}
	}
}