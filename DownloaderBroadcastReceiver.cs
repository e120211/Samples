using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Util;
using Android.Graphics;
using Xamarin.Essentials;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Android;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Net;
using Android.Database;
using System.IO;

namespace MyApp.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { DownloadManager.ActionDownloadComplete })]
    public class DownloaderBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            long id = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
            openDownloadedAttachment(context, id);
        }

        private void openDownloadedAttachment(Context context, long downloadId)
        {
            DownloadManager downloadManager = DownloadManager.FromContext(context);
            DownloadManager.Query query = new DownloadManager.Query();
            query.SetFilterById(downloadId);
            ICursor cursor = downloadManager.InvokeQuery(query);
            if (cursor.MoveToFirst())
            {
                int downloadStatus = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus));
                string downloadLocalUri = cursor.GetString(cursor.GetColumnIndex(DownloadManager.ColumnLocalUri));
                string downloadMimeType = cursor.GetString(cursor.GetColumnIndex(DownloadManager.ColumnMediaType));
                if (downloadLocalUri != null)
                {
                    openDownloadedAttachment(context, Android.Net.Uri.Parse(downloadLocalUri), downloadMimeType);
                }
            }
            cursor.Close();
        }

        private async void openDownloadedAttachment(Context context, Android.Net.Uri attachmentUri, string attachmentMimeType)
        {
            if (attachmentUri != null)
            {
                if (ContentResolver.SchemeFile.Equals(attachmentUri.Scheme))
                {
                    Java.IO.File file = new Java.IO.File(attachmentUri.Path);
                    attachmentUri = FileProvider.GetUriForFile(context, "com.pskkama.TenderApp.fileprovider", file); ;
                }
                try
                {
                    string path = attachmentUri.Path.Replace("/external_files", "/storage/emulated/0");
                    await Xamarin.Essentials.Launcher.OpenAsync(new OpenFileRequest("Открытие файла", new ReadOnlyFile(path, attachmentMimeType)));
                }
                catch (ActivityNotFoundException ex)
                {
                    Toast.MakeText(context, "Ошибка: " + ex.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}