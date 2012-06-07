<<<<<<< HEAD
//-----------------------------------------------------------------------
// <copyright file="HiLoKeyGenerator.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Threading;
using System.Transactions;
#if !NET_3_5
using System.Threading.Tasks;
using Raven.Client.Connection.Async;
#endif
using Newtonsoft.Json.Linq;
using Raven.Abstractions.Data;
using Raven.Abstractions.Exceptions;
using Raven.Abstractions.Extensions;
using Raven.Client.Connection;
using Raven.Json.Linq;

namespace Raven.Client.Document
{
	/// <summary>
	/// Generate hilo numbers against a RavenDB document
	/// </summary>
	public class HiLoKeyGenerator
	{
		private const string RavenKeyGeneratorsHilo = "Raven/Hilo/";
		private readonly string tag;
		private long capacity;
		private readonly object generatorLock = new object();
		private long current;
		private volatile Hodler currentMax = new Hodler(0);
		private DateTime lastRequestedUtc;
		private IDatabaseCommands databaseCommands;

		private class Hodler
		{
			public readonly long Value;

			public Hodler(long value)
			{
				Value = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HiLoKeyGenerator"/> class.
		/// </summary>
		public HiLoKeyGenerator(IDatabaseCommands databaseCommands, string tag, long capacity)
		{
			this.databaseCommands = databaseCommands;
			this.tag = tag;
			this.capacity = capacity;
			current = 0;
		}

		/// <summary>
		/// Generates the document key.
		/// </summary>
		/// <param name="convention">The convention.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public string GenerateDocumentKey(DocumentConvention convention, object entity)
		{
			return string.Format("{0}{1}{2}",
								 tag,
								 convention.IdentityPartsSeparator,
								 NextId());
		}

		///<summary>
		/// Create the next id (numeric)
		///</summary>
		public long NextId()
		{
			long incrementedCurrent = Interlocked.Increment(ref current);
			while (incrementedCurrent > currentMax.Value)
			{
				lock (generatorLock)
				{
					if (current > currentMax.Value)
					{
						currentMax = new Hodler(GetNextMax());
						incrementedCurrent = current;
					}
					else
					{
						incrementedCurrent = Interlocked.Increment(ref current);
					}
				}
			}
			return incrementedCurrent;
		}

		private long GetNextMax()
		{
			using (new TransactionScope(TransactionScopeOption.Suppress))
			{
			var span = DateTime.UtcNow - lastRequestedUtc;
			if (span.TotalSeconds < 1)
			{
				capacity *= 2;
			}

			lastRequestedUtc = DateTime.UtcNow;
			while (true)
			{
				try
				{
					var document = GetDocument();
					if (document == null)
					{
						PutDocument(new JsonDocument
						{
							Etag = Guid.Empty,
							// sending empty guid means - ensure the that the document does NOT exists
							Metadata = new RavenJObject(),
							DataAsJson = RavenJObject.FromObject(new {Max = capacity}),
							Key = RavenKeyGeneratorsHilo + tag
						});
						return capacity;
					}
					long max;
					if (document.DataAsJson.ContainsKey("ServerHi")) // convert from hi to max
					{
						var hi = document.DataAsJson.Value<long>("ServerHi");
						max = ((hi - 1)*capacity);
						document.DataAsJson.Remove("ServerHi");
						document.DataAsJson["Max"] = max;
					}
					max = document.DataAsJson.Value<long>("Max");
					document.DataAsJson["Max"] = max + capacity;
					PutDocument(document);

					current = max + 1;
					return max + capacity;
				}
				catch (ConcurrencyException)
				{
					// expected, we need to retry
				}
			}
		}
		}

		private void PutDocument(JsonDocument document)
		{
			databaseCommands.Put(RavenKeyGeneratorsHilo + tag, document.Etag,
			                     document.DataAsJson,
			                     document.Metadata);
		}

		private JsonDocument GetDocument()
		{
			return databaseCommands.Get(RavenKeyGeneratorsHilo + tag);
		}
	}
}
=======
//-----------------------------------------------------------------------
// <copyright file="HiLoKeyGenerator.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using System.Threading;
using System.Transactions;
#if !NET_3_5
using System.Threading.Tasks;
using Raven.Client.Connection.Async;
#endif
using Raven.Abstractions.Data;
using Raven.Abstractions.Exceptions;
using Raven.Abstractions.Extensions;
using Raven.Client.Connection;
using Raven.Client.Exceptions;
using Raven.Json.Linq;

namespace Raven.Client.Document
{
	/// <summary>
	/// Generate hilo numbers against a RavenDB document
	/// </summary>
	public class HiLoKeyGenerator : HiLoKeyGeneratorBase
	{
		private readonly IDatabaseCommands databaseCommands;
		private readonly object generatorLock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="HiLoKeyGenerator"/> class.
		/// </summary>
		public HiLoKeyGenerator(IDatabaseCommands databaseCommands, string tag, long capacity)
			: base(tag, capacity)
		{
			this.databaseCommands = databaseCommands;
		}

