﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using Xunit;

namespace Raven.Tests.MailingList
{
	public class Vlad : RavenTest
	{
		public class SampleDoc
		{
			public long Number;
			public string Name;
		}

		[Fact]
		public void TestLazyQuery()
		{
			var doc = new SampleDoc
			{
				Number = DateTime.Now.Ticks,
				Name = "Test1"
			};
			using (GetNewServer())
			using (var store = new DocumentStore
			{
				Url = "http://localhost:8079"
			}.Initialize())
			{
				using (var session = store.OpenSession())
				{
					session.Store(doc);
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					RavenQueryStatistics stat1;
					var query1 = session.Query<SampleDoc>().Statistics(out stat1).Where(x => x.Number == doc.Number)
						.Lazily();

					Assert.Equal(query1.Value.ToList().Count, 1);
					Assert.Equal(stat1.TotalResults, 1);

					RavenQueryStatistics stat2;
					var query2 = session.Query<SampleDoc>().Statistics(out stat2).Where(x => x.Number == doc.Number)
						.Select(x => new { x.Name })
						.Lazily();
					Assert.Equal(query2.Value.ToList().Count, 1);
					Assert.Equal(stat2.TotalResults, 1);
				}
			}
		}

		[Fact]
		public void TestReplication()
		{
			string dbName = "MyNewDatabase";

			using (GetNewServer())
			using (var store = new DocumentStore
			{
				Url = "http://localhost:8079"
			}.Initialize())
			{

				store.DatabaseCommands.EnsureDatabaseExists(dbName);
				using (var session = store.OpenSession(dbName))
				{
					session.Store(new SampleDoc { Number = 1, Name = "Test 1" }, "SampleDocs/1");
					session.Store(new SampleDoc { Number = 2, Name = "Test 2" }, "SampleDocs/2");
					session.Store(new SampleDoc { Number = 3, Name = "Test 3" }, "SampleDocs/3");
					session.SaveChanges();

					// Force index creation (it must be done BEFORE deleting)
					var selectWarmup = session.Query<SampleDoc>().Select(x => new { x.Name, x.Number }).ToList();
				}

				using (var session = store.OpenSession(dbName))
				{
					var doc = session.Load<SampleDoc>("SampleDocs/2");
					session.Delete(doc);
					session.SaveChanges();
				}

				using (var session = store.OpenSession(dbName))
				{
					var selectGood = session.Query<SampleDoc>().ToList();
					Assert.Equal(2, selectGood.Count);

					var selectBad = session.Query<SampleDoc>().Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
						.Select(x => new { x.Name, x.Number }).ToList();
					var count = session.Query<SampleDoc>().Customize(x => x.WaitForNonStaleResultsAsOfLastWrite()).Count();

					Assert.Equal(2, selectBad.Count);
					Assert.Equal(2, count);
				}
			}
		}

		[Fact]
		public void WillOnlyGetPost2Once()
		{
			using (EmbeddableDocumentStore store = NewDocumentStore())
			{
				new Post_ByTag().Execute(store);
				using (IDocumentSession session = store.OpenSession())
				{
					session.Store(new Post
					{
						Title = "Post1",
						Tags = new[] { "cloud", "azure" }
					});

					session.Store(new Post
					{
						Title = "Post2",
						Tags = new[] { "amazon", "cloud", "cloud" }
					});
					session.Store(new Post
					{
						Title = "Post3",
						Tags = new[] { "events" }
					});
					session.Store(new Post
					{
						Title = "Post4",
						Tags = new[] { "raven", "db", "cloud" }
					});

					session.SaveChanges();
				}
				using (IDocumentSession session = store.OpenSession())
				{
					int pageSize = 2;
					int pageNumber = 0;
					int recordsToSkip = 0;

					var posts = new List<Post>();

					RavenQueryStatistics stat;
					while (true)
					{
						List<Post> results = session.Query<Post_ByTag.Result, Post_ByTag>()
							.Customize(x => x.WaitForNonStaleResults())
							.OrderBy(x => x.Title)
							.Statistics(out stat)
							.Where(x => x.Tag == "cloud")
							.Take(pageSize)
							.Skip(recordsToSkip)
							.As<Post>()
							.ToList();

						posts.AddRange(results);

						if (results.Count < pageSize)
							break;

						pageNumber++;
						recordsToSkip = pageSize * pageNumber + stat.SkippedResults;
					}

					Assert.Equal(3, posts.Count);
				}
			}
		}

		#region Nested type: Post

		public class Post_ByTag : AbstractIndexCreationTask<Post>
		{
			public Post_ByTag()
			{
				Map = posts => from post in posts
							   from Tag in post.Tags
							   select new { Tag, post.Title };
			}

			public class Result
			{
				public string Tag { get; set; }
				public string Title { get; set; }
			}
		}

		public class Post
		{
			public string Title { get; set; }
			public string[] Tags { get; set; }

			public override string ToString()
			{
				return string.Format("Title: {0}", Title);
			}
		}

		#endregion
	}
}