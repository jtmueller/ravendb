//-----------------------------------------------------------------------
// <copyright file="MappedResults.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Raven.Json.Linq;
using Raven.Database.Storage;
using Xunit;
using System.Linq;

namespace Raven.Tests.ManagedStorage
{
	public class MappedResults : TxStorageTest
	{
		[Fact]
		public void CanStoreAndGetMappedResult()
		{
			using (var tx = NewTransactionalStorage())
			{
				tx.Batch(mutator => mutator.MappedResults.PutMappedResult("test", "users/ayende","ayende", RavenJObject.FromObject(new { Name = "Rahien" }), null));

				tx.Batch(viewer => Assert.NotEmpty(viewer.MappedResults.GetMappedResults(new GetMappedResultsParams("test", "ayende", null))));
			}
		}

		[Fact]
		public void CanDelete()
		{
			using (var tx = NewTransactionalStorage())
			{
				tx.Batch(mutator => mutator.MappedResults.PutMappedResult("test", "users/ayende", "ayende", RavenJObject.FromObject(new { Name = "Rahien" }), null));
				tx.Batch(mutator => Assert.NotEmpty(mutator.MappedResults.DeleteMappedResultsForDocumentId("users/ayende","test")));

				tx.Batch(viewer => Assert.Empty(viewer.MappedResults.GetMappedResults(new GetMappedResultsParams("test", "ayende", null))));
			}
		}

		[Fact]
		public void CanDeletePerView()
		{
			using (var tx = NewTransactionalStorage())
			{
				tx.Batch(mutator => mutator.MappedResults.PutMappedResult("test", "users/ayende", "ayende", RavenJObject.FromObject(new { Name = "Rahien" }), null));
				tx.Batch(mutator => mutator.MappedResults.DeleteMappedResultsForView("test"));

				tx.Batch(viewer => Assert.Empty(viewer.MappedResults.GetMappedResults(new GetMappedResultsParams("test", "ayende", null))));
			}
		}

		[Fact]
		public void CanHaveTwoResultsForSameDoc()
		{
			using (var tx = NewTransactionalStorage())
			{
				tx.Batch(mutator => mutator.MappedResults.PutMappedResult("test", "users/ayende", "ayende", RavenJObject.FromObject(new { Name = "Rahien" }), null));
				tx.Batch(mutator => mutator.MappedResults.PutMappedResult("test", "users/ayende", "ayende", RavenJObject.FromObject(new { Name = "Rahien" }), null));

				tx.Batch(viewer => Assert.Equal(2, viewer.MappedResults.GetMappedResults(new GetMappedResultsParams("test", "ayende", null)).Count()));
			}
		}

		[Fact]
		public void CanStoreAndGetMappedResultWithSeveralResultsForSameReduceKey()
		{
			using (var tx = NewTransactionalStorage())
			{
				tx.Batch(mutator =>
				{
					mutator.MappedResults.PutMappedResult("test", "users/ayende", "ayende", RavenJObject.FromObject(new {Name = "Rahien"}),
					                                      null);
					mutator.MappedResults.PutMappedResult("test", "users/rahien", "ayende", RavenJObject.FromObject(new { Name = "Rahien" }),
														  null);
				});

				tx.Batch(viewer => Assert.Equal(2, viewer.MappedResults.GetMappedResults(new GetMappedResultsParams("test", "ayende", null)).Count()));
			}
		}
	}
}