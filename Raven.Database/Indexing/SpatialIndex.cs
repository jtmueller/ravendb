<<<<<<< HEAD
//-----------------------------------------------------------------------
// <copyright file="SpatialIndex.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Lucene.Net.Documents;
using Lucene.Net.Util;
using Lucene.Net.Spatial.Tier;
using Lucene.Net.Spatial.Tier.Projectors;


namespace Raven.Database.Indexing
{
	public static class SpatialIndex
	{
		private static readonly List<CartesianTierPlotter> Ctps = new List<CartesianTierPlotter>();
		private static readonly IProjector Projector = new SinusoidalProjector();

		private const int MinTier = 2;
		private const int MaxTier = 15;

		public const string LatField = "latitude";
		public const string LngField = "longitude";

		static SpatialIndex()
		{
			for (int tier = MinTier; tier <= MaxTier; ++tier)
			{
				Ctps.Add(new CartesianTierPlotter(tier, Projector, CartesianTierPlotter.DefaltFieldPrefix));
			}
		}

		public static string Lat(double value)
		{
			return NumericUtils.DoubleToPrefixCoded(value);
		}

		public static string Lng(double value)
		{
			return NumericUtils.DoubleToPrefixCoded(value);
		}

		public static string Tier(int id, double lat, double lng)
		{
			if (id < MinTier || id > MaxTier)
			{
				throw new ArgumentException(
					string.Format("tier id should be between {0} and {1}", MinTier, MaxTier), "id");
			}

			var boxId = Ctps[id - MinTier].GetTierBoxId(lat, lng);

			return NumericUtils.DoubleToPrefixCoded(boxId);
		}

		public static double GetDistanceMi(double x1, double y1, double x2, double y2)
		{
			return DistanceUtils.GetInstance().GetDistanceMi(x1, y1, x2, y2);
		}

		public static IEnumerable<AbstractField> Generate(double lat, double lng)
		{
			yield return new Field("latitude", Lat(lat), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS);
			yield return new Field("longitude", Lng(lng), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS);

			for (var id = MinTier; id <= MaxTier; ++id)
			{
				yield return new Field("_tier_" + id, Tier(id, lat, lng),Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS);
			}
		}
	}
}
=======
//-----------------------------------------------------------------------
// <copyright file="SpatialIndex.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Raven.Abstractions.Data;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Query;
using Spatial4n.Core.Shapes;


namespace Raven.Database.Indexing
{
	public static class SpatialIndex
	{
		internal static readonly SpatialContext RavenSpatialContext = new SpatialContext(DistanceUnits.MILES);
		private static readonly SpatialStrategy<SimpleSpatialFieldInfo> strategy;
		private static readonly SimpleSpatialFieldInfo fieldInfo;
		private static readonly int maxLength;

		static SpatialIndex()
		{
			maxLength = GeohashPrefixTree.GetMaxLevelsPossible();
			fieldInfo = new SimpleSpatialFieldInfo(Constants.SpatialFieldName);
			strategy = new RecursivePrefixTreeStrategy(new GeohashPrefixTree(RavenSpatialContext, maxLength));
		}

		public static IEnumerable<Fieldable> Generate(double? lat, double? lng)
		{
			Shape shape = RavenSpatialContext.MakePoint(lng ?? 0, lat ?? 0);
			return strategy.CreateFields(fieldInfo, shape, true, false).Where(f => f != null)
				.Concat(new[] { new Field(Constants.SpatialShapeFieldName, RavenSpatialContext.ToString(shape), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS), });
		}

		/// <summary>
		/// Make a spatial query
		/// </summary>
		/// <param name="lat"></param>
		/// <param name="lng"></param>
		/// <param name="radius">Radius, in miles</param>
		/// <returns></returns>
		public static Query MakeQuery(double lat, double lng, double radius)
		{
			return strategy.MakeQuery(new SpatialArgs(SpatialOperation.IsWithin, RavenSpatialContext.MakeCircle(lng, lat, radius)), fieldInfo);
		}

		public static Filter MakeFilter(IndexQuery indexQuery)
		{
			var spatialQry = indexQuery as SpatialIndexQuery;
			if (spatialQry == null) return null;

			var args = new SpatialArgs(SpatialOperation.IsWithin, RavenSpatialContext.MakeCircle(spatialQry.Longitude, spatialQry.Latitude, spatialQry.Radius));
			return strategy.MakeFilter(args, fieldInfo);
		}

		public static double GetDistance(double fromLat, double fromLng, double toLat, double toLng)
		{
			var ptFrom = RavenSpatialContext.MakePoint(fromLng, fromLat);
			var ptTo = RavenSpatialContext.MakePoint(toLng, toLat);
			return RavenSpatialContext.GetDistCalc().Distance(ptFrom, ptTo);
		}
	}
}
>>>>>>> upstream/master
