//-----------------------------------------------------------------------
// <copyright file="IndexTriggers.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.ComponentModel.Composition.Hosting;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Json.Linq;
using Raven.Database;
using Raven.Database.Config;
using Raven.Tests.Storage;
using Xunit;
using System.Linq;

namespace Raven.Tests.Triggers
{
	public class IndexTriggers : AbstractDocumentStorageTest
	{
		private readonly DocumentDatabase db;

		public IndexTriggers()
		{
			db = new DocumentDatabase(new RavenConfiguration
			{
				DataDirectory = DataDir,
				Container = new CompositionContainer(new TypeCatalog(
					typeof(IndexToDataTable))),
				RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true
			});
			db.SpinBackgroundWorkers();
		}

		public override void Dispose()
		{
			db.Dispose();
			base.Dispose();
		}

		[Fact]
		public void CanReplicateValuesFromIndexToDataTable()
		{
			db.PutIndex("test", new IndexDefinition
			{
				Map = "from doc in docs from prj in doc.Projects select new{Project = prj}",
				Stores = { { "Project", FieldStorage.Yes } }
			});
			db.Put("t", null, RavenJObject.Parse("{'Projects': ['RavenDB', 'NHibernate']}"), new RavenJObject(), null);

			QueryResult queryResult;
			do
			{
				queryResult = db.Query("test", new IndexQuery { Start = 0, PageSize = 2, Query = "Project:RavenDB" });
			} while (queryResult.IsStale);

			var indexToDataTable = db.IndexUpdateTriggers.OfType<IndexToDataTable>().Single();
			Assert.Equal(2, indexToDataTable.DataTable.Rows.Count);
		}
	}
}