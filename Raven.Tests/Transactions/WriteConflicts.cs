//-----------------------------------------------------------------------
// <copyright file="WriteConflicts.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Raven.Abstractions.Data;
using Raven.Abstractions.Exceptions;
using Raven.Json.Linq;
using Raven.Database;
using Raven.Database.Config;
using Raven.Tests.Storage;
using Xunit;

namespace Raven.Tests.Transactions
{
	public class WriteConflicts : AbstractDocumentStorageTest
	{
		private readonly DocumentDatabase db;

		public WriteConflicts()
		{
			db = new DocumentDatabase(new RavenConfiguration { DataDirectory = DataDir, RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true });
		}

		public override void Dispose()
		{
			db.Dispose();
			base.Dispose();
		}

		[Fact]
		public void WhileDocumentIsBeingUpdatedInTransactionCannotUpdateOutsideTransaction()
		{
			db.Put("ayende", null, RavenJObject.Parse("{ayende:'oren'}"), new RavenJObject(), null);
			var transactionInformation = new TransactionInformation { Id = Guid.NewGuid(), Timeout = TimeSpan.FromMinutes(1) };
			db.Put("ayende", null, RavenJObject.Parse("{ayende:'rahien'}"), new RavenJObject(), transactionInformation);
			Assert.Throws<ConcurrencyException>(
				() => db.Put("ayende", null, RavenJObject.Parse("{ayende:'oren'}"), new RavenJObject(), null));
		}

		[Fact]
		public void WhileDocumentIsBeingUpdatedInTransactionCannotUpdateInsideAnotherTransaction()
		{
			db.Put("ayende", null, RavenJObject.Parse("{ayende:'oren'}"), new RavenJObject(), null);
			var transactionInformation = new TransactionInformation { Id = Guid.NewGuid(), Timeout = TimeSpan.FromMinutes(1) };
			db.Put("ayende", null, RavenJObject.Parse("{ayende:'rahien'}"), new RavenJObject(), transactionInformation);
			Assert.Throws<ConcurrencyException>(
				() => db.Put("ayende", null, RavenJObject.Parse("{ayende:'oren'}"), new RavenJObject(), new TransactionInformation
				{
					Id = Guid.NewGuid(),
					Timeout = TimeSpan.FromMinutes(1)
				}));
		}


		[Fact]
		public void WhileCreatingDocumentInTransactionTryingToWriteInAnotherTransactionFail()
		{
			var transactionInformation = new TransactionInformation { Id = Guid.NewGuid(), Timeout = TimeSpan.FromMinutes(1) };
			db.Put("ayende", null, RavenJObject.Parse("{ayende:'rahien'}"), new RavenJObject(), transactionInformation);
			Assert.Throws<ConcurrencyException>(
				() => db.Put("ayende", null, RavenJObject.Parse("{ayende:'oren'}"), new RavenJObject(), new TransactionInformation
				{
					Id = Guid.NewGuid(),
					Timeout = TimeSpan.FromMinutes(1)
				}));
		}

		[Fact]
		public void WhileCreatingDocumentInTransactionTryingToWriteOutsideTransactionFail()
		{
			var transactionInformation = new TransactionInformation { Id = Guid.NewGuid(), Timeout = TimeSpan.FromMinutes(1) };
			db.Put("ayende", null, RavenJObject.Parse("{ayende:'rahien'}"), new RavenJObject(), transactionInformation);
			Assert.Throws<ConcurrencyException>(
				() => db.Put("ayende", null, RavenJObject.Parse("{ayende:'oren'}"), new RavenJObject(), null));
		}
	}
}
