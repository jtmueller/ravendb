<<<<<<< HEAD
//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Raven.Client.Document;

namespace Raven.Sample.SimpleClient
{
	class Program
	{
		static void Main()
		{
			using (var documentStore = new DocumentStore { Url = "http://ipv4.fiddler:8080" })
			{
				documentStore.Initialize();

			   

			}
		}

		public class U2
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}

	}
}
=======
//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using Raven.Client.Document;
using Raven.Client.Linq;
using System.Linq;

namespace Raven.Sample.SimpleClient
{
	class Program
	{
		static void Main()
		{
			using (var documentStore = new DocumentStore
			{
				Url = "http://localhost:8079",
			}.Initialize())
			{
				for (int i = 0; i < 1000; i++)
				{
					var sp = Stopwatch.StartNew();
					using (var session = documentStore.OpenSession())
					{
						session.Load<dynamic>("users/ayende");
					}
					Console.WriteLine("{0}: {1:#,#} ms", i, sp.ElapsedMilliseconds);
					Console.ReadLine();
				}
			}
		}
	}
}
>>>>>>> upstream/master
