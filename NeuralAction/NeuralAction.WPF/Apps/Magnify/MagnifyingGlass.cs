﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Timers;
using NeuralAction.WPF.Magnify;
using Vision;

namespace NeuralAction.WPF
{
    public class MagnifyingGlass : NotifyPropertyChnagedBase, IDisposable
    {
        public static MagnifyingGlass Current = new MagnifyingGlass();

        public double ZoomSmooth { get; set; } = 1.25;
        public double MoveMinimum { get; set; } = 3;
        public double MoveMaximum { get; set; } = 80;
        public double MoveSmooth { get; set; } = 5;
        public bool UseDynamicZoom { get; set; } = true;

        public bool IsShowed { get; set; } = false;

        double wpfScale = InputService.Current.Cursor.Window.WpfScale;
        MagnifierForm Mag;
        MagnifyingCursor Window;
        Thread moveUpdater;

        public MagnifyingGlass()
        {

        }

        public void Show()
        {
            if(Mag == null)
            {
                Mag = new MagnifierForm();
                Window = new MagnifyingCursor(this);

                Settings.Listener.PropertyChanged += Listener_PropertyChanged;
                Settings.Listener.SettingChanged += Listener_SettingChanged;
                Listener_SettingChanged(null, Settings.Current);
            }

            var scr = InputService.Current.TargetScreen.Bounds;
            Mag.Form.Width = scr.Width;
            Mag.Form.Height = scr.Height;
            Mag.Form.Top = scr.Top;
            Mag.Form.Left = scr.Left;
            Mag.Show();

            Window.Left = InputService.Current.Cursor.Window.Left;
            Window.Top = InputService.Current.Cursor.Window.Top;
            Window.Show();

            Mag.Magnifier.UpdateMaginifier();
            if (moveUpdater == null)
            {
                moveUpdater = new Thread(() =>
                {
                    while (true)
                    {
                        MoveUpdater_Tick(null, null);
                        Thread.Sleep(10);
                    }
                });
                moveUpdater.IsBackground = true;
                moveUpdater.Name = "MagUpdate";
            }
            moveUpdater.Start();

            var cursor = InputService.Current.Cursor;
            cursor.Window.Visibility = Visibility.Collapsed;
            cursor.GazeTracked += Cursor_GazeTracked;
            cursor.Clicked += Cursor_Clicked;
            cursor.Released += Cursor_Released;

            InputService.Current.KeyboardStartupOption = KeyboardStartupOption.CenterCursor;
            InputService.Current.KeyboardSize = 1.0 / Settings.Current.MagnifyFactor * 1.25;

            GlobalKeyHook.Hook.KeyboardPressed += Hook_KeyboardPressed;

            IsShowed = true;
        }

        public void Close()
        {
            if (Mag == null)
                return;

            Mag.Hide();
            Window.Hide();

            moveUpdater.Abort();
            moveUpdater = null;

            var cursor = InputService.Current.Cursor;
            cursor.Window.Visibility = Visibility.Visible;
            cursor.GazeTracked -= Cursor_GazeTracked;
            cursor.Clicked -= Cursor_Clicked;
            cursor.Released -= Cursor_Released;

            InputService.Current.KeyboardStartupOption = KeyboardStartupOption.FullScreen;

            GlobalKeyHook.Hook.KeyboardPressed -= Hook_KeyboardPressed;

            IsShowed = false;
        }

        void Cursor_Released(object sender, CursorReleasedArgs e)
        {
            Window.Dispatcher.Invoke(() =>
            {
                if (e.Duration > Settings.Current.CursorOpenMenuWaitDuration)
                {
                    Window.Click(false);
                    ShortcutMenuWindow.OpenPopup(new System.Windows.Point(Window.ActualLeft, Window.ActualTop));
                }
                else
                {
                    Window.Click();
                }
            });
        }

        void Cursor_Clicked(object sender, Vision.Point e)
        {
            Window.Dispatcher.Invoke(() =>
            {
                Window.Click(false);
            });
        }

        void Cursor_GazeTracked(object sender, GazeEventArgs e)
        {
            gazeArg = e;
            Window.Available = e.IsAvailable;
        }

        void Hook_KeyboardPressed(object sender, GlobalKeyHookEventArgs e)
        {
            if (e.VKeyCode == VKeyCode.F4)
            {
                if (GlobalKeyHook.IsKeyPressed(VKeyCode.LeftControl) && GlobalKeyHook.IsKeyPressed(VKeyCode.LeftAlt))
                {
                    Close();
                }
            }
        }

