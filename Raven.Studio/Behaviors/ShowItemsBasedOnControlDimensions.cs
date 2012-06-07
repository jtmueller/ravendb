using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Raven.Studio.Models;
using Raven.Studio.Infrastructure;

namespace Raven.Studio.Behaviors
{
	public class ShowItemsBasedOnControlDimensions : StudioBehavior<ListBox>
	{
		public DocumentsModel Model
		{
			get { return (DocumentsModel) GetValue(ModelProperty); }
			set { SetValue(ModelProperty, value); }
		}

		public static readonly DependencyProperty ModelProperty =
			DependencyProperty.Register("Model", typeof (DocumentsModel), typeof (ShowItemsBasedOnControlDimensions), null);

		private IDisposable disposable;

		protected override void OnAttached()
		{
			base.OnAttached();
			var events = Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(e => AssociatedObject.SizeChanged += e, e => AssociatedObject.SizeChanged -= e).NoSignature()
				.Merge(Observable.FromEventPattern<EventHandler, EventArgs>(e => DocumentSize.Current.SizeChanged += e, e => DocumentSize.Current.SizeChanged -= e))
				.Throttle(TimeSpan.FromSeconds(0.5))
				.Merge(Observable.FromEventPattern<RoutedEventHandler, EventArgs>(e => AssociatedObject.Loaded += e, e => AssociatedObject.Loaded -= e)) // Loaded should execute immediately.
				.ObserveOnDispatcher();

			disposable = events.Subscribe(_ => CalculatePageSize());
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			if (disposable != null)
				disposable.Dispose();
		}

		private void CalculatePageSize()
		{
			if (Model == null)
				return;

			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (AssociatedObject.ActualWidth == 0)
				return;
			// ReSharper restore CompareOfFloatsByEqualityOperator

			int row = (int) Math.Max(1, (AssociatedObject.ActualWidth/(DocumentSize.Current.Width + 28)));
			int column = (int) Math.Max(1, AssociatedObject.ActualHeight/(DocumentSize.Current.Height + 24));
			int pageSize = row*column;
			Model.Pager.PageSize = pageSize;
			Model.Pager.OnPagerChanged();
		}
	}
}