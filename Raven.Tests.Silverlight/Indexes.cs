﻿namespace Raven.Tests.Silverlight
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Linq;
	using Client.Document;
	using Client.Extensions;
	using Database.Indexing;
	using Microsoft.Silverlight.Testing;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class Indexes : RavenTestBase
	{
		[Asynchronous]
		public IEnumerable<Task> Can_get_index_names_async()
		{
			var dbname = GenerateNewDatabaseName();
			var documentStore = new DocumentStore {Url = Url + Port};
			documentStore.Initialize();
			yield return documentStore.AsyncDatabaseCommands.EnsureDatabaseExistsAsync(dbname);

			var task = documentStore.AsyncDatabaseCommands.ForDatabase(dbname).GetIndexNamesAsync(0, 25);
			yield return task;

			Assert.AreEqual("Raven/DocumentsByEntityName", task.Result[0]);
		}

		[Asynchronous]
		public IEnumerable<Task> Can_get_indexes_async()
		{
			var dbname = GenerateNewDatabaseName();
			var documentStore = new DocumentStore {Url = Url + Port};
			documentStore.Initialize();
			yield return documentStore.AsyncDatabaseCommands.EnsureDatabaseExistsAsync(dbname);

			var task = documentStore.AsyncDatabaseCommands.ForDatabase(dbname).GetIndexesAsync(0, 25);
			yield return task;

			Assert.AreEqual("Raven/DocumentsByEntityName", task.Result[0].Name);
		}

		[Asynchronous]
		public IEnumerable<Task> Can_put_an_index_async()
		{
			var dbname = GenerateNewDatabaseName();
			var documentStore = new DocumentStore {Url = Url + Port};
			documentStore.Initialize();
			yield return documentStore.AsyncDatabaseCommands.EnsureDatabaseExistsAsync(dbname);

			yield return documentStore.AsyncDatabaseCommands
				.ForDatabase(dbname)
				.PutIndexAsync("Test", new IndexDefinition
				                       	{
				                       		Map = "from doc in docs.Companies select new { doc.Name }"
				                       	}, true);

			var verification = documentStore.AsyncDatabaseCommands
				.ForDatabase(dbname)
				.GetIndexNamesAsync(0, 25);
			yield return verification;

			Assert.IsTrue(verification.Result.Contains("Test"));
		}

		[Asynchronous]
		public IEnumerable<Task> Can_delete_an_index_async()
		{
			var dbname = GenerateNewDatabaseName();
			var documentStore = new DocumentStore {Url = Url + Port};
			documentStore.Initialize();
			yield return documentStore.AsyncDatabaseCommands.EnsureDatabaseExistsAsync(dbname);

			yield return documentStore.AsyncDatabaseCommands
				.ForDatabase(dbname)
				.PutIndexAsync("Test", new IndexDefinition
				                       	{
				                       		Map = "from doc in docs.Companies select new { doc.Name }"
				                       	}, true);

			var verify_put = documentStore.AsyncDatabaseCommands
				.ForDatabase(dbname)
				.GetIndexNamesAsync(0, 25);
			yield return verify_put;

			Assert.IsTrue(verify_put.Result.Contains("Test"));

			yield return documentStore.AsyncDatabaseCommands
				.ForDatabase(dbname)
				.DeleteIndexAsync("Test");

			var verify_delete = documentStore.AsyncDatabaseCommands
				.ForDatabase(dbname)
				.GetIndexNamesAsync(0, 25);
			yield return verify_delete;

			//NOTE: this is failing because Silverlight is caching the response from the first verification
			Assert.IsFalse(verify_delete.Result.Contains("Test"));
		}
	}
}