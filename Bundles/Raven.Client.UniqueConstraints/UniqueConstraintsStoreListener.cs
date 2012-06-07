<<<<<<< HEAD
﻿namespace Raven.Client.UniqueConstraints
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using Listeners;
	using Json.Linq;

	using Constants = Bundles.UniqueConstraints.Constants;

	public class UniqueConstraintsStoreListener : IDocumentStoreListener
	{
		private static readonly ConcurrentDictionary<Type, string[]> TypeProperties = new ConcurrentDictionary<Type, string[]>();

		public bool BeforeStore(string key, object entityInstance, RavenJObject metadata)
		{
			if (metadata[Constants.EnsureUniqueConstraints] != null)
			{
				return true;
			}

			var type = entityInstance.GetType();

			string[] properties;
			if (!TypeProperties.TryGetValue(type, out properties))
			{
				properties = TypeProperties.GetOrAdd(type, type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(UniqueConstraintAttribute))).Select(x => x.Name).ToArray());
			}

			metadata.Add(Constants.EnsureUniqueConstraints, new RavenJArray(properties));

			return true;
		}

		public void AfterStore(string key, object entityInstance, RavenJObject metadata)
		{
		}
	}
}
=======
﻿namespace Raven.Client.UniqueConstraints
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using Listeners;
	using Json.Linq;

	using Constants = Bundles.UniqueConstraints.Constants;

	public class UniqueConstraintsStoreListener : IDocumentStoreListener
	{
		public bool BeforeStore(string key, object entityInstance, RavenJObject metadata, RavenJObject original)
		{
			if (metadata[Constants.EnsureUniqueConstraints] != null)
			{
				return true;
			}

			var type = entityInstance.GetType();

			var properties = UniqueConstraintsTypeDictionary.GetProperties(type);

			if (properties != null)
			{
				metadata.Add(Constants.EnsureUniqueConstraints, new RavenJArray(properties.Select(x => x.Name).ToArray()));
			}

			return true;
		}

		public void AfterStore(string key, object entityInstance, RavenJObject metadata)
		{
		}
	}
}
>>>>>>> upstream/master
