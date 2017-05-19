﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Vision
{
    public class Rect
    {
        internal double _width;
        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        internal double _height;
        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        internal double _x;
        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        internal double _y;
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = Y;
            }
        }

        public Rectangle Rectangle
        {
            get { return GetRectangle(); }
            set { SetRectangle(value); }
        }

        public Rect(double x, double y, double width, double height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public Rect(Rect rect)
        {
            _x = rect.X;
            _y = rect.Y;
            _width = rect.Width;
            _height = rect.Height;
        }

        public Rect(Rectangle rect)
        {
            SetRectangle(rect);
        }

        public void SetRectangle(Rectangle Rect)
        {
            _x = Rect.X;
            _y = Rect.Y;
            _width = Rect.Width;
            _height = Rect.Height;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle(_x, _y, _width, _height);
        }

        public void Scale(double scaleFactor)
        {
            _x *= scaleFactor;
            _y *= scaleFactor;
            _width *= scaleFactor;
            _height *= scaleFactor;
        }
    }
}
