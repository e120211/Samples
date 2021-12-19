using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Controls;
using MyApp.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Essentials = Xamarin.Essentials;
using MyApp.Forms;

namespace MyApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NoteListPage : TContentPage
    {
        public int PageSize = Essentials.Preferences.Get(Core.User.Id + ":notes_pagesize", AppConfig.DEFAULT_PAGE_SIZE);
        private string Direction { get { return "top"; } }

        #region TotalProperty
        public static readonly BindableProperty TotalProperty = BindableProperty.Create(nameof(Total), typeof(int), typeof(TContentPage), 0, propertyChanged: (bindable, oldValue, newValue) => (bindable as NoteListPage).TotalChange());
        public int Total
        {
            get { return (int)GetValue(TotalProperty); }
            set { SetValue(TotalProperty, value); }
        }
        protected void TotalChange()
        {
        }
        #endregion

        #region FirstId
        public Guid FirstId
        {
            get { return Items.Count > 0 ? Items.First().Id : Guid.Empty; }
        }
        #endregion

        #region LastId
        public Guid LastId
        {
            get { return Items.Count > 0 ? Items.Last().Id : Guid.Empty; }
        }
        #endregion

        #region Items
        private ObservableCollection<NoteItemEntry> _items = new ObservableCollection<NoteItemEntry>();
        public ObservableCollection<NoteItemEntry> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged(nameof(Items)); }
        }
        #endregion

        #region StatInfo property
        public static readonly BindableProperty StatInfoProperty = BindableProperty.Create(nameof(StatInfo), typeof(string), typeof(TContentPage), string.Empty);
        public string StatInfo
        {
            get { return (string)GetValue(StatInfoProperty); }
            set { SetValue(StatInfoProperty, value); }
        }
        #endregion

        #region CounterInfo property
        public static readonly BindableProperty CounterInfoProperty = BindableProperty.Create(nameof(CounterInfo), typeof(string), typeof(TContentPage), string.Empty);
        public string CounterInfo
        {
            get { return (string)GetValue(CounterInfoProperty); }
            set { SetValue(CounterInfoProperty, value); }
        }
        #endregion

        public NoteListPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!IsInitialized)
            {
                Subscribe();
                SetToolbarItems();
                ReloadList();
                SearchButtonPressed += (s, e) =>
                {
                    ReloadList();
                };
                SearchTextChanged += (s, e) =>
                {
                    ToolbarItems[0].IconImageSource = string.IsNullOrWhiteSpace(SearchText) ? "outline_close_24" : "outline_search_24";
                };
                IsInitialized = true;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        public void SetOverlay(bool value)
        {
            blockLoading.IsVisible = value;
        }

        public void SetLoading(bool value)
        {
            blockIndicator.IsVisible = value;
        }

        private void SetToolbarItems()
        {
            ToolbarItems.Clear();

            ToolbarItems.Add(new ToolbarItem(null, "outline_search_24", () =>
            {
                if (PageMode == PageModeType.BaseMode)
                {
                    PageMode = PageModeType.SearchMode;
                    ToolbarItems[0].IconImageSource = "outline_close_24";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(SearchText))
                    {
                        PageMode = PageModeType.BaseMode;
                        ToolbarItems[0].IconImageSource = "outline_search_24";
                    }
                    if (IsSearchTextChanged)
                    {
                        ReloadList();
                    }
                }
            }, ToolbarItemOrder.Primary, 0));

            ToolbarItems.Add(new ToolbarItem(null, "outline_more_horiz_24", () =>
            {
                ShowContextMenu();
            }, ToolbarItemOrder.Primary, 1));
        }

        private void Subscribe()
        {
            TMessagingCenter.Subscribe<Page, NoteItemEntry>(this, "NoteCreateItem", (page, arg) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Items.Insert(0, arg);
                    Total++;
                    UpdateStat();
                    blockNotFound.IsVisible = Items.Count == 0;
                });
            });
            TMessagingCenter.Subscribe<Page, NoteItemEntry>(this, "NoteUpdateItem", (page, arg) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    NoteItemEntry o = Items.FirstOrDefault(x => x.Id == arg.Id);
                    int i = Items.IndexOf(o);
                    if (i != -1)
                    {
                        Items[i] = arg;
                    }
                });
            });
            TMessagingCenter.Subscribe<Page, JArray>(this, "NoteDeleteItems", (page, arg) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (arg.Count > 0)
                    {
                        foreach (Guid id in arg)
                        {
                            NoteItemEntry o = Items.FirstOrDefault(x => x.Id == id);
                            TMessagingCenter.Send<Page, JObject>(this, "NoteRemoveItem_" + o.Id.ToString(), new JObject() { });
                            await Task.Delay(450 / arg.Count);
                            Items.Remove(o);
                            Total--;
                        }
                    }
                    UpdateStat();
                    if (Items.Count < 5)
                    {
                        RefreshList();
                    }
                    blockNotFound.IsVisible = Items.Count == 0;
                });
            });
        }

        public void UpdateStat()
        {
            StatInfo = string.Format(Locale["LabelStat"], StringHelper.FormatNumber(Items.Count), StringHelper.FormatNumber(Total));
            CounterInfo = string.Format(Locale["LabelCounter"], StringHelper.FormatNumber(Items.Count(x => x.IsSelected)));
        }

        public void ReloadList()
        {
            blockNotFound.IsVisible = false;
            SetOverlay(true);
            Items.Clear();
            Total = 0;
            loadNoteList();
        }

        public void RefreshList()
        {
            blockNotFound.IsVisible = false;
            SetLoading(true);
            loadNoteList();
        }

        public void DeleteItems(JArray arr)
        {
            deleteNote(arr);
        }

        #region Load note list
        private async void loadNoteList()
        {
            ActionTask task = new ActionTask();
            task.Request = new RequestObject()
            {
                mid = task.Id,
                action = "note.list",
                options = new ActionTaskBaseOptions()
                {
                    data = new JObject() {
                        { "size", PageSize },
                        { "firstid", FirstId },
                        { "lastid", LastId },
                        { "sort", Essentials.Preferences.Get(Core.User.Id + ":notes_sort", "sort_date_desc") },
                        { "dir", Direction },
                        { "search", SearchText }
                    }
                }
            };
            task.Callback += (sender, e) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1);
                    try
                    {
                        ResponseObject response = e.Response;
                        if (response.Status == StatusCode.SUCCESS)
                        {
                            JArray arr = new JArray(response.ResultArray.OrderBy(obj => (int)obj["rn"]));
                            foreach (JObject obj in arr)
                            {
                                if (Items.Count(f => f.Id == Guid.Parse(obj["id_note"].ToString())) == 0)
                                {
                                    Items.Add(new NoteItemEntry(obj));
                                }
                            }
                            if (arr.Count > 0)
                            {
                                Total = Convert.ToInt32(arr[0]["total"].ToString());
                            }
                            UpdateStat();
                            blockNotFound.IsVisible = Items.Count == 0;
                        }
                        else
                        {
                            throw new ResponseException(response);
                        }
                    }
                    catch (ResponseException ex) { LogHelper.Debug(ex); }
                    catch (Exception ex) { LogHelper.Debug(ex); }
                    finally { SetOverlay(false); SetLoading(false); }
                });
            };
            await Core.Task.Add(task, true);
        }
        #endregion

        #region Delete note
        private async void deleteNote(JArray arr)
        {
            ActionTask task = new ActionTask();
            task.Request = new RequestObject()
            {
                mid = task.Id,
                action = "note.delete",
                options = new ActionTaskBaseOptions()
                {
                    data = new JObject() {
                        { "id", arr }
                    }
                }
            };
            task.Callback += (sender, e) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1);
                    try
                    {
                        ResponseObject response = e.Response;
                        if (response.Status == StatusCode.SUCCESS)
                        {
                            TMessagingCenter.Send<Page, JArray>(this, "NoteDeleteItems", arr);
                            TMessagingCenter.Send<Page, JObject>(this, "NoteServiceReload", new JObject() { });
                        }
                        else
                        {
                            throw new ResponseException(response);
                        }
                    }
                    catch (ResponseException ex) { LogHelper.Debug(ex); }
                    catch (Exception ex) { LogHelper.Debug(ex); }
                    finally { SetOverlay(false); }
                });
            };
            await Core.Task.Add(task, true);
        }
        #endregion

        #region Events
        private void ShowSortContextMenu()
        {
            if (App.ContextMenu != null && App.ContextMenu.IsVisible)
            {
                App.ContextMenu.Hide();
                return;
            }
            string selected = Essentials.Preferences.Get(Core.User.Id + ":notes_sort", "sort_date_desc");
            ObservableCollection<ContextMenuItem> list = new ObservableCollection<ContextMenuItem>
            {
                new ContextMenuItem() { Icon = "\uf887", Text = Locale["CMSortDateDesc"], CommandName = "sort_date_desc", Color = selected == "sort_date_desc" ? (Color)Application.Current.Resources["TColorBlue"] : (Color)Application.Current.Resources["TColorGrayDark"] },
                new ContextMenuItem() { Icon = "\uf162", Text = Locale["CMSortDateAsc"], CommandName = "sort_date_asc", Color = selected == "sort_date_asc" ? (Color)Application.Current.Resources["TColorBlue"] : (Color)Application.Current.Resources["TColorGrayDark"] },
                new ContextMenuItem() { Icon = "\uf882", Text = Locale["CMSortNameDesc"], CommandName = "sort_name_desc", Color = selected == "sort_name_desc" ? (Color)Application.Current.Resources["TColorBlue"] : (Color)Application.Current.Resources["TColorGrayDark"] },
                new ContextMenuItem() { Icon = "\uf15d", Text = Locale["CMSortNameAsc"], CommandName = "sort_name_asc", Color = selected == "sort_name_asc" ? (Color)Application.Current.Resources["TColorBlue"] : (Color)Application.Current.Resources["TColorGrayDark"] }
            };
            App.ContextMenu.SetOnItemClick(Context_OnSortItemClick);
            App.ContextMenu.Show(this, list, position: ContextMenuPosition.CENTER);
        }

        private void Context_OnSortItemClick(object sender, EventArgs e)
        {
            App.ContextMenu.Hide();
            ContextMenuItem item = (e as MenuItemEventArgs).Result;
            Essentials.Preferences.Set(Core.User.Id + ":notes_sort", item.CommandName);
            ReloadList();
        }

        private void ShowContextMenu()
        {
            if (App.ContextMenu != null && App.ContextMenu.IsVisible)
            {
                App.ContextMenu.Hide();
                return;
            }
            ObservableCollection<ContextMenuItem> list = new ObservableCollection<ContextMenuItem>();
            list.Add(new ContextMenuItem() { Icon = "\uf067", Text = Locale["CMCreateNote"], CommandName = "create" });
            list.Add(new ContextMenuItem() { Icon = "\uf021", Text = Locale["CMRefresh"], CommandName = "refresh" });
            list.Add(new ContextMenuItem() { Icon = "\uf885", Text = Locale["CMSort"], CommandName = "sort" });
            if (Items.Count(x => x.IsSelected) > 0)
            {
                list.Add(new ContextMenuItem() { IsDivider = true });
                list.Add(new ContextMenuItem() { Icon = "\uf1f8", Text = Locale["CMDeleteSelected"], CommandName = "delete", Color = (Color)Application.Current.Resources["TColorDanger"] });
            }
            list.Add(new ContextMenuItem() { IsDivider = true });
            list.Add(new ContextMenuItem() { Icon = "\uf013", Text = Locale["MenuSettings"] + "...", CommandName = "settings", Color = (Color)Application.Current.Resources["TColorBlue"] });
            App.ContextMenu.SetOnItemClick(Context_OnItemClick);
            App.ContextMenu.Show(this, list, position: ContextMenuPosition.TOP_RIGHT, offset: 3);
        }

        private async void Context_OnItemClick(object sender, EventArgs e)
        {
            App.ContextMenu.Hide();
            ContextMenuItem item = (e as MenuItemEventArgs).Result;
            JArray arr = new JArray();
            switch (item.CommandName)
            {
                case "refresh":
                    ReloadList();
                    break;
                case "create":
                    await ((HomePage)App.Current.MainPage).Detail.Navigation.PushAsync(new NotePropertiesPage() { NoteItem = new NoteItemEntry(), EditMode = FormState.C_EDIT_MODE_CREATE }, false);
                    break;
                case "delete":
                    if (Essentials.Preferences.Get(Core.User.Id + ":confirm_actions", true))
                    {
                        var confirmed = await DisplayAlert(null, Locale["ConfirmDeleteSelected"], Locale["ButtonYes"], Locale["ButtonNo"]);
                        if (!confirmed)
                        {
                            return;
                        }
                    }
                    Items.Where(x => x.IsSelected).ForEach((o) => { arr.Add(o.Id); });
                    DeleteItems(arr);
                    break;
                case "settings":
                    await ((HomePage)App.Current.MainPage).Detail.Navigation.PushAsync(new NotesSettingsPage() { CanGoBack = true }, false);
                    break;
                case "sort":
                    ShowSortContextMenu();
                    break;
            }
        }

        private void chkSelectedAll_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Items.ForEach((o) => { o.IsSelected = e.Value; });
        }

        private void scrBase_ScrollToBottom(object sender, ScrollEventArgs e)
        {
            if (Items.Count < Total)
            {
                RefreshList();
            }
        }

        private void scrBase_ScrollToTop(object sender, ScrollEventArgs e)
        {
        }
        #endregion
    }
}