using System;
using Raven.Tests.Bugs.Identifiers;
using System.Linq;
using Xunit;

namespace Raven.Tests.MailingList
{
	public class IisQueryLengthIssues : IisExpressTestClient
	{
		private readonly string[] errorOptions = new[]
		                                {
		                                	"Verify the configuration/system.webServer/security/requestFiltering/requestLimits@maxQueryString setting in the applicationhost.config or web.config file.",
		                                	"The length of the query string for this request exceeds the configured maxQueryStringLength value"
		                                };

		[IISExpressInstalledFact]
		public void ShouldFailGracefully()
		{
			using (var store = NewDocumentStore())
			{
				var name = new string('x', 0x1000);
				var exception = Assert.Throws<InvalidOperationException>(() => store.OpenSession().Query<User>().Where(u => u.FirstName == name).ToList());
				
				Assert.True(errorOptions.Any(s => exception.Message.Contains(s)));
			}
		}

		[IISExpressInstalledFact]
		public void ShouldFailGracefully_StaticIndex()
		{
			using (var store = NewDocumentStore())
			{
				var name = new string('x', 0x1000);
				var exception = Assert.Throws<InvalidOperationException>(() => store.OpenSession().Query<User>("test").Where(u => u.FirstName == name).ToList());
				Assert.True(errorOptions.Any(s => exception.Message.Contains(s)));
			}
		}
	}
}