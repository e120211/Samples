using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Essentials = Xamarin.Essentials;
using MyApp.Localization;

namespace MyApp.Controls
{
    public class TTabbedPage : TabbedPage
    {
        public LocalizedResources Locale { get { return App.LocResources; } }
        public bool IsInitialized { get; set; } = false;

        public bool BackOnTab { get; set; }

        #region SubTitle property
        public static readonly BindableProperty SubTitleProperty = BindableProperty.Create(nameof(SubTitle), typeof(string), typeof(TContentPage), string.Empty);
        public string SubTitle
        {
            get { return (string)GetValue(SubTitleProperty); }
            set { SetValue(SubTitleProperty, value); }
        }
        #endregion

        #region SubTitleIcon property
        public static readonly BindableProperty SubTitleIconProperty = BindableProperty.Create(nameof(SubTitleIcon), typeof(string), typeof(TContentPage), string.Empty);
        public string SubTitleIcon
        {
            get { return (string)GetValue(SubTitleIconProperty); }
            set { SetValue(SubTitleIconProperty, value); }
        }
        #endregion

        #region Back navigation
        public Stack<Page> TabStack { get; private set; } = new Stack<Page>();
        protected override void OnCurrentPageChanged()
        {
            if (BackOnTab)
            {
                var page = CurrentPage;
                if (page != null)
                {
                    TabStack.Push(page);
                }
            }
            base.OnCurrentPageChanged();
        }

        protected override bool OnBackButtonPressed()
        {
            if (BackOnTab)
            {
                if (TabStack.Any())
                {
                    TabStack.Pop();
                }
                if (TabStack.Any())
                {
                    CurrentPage = TabStack.Pop();
                    return true;
                }
            }
            return base.OnBackButtonPressed();
        }
        #endregion

        public TTabbedPage()
        {
            Label lblTitle = new Label()
            {
                Style = (Style)Application.Current.Resources["TLabelToolbarTitle"]
            };
            lblTitle.SetBinding(Label.TextProperty, "Title");

            Label lblSubTitle = new Label()
            {
                Style = (Style)Application.Current.Resources["TLabelToolbarSubTitle"]
            };
            lblSubTitle.SetBinding(Label.TextProperty, "SubTitle");
            lblSubTitle.SetBinding(Label.IsVisibleProperty, "SubTitle", converter: new TextToVisibleConverter());

            Label lblSubTitleIcon = new Label()
            {
                Style = (Style)Application.Current.Resources["TLabelToolbarSubTitleIcon"]
            };
            lblSubTitleIcon.SetBinding(Label.TextProperty, "SubTitleIcon");
            lblSubTitleIcon.SetBinding(Label.IsVisibleProperty, "SubTitleIcon", converter: new TextToVisibleConverter());

            NavigationPage.SetTitleView(this, new StackLayout()
            {
                Spacing = 0,
                VerticalOptions = LayoutOptions.Fill,
                Children = { lblTitle, new StackLayout() {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 6,
                    Children = { lblSubTitleIcon, lblSubTitle }
                } }
            });
        }
    }
}