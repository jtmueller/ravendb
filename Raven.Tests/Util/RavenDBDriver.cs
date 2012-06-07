﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Raven.Client;
using Raven.Client.Document;
using Raven.Database.Extensions;

namespace Raven.Tests.Util
{
	[CLSCompliant(false)]
	public class RavenDBDriver : ProcessDriver, IDisposable
	{
		readonly string _shardName;
		readonly DocumentConvention _conventions;
		readonly string _dataDir;

		public string Url { get; private set;}

		public RavenDBDriver(string shardName, DocumentConvention conventions)
		{
			_shardName = shardName;
			_conventions = conventions;
			_dataDir = GetPath(shardName);
		}

		public void Start() 
		{
			IOExtensions.DeleteDirectory(_dataDir);

			var exePath = GetPath("Raven.Server.Exe");

			if (!File.Exists(exePath))
			{
				throw new Exception("Could not find Raven.server.exe");
			}

			var configPath = exePath + ".config";

			if (!File.Exists(configPath))
			{
				File.WriteAllText(configPath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <appSettings>
	<add key=""Raven/DataDir"" value=""~/Data"" />
	<add key=""Raven/AnonymousAccess"" value=""All"" />
  </appSettings>
  <runtime>
	<loadFromRemoteSources enabled=""true"" />
	<assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
	  <probing privatePath=""Analyzers"" />
	</assemblyBinding>
  </runtime>
</configuration>
");
			}

			var doc = System.Xml.Linq.XDocument.Load(configPath);

			var configSettings = doc.Root.Element("appSettings").Elements("add");
			var dataDirSetting = configSettings.Where(e => e.Attribute("key").Value.ToLower() == "raven/datadir").Single();

			dataDirSetting.SetAttributeValue("value", _dataDir);

			doc.Save(configPath);

			StartProcess(exePath);

			Match match = WaitForConsoleOutputMatching(@"^Server Url: (http://.*/)\s*$");

			Url = match.Groups[1].Value;
		}

		public IDocumentStore GetDocumentStore()
		{
			var documentStore = new DocumentStore()
			{
				Identifier = _shardName,
				Url = this.Url,
				Conventions = _conventions
			};

			documentStore.Initialize();

			return documentStore;
		}

		public void TraceExistingOutput()
		{
			Console.WriteLine("Console output:");
			Console.WriteLine(_process.StandardOutput.ReadToEnd());
			Console.WriteLine("Error output:");
			Console.WriteLine(_process.StandardError.ReadToEnd());
		}

		public void Should_finish_without_error()
		{
			_process.StandardInput.Write("q\r\n");

			if (!_process.WaitForExit(10000))
				throw new Exception("RavenDB command-line server did not halt within 10 seconds of pressing enter.");

			if (_process.ExitCode != 0)
				throw new Exception("RavenDB command-line server finished with exit code: " + _process.ExitCode);
			
			string errorOutput = _process.StandardError.ReadToEnd();
			
			if (!String.IsNullOrEmpty(errorOutput))
				throw new Exception("RavendB command-line server finished with error text: " + errorOutput);
		}

		protected override void Shutdown()
		{
			Should_finish_without_error();
		}

		protected string GetPath(string subFolderName)
		{
			string retPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RavenDBDriver)).CodeBase);
			return Path.Combine(retPath, subFolderName).Substring(6); //remove leading file://
		}

	}
}
