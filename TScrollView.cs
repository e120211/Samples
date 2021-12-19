using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyApp.Controls
{
    public class TScrollView : ScrollView
    {
        private List<ContentView> TouchedElements { get; set; } = new List<ContentView>();

        public TScrollView()
        {
            this.Scrolled += (sender, args) =>
            {
                if (this.OnTop)
                {
                    this.OnScrollToTop(new ScrollEventArgs() { ScrollY = args.ScrollY, ScrollX = args.ScrollX });
                }
                if (this.OnBottom)
                {
                    this.OnScrollToBottom(new ScrollEventArgs() { ScrollY = args.ScrollY, ScrollX = args.ScrollX });
                }
            };
        }

        public void AppendTouchedElement(SwipeItemView item)
        {
            TouchedElements.Add(item);
        }

        private void ExtendedScrollView_Scrolled(object sender, ScrollEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static readonly BindableProperty OverScrollModeTypeProperty = BindableProperty.Create(nameof(OverScrollModeType), typeof(OverScrollMode), typeof(TScrollView), default(OverScrollMode), BindingMode.TwoWay, null, null);
        public OverScrollMode OverScrollModeType
        {
            get { return (OverScrollMode)GetValue(OverScrollModeTypeProperty); }
            set { SetValue(OverScrollModeTypeProperty, value); }
        }

        public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(Point), typeof(TScrollView), default(Point), BindingMode.TwoWay, null, null);
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly BindableProperty AnimateScrollProperty = BindableProperty.Create(nameof(AnimateScroll), typeof(bool), typeof(TScrollView), true, BindingMode.TwoWay, null, null);
        public bool AnimateScroll
        {
            get { return (bool)GetValue(AnimateScrollProperty); }
            set { SetValue(AnimateScrollProperty, value); }
        }

        public static readonly BindableProperty IsScrollableProperty = BindableProperty.Create(nameof(IsScrollable), typeof(bool), typeof(TScrollView), true, BindingMode.TwoWay, null, null);

        public bool IsScrollable
        {
            get { return (bool)GetValue(IsScrollableProperty); }
            set { SetValue(IsScrollableProperty, value); }
        }

        public static readonly BindableProperty OnTopProperty = BindableProperty.Create(nameof(OnTop), typeof(bool), typeof(TScrollView), true, BindingMode.TwoWay, null, null);

        public bool OnTop
        {
            get { return (bool)GetValue(OnTopProperty); }
            set { SetValue(OnTopProperty, value); }
        }

        public static readonly BindableProperty OnBottomProperty = BindableProperty.Create(nameof(OnBottom), typeof(bool), typeof(TScrollView), true, BindingMode.TwoWay, null, null);

        public bool OnBottom
        {
            get { return (bool)GetValue(OnBottomProperty); }
            set { SetValue(OnBottomProperty, value); }
        }

        public event EventHandler<ScrollEventArgs> ScrollChanged;
        public event EventHandler<ScrollEventArgs> ScrollToBottom;
        public event EventHandler<ScrollEventArgs> ScrollToTop;

        public void OnScrollChanged(ScrollEventArgs args)
        {
            ScrollChanged?.Invoke(this, args);
        }

        public void OnScrollToBottom(ScrollEventArgs args)
        {
            ScrollToBottom?.Invoke(this, args);
        }

        public void OnScrollToTop(ScrollEventArgs args)
        {
            ScrollToTop?.Invoke(this, args);
        }
    }
}