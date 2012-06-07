﻿using System;
using System.Transactions;
using Raven.Client.Document;
using Raven.Tests.Bugs;
using Xunit;

namespace Raven.Tests.Track
{
	public class RavenDB17 : RavenTest
	{
		[Fact]
		public void CacheRespectInFlightTransaction()
		{
			using (GetNewServer())
			using (var store = new DocumentStore
			{
				Url = "http://localhost:8079"
			}.Initialize())
			{
				// Session #1
				using (var scope = new TransactionScope())
				using (var session = store.OpenSession())
				{
					System.Transactions.Transaction.Current.EnlistDurable(ManyDocumentsViaDTC.DummyEnlistmentNotification.Id,
																  new ManyDocumentsViaDTC.DummyEnlistmentNotification(),
																  EnlistmentOptions.None);


					session.Advanced.UseOptimisticConcurrency = true;
					session.Advanced.AllowNonAuthoritativeInformation = false;

					session.Store(new SomeDocument() { Id = 1, Data = "Data1" });

					session.SaveChanges();
					scope.Complete();
				}

				// Session #2
				using (var scope = new TransactionScope())
				using (var session = store.OpenSession())
				{
					System.Transactions.Transaction.Current.EnlistDurable(ManyDocumentsViaDTC.DummyEnlistmentNotification.Id,
																  new ManyDocumentsViaDTC.DummyEnlistmentNotification(),
																  EnlistmentOptions.None);

					session.Advanced.UseOptimisticConcurrency = true;
					session.Advanced.AllowNonAuthoritativeInformation = false;

					var doc = session.Load<SomeDocument>(1);
					if (doc.Data != "Data1")
						throw new InvalidOperationException("Should be Data1");

					doc.Data = "Data2";

					session.SaveChanges();
					scope.Complete();
				}


				// Session #3
				using (var scope = new TransactionScope())
				using (var session = store.OpenSession())
				{
					System.Transactions.Transaction.Current.EnlistDurable(ManyDocumentsViaDTC.DummyEnlistmentNotification.Id,
																  new ManyDocumentsViaDTC.DummyEnlistmentNotification(),
																  EnlistmentOptions.None);

					session.Advanced.UseOptimisticConcurrency = true;
					session.Advanced.AllowNonAuthoritativeInformation = false;

					var doc = session.Load<SomeDocument>(1);
					Assert.Equal("Data2", doc.Data);

					session.SaveChanges();
					scope.Complete();
				}
			}
		}

		public class SomeDocument
		{
			public string Data { get; set; }

			public int Id { get; set; }
		}
	}
}