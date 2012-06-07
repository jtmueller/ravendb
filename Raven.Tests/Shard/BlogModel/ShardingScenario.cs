<<<<<<< HEAD
using System;
using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Shard;
using Raven.Server;
using System.Linq;
using Xunit;

namespace Raven.Tests.Shard.BlogModel
{
	public abstract class ShardingScenario : RavenTest, IDisposable
	{
		protected readonly ShardedDocumentStore ShardedDocumentStore;
		protected readonly Dictionary<string, RavenDbServer> Servers;

		protected override void CreateDefaultIndexes(IDocumentStore documentStore)
		{
		}

		protected ShardingScenario()
		{
			RavenDbServer users = null;
			RavenDbServer blogs = null;
			RavenDbServer posts1 = null;
			RavenDbServer posts2 = null;
			RavenDbServer posts3 = null;
			try
			{
				users = GetNewServer(8079, "shard1");
				blogs = GetNewServer(8078, "shard2");
				posts1 = GetNewServer(8077, "shard3");
				posts2 = GetNewServer(8076, "shard4");
				posts3 = GetNewServer(8075, "shard5");
			}
			catch (Exception)
			{
				if (users != null)
					users.Dispose();
				if (blogs != null)
					blogs.Dispose();
				if (posts1 != null)
					posts1.Dispose();
				if (posts2 != null)
					posts2.Dispose();
				if (posts3 != null)
					posts3.Dispose();
				throw;
			}

			Servers = new Dictionary<string, RavenDbServer>
			{
				{"Users", users},
				{"Blogs", blogs},
				{"Posts01", posts1},
				{"Posts02", posts2},
				{"Posts03", posts3}
			};

			var shards = new List<IDocumentStore>
			             	{
			             		new DocumentStore {Identifier = "Users", Url = "http://localhost:8079"},
			             		new DocumentStore {Identifier = "Blogs", Url = "http://localhost:8078"},
			             		new DocumentStore {Identifier = "Posts01", Url = "http://localhost:8077"},
			             		new DocumentStore {Identifier = "Posts02", Url = "http://localhost:8076"},
			             		new DocumentStore {Identifier = "Posts03", Url = "http://localhost:8075"}
			             	}.ToDictionary(x => x.Identifier, x => x);

			foreach (var shard in shards)
			{
				shard.Value.Conventions.FailoverBehavior = FailoverBehavior.FailImmediately;
			}

			ShardedDocumentStore = new ShardedDocumentStore(new ShardStrategy(shards)
															{
																ShardAccessStrategy = new SequentialShardAccessStrategy(),
																ShardResolutionStrategy = new BlogShardResolutionStrategy(3),
															});
			ShardedDocumentStore = (ShardedDocumentStore) ShardedDocumentStore.Initialize();
		}

		protected void AssertNumberOfRequests(RavenDbServer server, int numberOfRequests)
		{
			Assert.Equal(numberOfRequests, server.Server.NumberOfRequests);
		}

		public void Dispose()
		{
			ShardedDocumentStore.Dispose();
			foreach (var ravenDbServer in Servers)
			{
				ravenDbServer.Value.Dispose();
			}
		}
	}
=======
using System;
using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Shard;
using Raven.Server;
using System.Linq;
using Xunit;

namespace Raven.Tests.Shard.BlogModel
{
	public abstract class ShardingScenario : RavenTest, IDisposable
	{
		protected readonly ShardedDocumentStore ShardedDocumentStore;
		protected readonly Dictionary<string, RavenDbServer> Servers;

		protected override void CreateDefaultIndexes(IDocumentStore documentStore)
		{
		}

		protected ShardingScenario()
		{
			RavenDbServer users = null;
			RavenDbServer blogs = null;
			RavenDbServer posts1 = null;
			RavenDbServer posts2 = null;
			RavenDbServer posts3 = null;
			try
			{
				users = GetNewServer(8079, "shard1");
				blogs = GetNewServer(8078, "shard2");
				posts1 = GetNewServer(8077, "shard3");
				posts2 = GetNewServer(8076, "shard4");
				posts3 = GetNewServer(8075, "shard5");
			}
			catch (Exception)
			{
				if (users != null)
					users.Dispose();
				if (blogs != null)
					blogs.Dispose();
				if (posts1 != null)
					posts1.Dispose();
				if (posts2 != null)
					posts2.Dispose();
				if (posts3 != null)
					posts3.Dispose();
				throw;
			}

			Servers = new Dictionary<string, RavenDbServer>
			{
				{"Users", users},
				{"Blogs", blogs},
				{"Posts01", posts1},
				{"Posts02", posts2},
				{"Posts03", posts3}
			};

			var shards = new List<IDocumentStore>
			             	{
			             		new DocumentStore {Identifier = "Users", Url = "http://localhost:8079"},
			             		new DocumentStore {Identifier = "Blogs", Url = "http://localhost:8078"},
			             		new DocumentStore {Identifier = "Posts01", Url = "http://localhost:8077"},
			             		new DocumentStore {Identifier = "Posts02", Url = "http://localhost:8076"},
			             		new DocumentStore {Identifier = "Posts03", Url = "http://localhost:8075"}
			             	}.ToDictionary(x => x.Identifier, x => x);

			foreach (var shard in shards)
			{
				shard.Value.Conventions.FailoverBehavior = FailoverBehavior.FailImmediately;
			}

			ShardedDocumentStore = new ShardedDocumentStore(new ShardStrategy(shards)
															{
																ShardAccessStrategy = new SequentialShardAccessStrategy(),
																ShardResolutionStrategy = new BlogShardResolutionStrategy(3),
															});
			ShardedDocumentStore = (ShardedDocumentStore) ShardedDocumentStore.Initialize();
		}

		protected void AssertNumberOfRequests(RavenDbServer server, int numberOfRequests)
		{
			Assert.Equal(numberOfRequests, server.Server.NumberOfRequests);
		}

		public override void Dispose()
		{
			ShardedDocumentStore.Dispose();
			foreach (var ravenDbServer in Servers)
			{
				ravenDbServer.Value.Dispose();
			}
			base.Dispose();
		}
	}
>>>>>>> upstream/master
}