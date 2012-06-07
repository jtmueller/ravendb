<<<<<<< HEAD
//-----------------------------------------------------------------------
// <copyright file="AttachmentEndoding.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using Raven.Database.Server;
using Raven.Json.Linq;
using Raven.Client.Document;
using Raven.Database.Extensions;
using Xunit;

namespace Raven.Tests.Bugs
{
	public class AttachmentEndoding : RemoteClientTest, IDisposable
	{
		private readonly string path;
		private readonly int port;

		public AttachmentEndoding()
		{
			port = 8079;
			path = GetPath("TestDb");
			NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8079);
		}

		public void Dispose()
		{
			IOExtensions.DeleteDirectory(path);
		}

		[Fact]
		public void Can_get_proper_attachment_names()
		{
			using (var server = GetNewServer(port, path))
			{
				using (var documentStore = new DocumentStore { Url = "http://localhost:" + port })
				{
					documentStore.Initialize();

					documentStore.DatabaseCommands.PutAttachment("test/hello/world", null, new MemoryStream(new byte[] {1, 2, 3}),
					                                             new RavenJObject());

					using (var wc = new WebClient())
					{
						var staticJson = wc.DownloadString("http://localhost:8079/static");
						var value = RavenJArray.Parse(staticJson)[0].Value<string>("Key");
						Assert.Equal("test/hello/world", value);
					}
				}
			}
		}
	}
=======
//-----------------------------------------------------------------------
// <copyright file="AttachmentEndoding.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using Raven.Database.Server;
using Raven.Json.Linq;
using Raven.Client.Document;
using Raven.Database.Extensions;
using Xunit;

namespace Raven.Tests.Bugs
{
	public class AttachmentEndoding : RemoteClientTest, IDisposable
	{
		private readonly string path;
		private readonly int port;

		public AttachmentEndoding()
		{
			port = 8079;
			path = GetPath("TestDb");
			NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8079);
		}

		public override void Dispose()
		{
			IOExtensions.DeleteDirectory(path);
			base.Dispose();
		}

		[Fact]
		public void Can_get_proper_attachment_names()
		{
			using (var server = GetNewServer(port, path))
			{
				using (var documentStore = new DocumentStore { Url = "http://localhost:" + port })
				{
					documentStore.Initialize();

					documentStore.DatabaseCommands.PutAttachment("test/hello/world", null, new MemoryStream(new byte[] {1, 2, 3}),
					                                             new RavenJObject());

					using (var wc = new WebClient())
					{
						var staticJson = wc.DownloadString("http://localhost:8079/static");
						var value = RavenJArray.Parse(staticJson)[0].Value<string>("Key");
						Assert.Equal("test/hello/world", value);
					}
				}
			}
		}
	}
>>>>>>> upstream/master
}