<<<<<<< HEAD
using Raven.Database.Plugins;

namespace Raven.Bundles.Quotas.Triggers
{
	public class DatabaseSizeDocumentDeleteTrigger : AbstractDeleteTrigger
	{
		public override void AfterDelete(string key, Abstractions.Data.TransactionInformation transactionInformation)
		{
			SizeQuotaConfiguration.GetConfiguration(Database).AfterDelete();
		}
	}
=======
using Raven.Database.Plugins;

namespace Raven.Bundles.Quotas.Size.Triggers
{
	public class DatabaseSizeDocumentDeleteTrigger : AbstractDeleteTrigger
	{
		public override void AfterDelete(string key, Abstractions.Data.TransactionInformation transactionInformation)
		{
			SizeQuotaConfiguration.GetConfiguration(Database).AfterDelete();
		}
	}
>>>>>>> upstream/master
}