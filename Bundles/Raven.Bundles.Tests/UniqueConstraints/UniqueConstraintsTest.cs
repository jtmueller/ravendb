﻿namespace Raven.Bundles.Tests.UniqueConstraints
{
	using System;
	using System.IO;

	using Raven.Bundles.UniqueConstraints;
	using Raven.Client.Embedded;
	using Raven.Client.UniqueConstraints;

	public abstract class UniqueConstraintsTest : IDisposable
	{
		protected UniqueConstraintsTest()
		{
			this.DocumentStore = new EmbeddableDocumentStore {RunInMemory = true};
			this.DocumentStore.Configuration.PluginsDirectory = Path.GetDirectoryName(typeof(UniqueConstraintsPutTrigger).Assembly.Location);
			this.DocumentStore.RegisterListener(new UniqueConstraintsStoreListener());

			this.DocumentStore.Initialize();
		}

		protected EmbeddableDocumentStore DocumentStore { get; set; }

		public void Dispose()
		{
			DocumentStore.Dispose();
		}
	}

	public class User
	{
		public string Id { get; set; }

		[UniqueConstraint]
		public string Email { get; set; }

		public string Name { get; set; }
	}
}
