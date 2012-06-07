﻿using System.ComponentModel.Composition.Hosting;
using Raven.Abstractions.Data;
using Raven.Client.Connection.Async;
using Raven.Client.Indexes;

namespace Raven.Tests.Silverlight
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Client.Document;
	using Client.Extensions;
	using Document;
	using Entities;
	using Microsoft.Silverlight.Testing;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class Collections : RavenTestBase
	{
		[Asynchronous]
		[Ignore]
		//NOTE: This expects a certain index to be present that is currently only created when Studio accesses a database 
		public IEnumerable<Task> Can_get_collections_async()
		{
			var dbname = GenerateNewDatabaseName();
			var store = new DocumentStore {Url = Url + Port};
			store.Initialize();


			yield return store.AsyncDatabaseCommands.EnsureDatabaseExistsAsync(dbname);

			using (var session = store.OpenAsyncSession(dbname))
			{
				Enumerable.Range(0, 25).ToList()
					.ForEach(i => session.Store(new Company {Id = "Companies/" + i, Name = i.ToString()}));

				Enumerable.Range(0, 25).ToList()
					.ForEach(i => session.Store(new Order {Id = "Orders/" + i, Note = i.ToString()}));

				yield return session.SaveChangesAsync();
			}

			
			Task<NameAndCount[]> task;
			do
			{
				task = store.AsyncDatabaseCommands.ForDatabase(dbname)
					.GetTermsCount("Raven/DocumentsByEntityName", "Tag", "", 25);
				yield return task;
				if (task.Result.Length == 0)
					yield return Delay(100);
			} while (task.Result.Length == 0); 

			var collections = task.Result;

			Assert.AreEqual("Companies", collections[0].Name);
			Assert.AreEqual("Orders", collections[1].Name);
			Assert.AreEqual(25, collections[0].Count);
			Assert.AreEqual(25, collections[1].Count);
		}
	}
}