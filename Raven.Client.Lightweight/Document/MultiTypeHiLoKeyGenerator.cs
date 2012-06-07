//-----------------------------------------------------------------------
// <copyright file="MultiTypeHiLoKeyGenerator.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Raven.Client.Connection;

namespace Raven.Client.Document
{
	/// <summary>
	/// Generate a hilo key for each given type
	/// </summary>
	public class MultiTypeHiLoKeyGenerator
	{
		private readonly IDatabaseCommands databaseCommands;
		private readonly IDocumentStore documentStore;
		private readonly int capacity;
		private readonly object generatorLock = new object();
		private IDictionary<string, HiLoKeyGenerator> keyGeneratorsByTag = new Dictionary<string, HiLoKeyGenerator>();

		private IDatabaseCommands DatabaseCommands
		{
			get { return databaseCommands ?? documentStore.DatabaseCommands; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiTypeHiLoKeyGenerator"/> class.
		/// </summary>
		public MultiTypeHiLoKeyGenerator(IDatabaseCommands databaseCommands, int capacity)
		{
			this.databaseCommands = databaseCommands;
			this.capacity = capacity;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiTypeHiLoKeyGenerator"/> class.
		/// </summary>
		public MultiTypeHiLoKeyGenerator(IDocumentStore documentStore, int capacity)
		{
			this.documentStore = documentStore;
			this.capacity = capacity;
		}


		/// <summary>
		/// Generates the document key.
		/// </summary>
		/// <param name="conventions">The conventions.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public string GenerateDocumentKey(DocumentConvention conventions, object entity)
		{
		    var typeTagName = conventions.GetTypeTagName(entity.GetType());
			if (string.IsNullOrEmpty(typeTagName)) //ignore empty tags
				return null;
			var tag = conventions.TransformTypeTagNameToDocumentKeyPrefix(typeTagName);
			HiLoKeyGenerator value;
			if (keyGeneratorsByTag.TryGetValue(tag, out value))
				return value.GenerateDocumentKey(conventions, entity);

			lock(generatorLock)
			{
				if (keyGeneratorsByTag.TryGetValue(tag, out value))
					return value.GenerateDocumentKey(conventions, entity);

				value = new HiLoKeyGenerator(DatabaseCommands, tag, capacity);
				// doing it this way for thread safety
				keyGeneratorsByTag = new Dictionary<string, HiLoKeyGenerator>(keyGeneratorsByTag)
				{
					{tag, value}
				};
			}

			return value.GenerateDocumentKey(conventions, entity);
		}
	}
}
