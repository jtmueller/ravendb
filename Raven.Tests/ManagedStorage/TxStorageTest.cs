//-----------------------------------------------------------------------
// <copyright file="TxStorageTest.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.IO;
using Raven.Abstractions.MEF;
using Raven.Database.Config;
using Raven.Database.Impl;
using Raven.Database.Plugins;
using Raven.Database.Storage;
using Raven.Storage.Managed;

namespace Raven.Tests.ManagedStorage
{
	public class TxStorageTest
	{
		public TxStorageTest()
		{
			if(Directory.Exists("test"))
				Directory.Delete("test", true);
		}

		public ITransactionalStorage NewTransactionalStorage()
		{
			var newTransactionalStorage = new TransactionalStorage(new RavenConfiguration
			{
				DataDirectory = "test",
			}, () => { })
			{
				DocumentCodecs = new OrderedPartCollection<AbstractDocumentCodec>()
			};
			newTransactionalStorage.Initialize(new DummyUuidGenerator());
			return newTransactionalStorage;
		}
	}
}