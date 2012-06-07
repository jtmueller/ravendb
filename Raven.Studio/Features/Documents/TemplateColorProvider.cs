using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Raven.Studio.Features.Documents
{
	public class TemplateColorProvider
	{
		public static TemplateColorProvider Instance = new TemplateColorProvider();

		const double GoldenAngle = 0.381966;

		readonly Dictionary<string, double> baseHues = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);
		readonly Dictionary<string, Brush> colors = new Dictionary<string, Brush>(StringComparer.InvariantCultureIgnoreCase);

		public TemplateColorProvider()
		{
			colors.Add("Sys doc", new SolidColorBrush(new Color { R = 0x12, G = 0x3c, B = 0x65, A = 0xff }));
			colors.Add("Orphans", new SolidColorBrush(new Color { R = 0xbf, G = 0x40, B = 0x40, A = 0xff }));
		}

		public Brush ColorFrom(string key)
		{
			if (!colors.ContainsKey(key))
			{
				var s = 1.0;
				var v = 0.66;
				var h = BaseHueFor(key);

				colors[key] = new SolidColorBrush(ColorFromHSV(h, s, v));
			}

			return colors[key];
		}

		double BaseHueFor(string key)
		{
			if (!baseHues.ContainsKey(key))
			{
				var index = baseHues.Count + 1;
				var angle = index * GoldenAngle;
				var hue = angle - Math.Floor(angle);
				baseHues[key] = hue;
			}

			return baseHues[key];
		}

		public static Color ColorFromHSV(double hue, double saturation, double value)
		{
			var hi = Convert.ToInt32(Math.Floor(hue * 6)) % 6;

			value = value * 255;
			var v = Convert.ToByte(value);
			var p = Convert.ToByte(value * (1 - saturation));
			var q = Convert.ToByte(value * (1 - hue * saturation));
			var t = Convert.ToByte(value * (1 - (1 - hue) * saturation));

			switch (hi)
			{
				case 0:
					return Color.FromArgb(255, v, t, p);
				case 1:
					return Color.FromArgb(255, q, v, p);
				case 2:
					return Color.FromArgb(255, p, v, t);
				case 3:
					return Color.FromArgb(255, p, q, v);
				case 4:
					return Color.FromArgb(255, t, p, v);
				default:
					return Color.FromArgb(255, v, p, q);
			}
		}
	}
}