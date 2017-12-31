﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//http://codegenerater.blogspot.kr/2013/11/c-api.html

namespace NeuralAction.WPF
{
    [Flags()]
    public enum MouseEventFlag : int
    {
        Absolute = 0x8000,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        Move = 0x0001,
        RightDown = 0x0008,
        RightUp = 0x0010,
        Wheel = 0x0800,
        XDown = 0x0080,
        XUp = 0x0100,
        HWheel = 0x1000,
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        X,
    }

    public class MouseEvent
    {
        const int ABSOLUTE_SIZE = 65535;
        public static Size ActualDisplaySize { set; get; }

        static MouseEvent()
        {
            var bound = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            ActualDisplaySize = new Size(bound.Width, bound.Height);
        }

        public static void Move(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move;

            Event((int)Flag, (int)Point.X, (int)Point.Y, 0, IntPtr.Zero);
        }

        public static void MoveAt(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move | MouseEventFlag.Absolute;

            int X = (int)((ABSOLUTE_SIZE / ActualDisplaySize.Width) * Point.X);
            int Y = (int)((ABSOLUTE_SIZE / ActualDisplaySize.Height) * Point.Y);

            Event((int)Flag, X, Y, 0, IntPtr.Zero);
        }

        public static void MoveAbsolute(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move | MouseEventFlag.Absolute;

            Event((int)Flag, (int)Point.X, (int)Point.Y, 0, IntPtr.Zero);
        }

        public static void Click(MouseButton button)
        {
            Down(button);
            Up(button);
        }

        public static void Down(MouseButton Button)
        {
            MouseEventFlag Flag;
            switch (Button)
            {
                case MouseButton.Left:
                    Flag = MouseEventFlag.LeftDown;
                    break;
                case MouseButton.Right:
                    Flag = MouseEventFlag.RightDown;
                    break;
                case MouseButton.Middle:
                    Flag = MouseEventFlag.MiddleDown;
                    break;
                case MouseButton.X:
                    Flag = MouseEventFlag.XDown;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Event((int)Flag, 0, 0, 0, IntPtr.Zero);
        }

        public static void Up(MouseButton Button)
        {
            MouseEventFlag Flag;
            switch (Button)
            {
                case MouseButton.Left:
                    Flag = MouseEventFlag.LeftUp;
                    break;
                case MouseButton.Right:
                    Flag = MouseEventFlag.RightUp;
                    break;
                case MouseButton.Middle:
                    Flag = MouseEventFlag.MiddleUp;
                    break;
                case MouseButton.X:
                    Flag = MouseEventFlag.XUp;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Event((int)Flag, 0, 0, 0, IntPtr.Zero);
        }

        static void Event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo)
        {
            WinApi.MouseEvent(dwFlags, dx, dy, dwData, dwExtraInfo);
        }
    }
}
