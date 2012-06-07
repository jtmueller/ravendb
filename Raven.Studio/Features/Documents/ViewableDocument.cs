﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Abstractions.Data;
using Raven.Json.Linq;
using Raven.Studio.Framework;
using Raven.Studio.Infrastructure;
using System.Linq;
using Raven.Studio.Models;

namespace Raven.Studio.Features.Documents
{
	public class FriendlyDocument
	{
		public string Id { get; set; }
		public List<string> NeighborsIds { get; set; }
		public bool IsProjection { get; set; }
	}

	public class ViewableDocument : NotifyPropertyChangedBase, IDisposable
	{
		private readonly JsonDocument inner;
		private string id;
		private string clrType;
		private string collectionType;
		private IDisposable disposable;

		public ViewableDocument(JsonDocument inner)
		{
			this.inner = inner;

			Id = inner.Metadata.IfPresent<string>("@id");
			LastModified = inner.LastModified ?? DateTime.MinValue;
			if (LastModified.Kind == DateTimeKind.Utc)
				LastModified = LastModified.ToLocalTime();
			ClrType = inner.Metadata.IfPresent<string>(Constants.RavenClrType);
			CollectionType = DetermineCollectionType(inner.Metadata);

			disposable = Observable.FromEventPattern<EventHandler, EventArgs>(e => DocumentSize.Current.SizeChanged += e, e => DocumentSize.Current.SizeChanged -= e)
				.Throttle(TimeSpan.FromSeconds(0.5))
				.Subscribe(_ => CalculateData());

			CalculateData();
			ToolTipText = ShortViewOfJson.GetContentDataWithMargin(inner.DataAsJson, 10);
		}

		Brush fill;
		public Brush Fill
		{
			get { return fill ?? (fill = TemplateColorProvider.Instance.ColorFrom(CollectionType)); }
		}

		public ICommand Edit
		{
			get { return new EditDocumentCommand(this); }
		}

		private string toolTipText;
		public string ToolTipText
		{
			get { return toolTipText; }
			set
			{
				toolTipText = value;
				OnPropertyChanged(() => ToolTipText);
			}
		}

		private string data;
		public string Data
		{
			get { return data; }
			set
			{
				data = value;
				OnPropertyChanged(() => Data);
			}
		}

		private void CalculateData()
		{
			string d = null;
			if (DocumentSize.Current.Height >= DocumentSize.ExpandedMinimumHeight)
			{
				var margin = Math.Sqrt(DocumentSize.Current.Width) - 4;
				d = ShortViewOfJson.GetContentDataWithMargin(inner.DataAsJson, (int)margin);
			}
			Execute.OnTheUI(() => Data = d);
		}

		public string DisplayId
		{
			get
			{
				if (string.IsNullOrEmpty(Id))
				{
					// this is projection, try to find something meaningful.
					return GetMeaningfulDisplayIdForProjection();
				}

				var display = GetIdWithoutPrefixes();

				Guid guid;
				if (Guid.TryParse(display, out guid))
				{
					display = display.Substring(0, 8);
				}
				return display;
			}
		}

		private string GetMeaningfulDisplayIdForProjection()
		{
			var selectedProperty = new KeyValuePair<string, RavenJToken>();
			var propertyNames = new[] {"Id", "Name"};
			foreach (var propertyName in propertyNames)
			{
				selectedProperty =
					inner.DataAsJson.FirstOrDefault(x => x.Key.EndsWith(propertyName, StringComparison.InvariantCultureIgnoreCase));
				if (selectedProperty.Key != null)
				{
					break;
				}
			}

			if (selectedProperty.Key == null) // couldn't find anything, we will use the first one
			{
				selectedProperty = inner.DataAsJson.FirstOrDefault();
			}

			if (selectedProperty.Key == null) // there aren't any properties 
			{
				return "{}";
			}
			string value = selectedProperty.Value.Type==JTokenType.String ? 
				selectedProperty.Value.Value<string>() : 
				selectedProperty.Value.ToString(Formatting.None);
			if (value.Length > 30)
			{
				value = value.Substring(0, 27) + "...";
			}
			return value;
		}

		private string GetIdWithoutPrefixes()
		{
			var display = Id;

			var prefixToRemoves = new[]
			{
				"Raven/",
				CollectionType + "/",
				CollectionType + "-"
			};

			foreach (var prefixToRemove in prefixToRemoves)
			{
				if (display.StartsWith(prefixToRemove, StringComparison.InvariantCultureIgnoreCase))
					display = display.Substring(prefixToRemove.Length);
			}
			return display;
		}

		public string CollectionType
		{
			get
			{
				return collectionType;
			}
			set
			{
				collectionType = value; OnPropertyChanged(() => CollectionType);
			}
		}

		public string ClrType
		{
			get
			{
				return clrType;
			}
			set
			{
				clrType = value; OnPropertyChanged(() => ClrType);
			}
		}

		private DateTime lastModified;
		public DateTime LastModified
		{
			get { return lastModified; }
			set { lastModified = value; OnPropertyChanged(() => LastModified); }
		}

		public string Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged(() => Id); }
		}

		public JsonDocument InnerDocument
		{
			get { return inner; }
		}

		public List<string> NeighborsIds { get; set; }

		public override string ToString()
		{
			return inner.DataAsJson.ToString();
		}

		public void Dispose()
		{
			if (disposable != null)
				disposable.Dispose();
		}

		public static string DetermineCollectionType(RavenJObject metadata)
		{
			var id = metadata.IfPresent<string>("@id") ?? string.Empty;

			if (string.IsNullOrEmpty(id))
				return "Projection"; // meaning that the document is a projection and not a 'real' document

			if (id.StartsWith("Raven/"))
				return "Sys Doc";

			var entity = metadata.IfPresent<string>(Constants.RavenEntityName);
			return entity ?? "Doc";
		}
	}
}