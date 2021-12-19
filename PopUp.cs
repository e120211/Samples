using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using MyApp.Localization;

namespace MyApp
{
    public enum PopUpPosition
    {
        TOP = 0,
        CENTER = 1,
        BOTTOM = 2
    }

    public enum PopUpDuration
    {
        INDEFINITE = -1,
        SHORT = 1,
        LONG = 1
    }

    public class PopUp
    {
        public static LocalizedResources Locale { get { return App.LocResources; } }

        public static void Info(string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await (App.Current.MainPage as Page)?.DisplayAlert(Locale["TitleInfo"], message, Locale["ButtonOk"]);
            });
        }

        public static void Message(string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await (App.Current.MainPage as Page)?.DisplayAlert(Locale["TitleMessage"], message, Locale["ButtonOk"]);
            });
        }

        public static void Warning(string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await (App.Current.MainPage as Page)?.DisplayAlert(Locale["TitleWarning"], message, Locale["ButtonClose"]);
            });
        }

        public static void Error(string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await (App.Current.MainPage as Page)?.DisplayAlert(Locale["TitleError"], message, Locale["ButtonClose"]);
            });
        }

        public async static void DisableActionButton(object button)
        {
            await Task.Run(() => {
                DependencyService.Get<IPopUp>().DisableActionButton(button);
            });
        }

        public async static void Toast(string message, PopUpPosition position = PopUpPosition.BOTTOM, PopUpDuration duration = PopUpDuration.SHORT, bool error = false)
        {
            await Task.Run(() => {
                DependencyService.Get<IPopUp>().ShowToast(message, position, duration, error);
            });
        }

        public async static void SnackBar(string message, PopUpPosition position, int duration, string actionText, Action<object> action)
        {
            await Task.Run(() => {
                DependencyService.Get<IPopUp>().ShowSnackBar(message, position, duration, actionText, action);
            });
        }

        public async static void HideToast()
        {
            await Task.Run(() => {
                DependencyService.Get<IPopUp>().HideToast();
            });
        }

        public async static void HideSnackBar()
        {
            await Task.Run(() => {
                DependencyService.Get<IPopUp>().HideSnackBar();
            });
        }
    }
}