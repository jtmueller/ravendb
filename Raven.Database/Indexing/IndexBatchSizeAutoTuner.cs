using System;
using Raven.Database.Config;
using System.Linq;
using System.Collections.Generic;

namespace Raven.Database.Indexing
{
	public class IndexBatchSizeAutoTuner : BaseBatchSizeAutoTuner
	{
<<<<<<< HEAD
		private readonly WorkContext context;
		private int numberOfItemsToIndexInSingleBatch;
		
=======
>>>>>>> upstream/master
		public IndexBatchSizeAutoTuner(WorkContext context)
			: base(context)
		{
		}

		protected override int InitialNumberOfItems
		{
			get { return context.Configuration.InitialNumberOfItemsToIndexInSingleBatch; }
		}

		protected override int MaxNumberOfItems
		{
<<<<<<< HEAD
			try
			{
				if (ReduceBatchSizeIfCloseToMemoryCeiling())
					return;
				if (ConsiderDecreasingBatchSize(amountOfItemsToIndex))
					return;
				ConsiderIncreasingBatchSize(amountOfItemsToIndex, size);
			}
			finally
			{
				context.Configuration.IndexingScheduler.LastAmountOfItemsToIndex = amountOfItemsToIndex;
			}
=======
			get { return context.Configuration.MaxNumberOfItemsToIndexInSingleBatch; }
>>>>>>> upstream/master
		}

		protected override int CurrentNumberOfItems
		{
<<<<<<< HEAD
			if (amountOfItemsToIndex < NumberOfItemsToIndexInSingleBatch)
			{
				return;
			}

			if (context.Configuration.IndexingScheduler.LastAmountOfItemsToIndex < NumberOfItemsToIndexInSingleBatch)
			{
				// this is the first time we hit the limit, we will give another go before we increase
				// the batch size
				return;
			}

			// in the previous run, we also hit the current limit, we need to check if we can increase the max batch size

			// here we make the assumptions that the average size of documents are the same. We check if we doubled the amount of memory
			// that we used for the last batch (note that this is only an estimate number, but should be close enough), would we still be
			// within the limits that governs us

			var sizeInMegabytes = size / 1024 / 1024;

			// we don't actually *know* what the actual cost of indexing, beause that depends on many factors (how the index
			// is structured, is it analyzed/default/not analyzed, etc). We just assume for now that it takes 25% of the actual
			// on disk structure per each active index. That should give us a good guesstimate about the value.
			// Because of the way we are executing indexes, only N are running at once, where N is the parallel level, so we take
			// that into account, you may have 10 indexes but only 2 CPUs, so we only consider the cost of executing 2 indexes,
			// not all 10
			var sizedPlusIndexingCost = sizeInMegabytes * (1 + (0.25 * Math.Min(context.IndexDefinitionStorage.IndexesCount, context.Configuration.MaxNumberOfParallelIndexTasks)));

			var remainingMemoryAfterBatchSizeIncrease = MemoryStatistics.AvailableMemory - sizedPlusIndexingCost;

			if (remainingMemoryAfterBatchSizeIncrease >= context.Configuration.AvailableMemoryForRaisingIndexBatchSizeLimit)
			{
				NumberOfItemsToIndexInSingleBatch = Math.Min(context.Configuration.MaxNumberOfItemsToIndexInSingleBatch,
															 NumberOfItemsToIndexInSingleBatch * 2);
				return;
			}

			
=======
			get { return context.CurrentNumberOfItemsToIndexInSingleBatch; }
			set { context.CurrentNumberOfItemsToIndexInSingleBatch = value; }
>>>>>>> upstream/master
		}

		protected override int LastAmountOfItemsToRemember
		{
			get { return context.Configuration.IndexingScheduler.LastAmountOfItemsToIndexToRemember; }
			set { context.Configuration.IndexingScheduler.LastAmountOfItemsToIndexToRemember = value; }
		}

		protected override void RecordAmountOfItems(int numberOfItems)
		{
<<<<<<< HEAD
			if (amountOfItemsToIndex >= NumberOfItemsToIndexInSingleBatch)
			{
				// we had as much work to do as we are currently capable of handling
				// there isn't nothing that we need to do here...
				return false;
			}

			// we didn't have a lot of work to do, so let us see if we can reduce the batch size

			// we are at the configured minimum, nothing to do
			if (NumberOfItemsToIndexInSingleBatch == context.Configuration.InitialNumberOfItemsToIndexInSingleBatch)
				return true;

			// we were above the max the last time, we can't reduce the work load now
			if (context.Configuration.IndexingScheduler.LastAmountOfItemsToIndex > NumberOfItemsToIndexInSingleBatch)
				return true;

			var old = NumberOfItemsToIndexInSingleBatch;
			// we have had a couple of times were we didn't get to the current max, so we can probably
			// reduce the max again now, this will reduce the memory consumption eventually, and will cause 
			// faster indexing times in case we get a big batch again
			NumberOfItemsToIndexInSingleBatch = Math.Max(context.Configuration.InitialNumberOfItemsToIndexInSingleBatch,
														 NumberOfItemsToIndexInSingleBatch / 2);

			// we just reduced the batch size because we have two concurrent runs where we had
			// less to do than the previous runs. That indicate the the busy period is over, maybe we
			// run out of data? Or the rate of data entry into the system was just reduce?
			// At any rate, there is a strong likelyhood of having a lot of garbage in the system
			// let us ask the GC nicely to clean it

			// but we only want to do it if the change was significant 
			if ( NumberOfItemsToIndexInSingleBatch - old > 4096)
			{
				GC.Collect(1, GCCollectionMode.Optimized);
			}
=======
			context.Configuration.IndexingScheduler.RecordAmountOfItemsToIndex(numberOfItems);
		}
>>>>>>> upstream/master

		protected override IEnumerable<int> GetLastAmountOfItems()
		{
			return context.Configuration.IndexingScheduler.GetLastAmountOfItemsToIndex();
		}
	}
}