		/// <summary>
		/// Generates the document key.
		/// </summary>
		/// <param name="convention">The convention.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public string GenerateDocumentKey(DocumentConvention convention, object entity)
		{
			return GetDocumentKeyFromId(convention, NextId());
		}

		///<summary>
		/// Create the next id (numeric)
		///</summary>
		public long NextId()
		{
			while (true)
			{
				var myRange = range; // thread safe copy

				var current = Interlocked.Increment(ref myRange.Current);
				if (current <= myRange.Max)
					return current;

				lock (generatorLock)
				{
					if (range != myRange)
						// Lock was contended, and the max has already been changed. Just get a new id as usual.
						continue;

					range = GetNextRange();
				}
			}
		}

		private Range GetNextRange()
		{
			using (new TransactionScope(TransactionScopeOption.Suppress))
			{
				IncreaseCapacityIfRequired();
				while (true)
				{
					try
					{
						var minNextMax = range.Max;
						JsonDocument document;

						try
						{
							document = GetDocument();
						}
						catch (ConflictException e)
						{
							// resolving the conflict by selecting the highest number
							var highestMax = e.ConflictedVersionIds
								.Select(conflictedVersionId => GetMaxFromDocument(databaseCommands.Get(conflictedVersionId), minNextMax))
								.Max();

							PutDocument(new JsonDocument
							{
								Etag = e.Etag,
								Metadata = new RavenJObject(),
								DataAsJson = RavenJObject.FromObject(new { Max = highestMax }),
								Key = HiLoDocumentKey
							});

							continue;
						}

						long min, max;
						if (document == null)
						{
							min = minNextMax + 1;
							max = minNextMax + capacity;
							document = new JsonDocument
							{
								Etag = Guid.Empty,
								// sending empty guid means - ensure the that the document does NOT exists
								Metadata = new RavenJObject(),
								DataAsJson = RavenJObject.FromObject(new { Max = max }),
								Key = HiLoDocumentKey
							};
						}
						else
						{
							var oldMax = GetMaxFromDocument(document, minNextMax);
							min = oldMax + 1;
							max = oldMax + capacity;

							document.DataAsJson["Max"] = max;
						}
						PutDocument(document);

						return new Range(min, max);
					}
					catch (ConcurrencyException)
					{
						// expected, we need to retry
					}
				}
			}
		}

		private void PutDocument(JsonDocument document)
		{
			databaseCommands.Put(HiLoDocumentKey, document.Etag,
								 document.DataAsJson,
								 document.Metadata);
		}

		private JsonDocument GetDocument()
		{
			var documents = databaseCommands.Get(new[] { HiLoDocumentKey, RavenKeyServerPrefix }, new string[0]);
			return HandleGetDocumentResult(documents);
		}
	}
}
>>>>>>> upstream/master
