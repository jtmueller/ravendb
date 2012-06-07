<<<<<<< HEAD
﻿using System.Collections.Generic;

namespace Raven.Abstractions.Data
{
	public class FacetSetup
	{
		public string Id { get; set; }
		public List<Facet> Facets { get; set; }
	}
=======
﻿using System.Collections.Generic;

namespace Raven.Abstractions.Data
{
	public class FacetSetup
	{
		public string Id { get; set; }
		public List<Facet> Facets { get; set; }

		public FacetSetup()
		{
			Facets = new List<Facet>();
		}
	}
>>>>>>> upstream/master
}