        PointSmoother pointSmoother = new PointSmoother()
        {
            Method = PointSmoother.SmoothMethod.MeanKalman,
            QueueCount = 10,
        };
        KalmanFilter kalman = new KalmanFilter();
        MeanSmoother mean = new MeanSmoother()
        {
            QueueCount = 10,
        };
        GazeEventArgs gazeArg;
        long lastMs;
        void MoveUpdater_Tick(object sender, EventArgs e)
        {
            if (gazeArg != null && gazeArg.IsAvailable)
            {
                var scr = gazeArg.ScreenProperties.PixelSize;
                var wndPos = Window.GetActualPosition();
                var x = (wndPos.X - Mag.Magnifier.Left) / Mag.Magnifier.Width;
                var y = (wndPos.Y - Mag.Magnifier.Top) / Mag.Magnifier.Height;
                var trackedX = gazeArg.Position.X / scr.Width;
                var trackedY = gazeArg.Position.Y / scr.Height;

                var xPow = trackedX - x;
                var yPow = trackedY - y;
                var power = 1 - Drop(Math.Pow(Math.Sqrt(xPow * xPow + yPow * yPow), 2) / ZoomSmooth, 0.1);
                if (!UseDynamicZoom)
                    power = 1;
                power = mean.Smooth(power);
                power = kalman.Calculate(power);
                if (double.IsNaN(power))
                    throw new ArgumentException();
                xPow = Clamp(Drop(xPow * Mag.Magnifier.Width / MoveSmooth, MoveMinimum), MoveMaximum);
                yPow = Clamp(Drop(yPow * Mag.Magnifier.Height / MoveSmooth, MoveMinimum), MoveMaximum);

                var smt = pointSmoother.Smooth(new Vision.Point(wndPos.X + xPow, wndPos.Y + yPow));

                if (Settings.Current.AllowControl && Logger.Stopwatch.ElapsedMilliseconds - lastMs > 33)
                {
                    MouseEvent.MoveAt(new System.Windows.Point(smt.X, smt.Y));
                    lastMs = Logger.Stopwatch.ElapsedMilliseconds;
                }

                Window.SetActualPosition(smt.X, smt.Y);
                Mag.Magnifier.CenterX = (int)(smt.X);
                Mag.Magnifier.CenterY = (int)(smt.Y);
                Mag.Magnifier.Magnification = Math.Max(1, Mag.Magnification * power);
            }
            Mag.Magnifier.UpdateMaginifier();
        }

        double Drop(double value, double abs)
        {
            if (value > 0)
                value = Math.Max(0, value - abs);
            else if (value < 0)
                value = Math.Min(0, value + abs);
            return value;
        }

        double Clamp(double value, double abs)
        {
            return Math.Max(-abs, Math.Min(abs, value));
        }

        void Listener_SettingChanged(object sender, Settings set)
        {
            Mag.Magnification = set.MagnifyFactor;
            MoveSmooth = set.MagnifyMoveSmooth;
            MoveMaximum = set.MagnifySpeedMax;
            MoveMinimum = set.MagnifySpeedMin;
            ZoomSmooth = set.MagnifyZoomSmooth;
            UseDynamicZoom = set.MagnifyUseDynZoom;
        }

        void Listener_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var set = Settings.Current;

            switch (e.PropertyName)
            {
                case nameof(Settings.MagnifyFactor):
                    Mag.Magnification = set.MagnifyFactor;
                    break;
                case nameof(Settings.MagnifyMoveSmooth):
                    MoveSmooth = set.MagnifyMoveSmooth;
                    break;
                case nameof(Settings.MagnifySpeedMax):
                    MoveMaximum = set.MagnifySpeedMax;
                    break;
                case nameof(Settings.MagnifySpeedMin):
                    MoveMinimum = set.MagnifySpeedMin;
                    break;
                case nameof(Settings.MagnifyZoomSmooth):
                    ZoomSmooth = set.MagnifyZoomSmooth;
                    break;
                case nameof(Settings.MagnifyUseDynZoom):
                    UseDynamicZoom = set.MagnifyUseDynZoom;
                    break;
            }
        }

        public void Dispose()
        {
            if (IsShowed)
                Close();

            Mag?.Dispose();
            Mag = null;

            Window?.Close();
            Window = null;

            Settings.Listener.PropertyChanged -= Listener_PropertyChanged;
            Settings.Listener.SettingChanged -= Listener_SettingChanged;
        }
    }
}
