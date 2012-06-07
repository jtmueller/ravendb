//-----------------------------------------------------------------------
// <copyright file="ReplicationLastEtagResponser.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
<<<<<<< HEAD:Bundles/Raven.Bundles.Replication/Reponsders/ReplicationLastEtagResponser.cs
=======
using System;
>>>>>>> upstream/master:Bundles/Raven.Bundles.Replication/Responders/ReplicationLastEtagResponser.cs
using NLog;
using Raven.Abstractions.Extensions;
using Raven.Bundles.Replication.Data;
using Raven.Database.Extensions;
using Raven.Database.Server.Abstractions;
using Raven.Database.Server.Responders;

<<<<<<< HEAD:Bundles/Raven.Bundles.Replication/Reponsders/ReplicationLastEtagResponser.cs
namespace Raven.Bundles.Replication.Reponsders
=======
namespace Raven.Bundles.Replication.Responders
>>>>>>> upstream/master:Bundles/Raven.Bundles.Replication/Responders/ReplicationLastEtagResponser.cs
{
	public class ReplicationLastEtagResponser : RequestResponder
	{
		private Logger log = LogManager.GetCurrentClassLogger();

		public override void Respond(IHttpContext context)
		{
<<<<<<< HEAD:Bundles/Raven.Bundles.Replication/Reponsders/ReplicationLastEtagResponser.cs
			var src = context.Request.QueryString["from"];
=======
			var src = context.Request.QueryString["from"];
			var currentEtag = context.Request.QueryString["currentEtag"];
>>>>>>> upstream/master:Bundles/Raven.Bundles.Replication/Responders/ReplicationLastEtagResponser.cs
			if (string.IsNullOrEmpty(src))
			{
				context.SetStatusToBadRequest();
				return;
			}
			while (src.EndsWith("/"))
				src = src.Substring(0, src.Length - 1);// remove last /, because that has special meaning for Raven
			if (string.IsNullOrEmpty(src))
			{
				context.SetStatusToBadRequest();
				return;
			}
			using (Database.DisableAllTriggersForCurrentThread())
			{
				var document = Database.Get(ReplicationConstants.RavenReplicationSourcesBasePath + "/" + src, null);

<<<<<<< HEAD:Bundles/Raven.Bundles.Replication/Reponsders/ReplicationLastEtagResponser.cs
				SourceReplicationInformation sourceReplicationInformation = null;
=======
				SourceReplicationInformation sourceReplicationInformation;
>>>>>>> upstream/master:Bundles/Raven.Bundles.Replication/Responders/ReplicationLastEtagResponser.cs

				if (document == null)
				{
					sourceReplicationInformation = new SourceReplicationInformation()
					{
						ServerInstanceId = Database.TransactionalStorage.Id
					};
				}
				else
				{
					sourceReplicationInformation = document.DataAsJson.JsonDeserialization<SourceReplicationInformation>();
					sourceReplicationInformation.ServerInstanceId = Database.TransactionalStorage.Id;
<<<<<<< HEAD:Bundles/Raven.Bundles.Replication/Reponsders/ReplicationLastEtagResponser.cs
				}

				log.Debug("Got replication last etag request from {0}: [{1}]", src, sourceReplicationInformation);
=======
				}

				log.Debug("Got replication last etag request from {0}: [Local: {1} Remote: {2}]", src, 
					sourceReplicationInformation.LastDocumentEtag, currentEtag);
>>>>>>> upstream/master:Bundles/Raven.Bundles.Replication/Responders/ReplicationLastEtagResponser.cs
				context.WriteJson(sourceReplicationInformation);
			}
		}

		public override string UrlPattern
		{
			get { return "^/replication/lastEtag$"; }
		}

		public override string[] SupportedVerbs
		{
			get { return new[] { "GET" }; }
		}
	}
}
