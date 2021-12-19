using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Graphics;
using Android.Graphics.Drawables;

using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using MyApp.Controls;
using MyApp.Controls.Droid.Renderers;
using MyApp.Controls.Droid.Helpers;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(ExtendedStackLayout), typeof(ExtendedStackLayoutRenderer))]
namespace MyApp.Controls.Droid.Renderers
{
    public class ExtendedStackLayoutRenderer : ViewRenderer
    {
        [Obsolete("This constructor is obsolete as of version 2.5.")]
        public ExtendedStackLayoutRenderer()
        {
        }

        public ExtendedStackLayoutRenderer(Context context) : base(context)
        {
        }

        private ExtendedStackLayoutListener _listener;
        private GestureDetector _detector;

        public ExtendedStackLayoutListener Listener
        {
            get { return _listener; }
        }

        public GestureDetector Detector
        {
            get { return _detector; }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);
            var element = (ExtendedStackLayout)Element;
            if (e.OldElement == null)
            {
                GenericMotion += HandleGenericMotion;
                Touch += HandleTouch;
                _listener = new ExtendedStackLayoutListener(element);
                _detector = new GestureDetector(this.Context, _listener);
            }
        }

        protected override void Dispose(bool disposing)
        {
            GenericMotion -= HandleGenericMotion;
            Touch -= HandleTouch;
            _listener = null;
            _detector?.Dispose();
            _detector = null;
            base.Dispose(disposing);
        }

        void HandleTouch(object sender, TouchEventArgs e)
        {
            _detector.OnTouchEvent(e.Event);
        }

        void HandleGenericMotion(object sender, GenericMotionEventArgs e)
        {
            _detector.OnTouchEvent(e.Event);
        }
    }

    public class ExtendedStackLayoutListener : GestureDetector.SimpleOnGestureListener
    {
        readonly ExtendedStackLayout _target;

        public ExtendedStackLayoutListener(ExtendedStackLayout s)
        {
            _target = s;
        }

        public override bool OnSingleTapConfirmed(MotionEvent e)
        {
            _target.RaisePressedHandler();
            return base.OnSingleTapConfirmed(e);
        }

        public override void OnLongPress(MotionEvent e)
        {
            _target.RaiseLongPressedHandler();
            base.OnLongPress(e);
        }
    }
}