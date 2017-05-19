﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public abstract class Cv
    {
        private static Cv _context;
        public static Cv Context
        {
            get
            {
                return _context;
            }
        }

        public virtual string BuildInformation { get { return GetBuildInformation(); } }

        internal static void Init(Cv Context)
        {
            if (_context == null)
            {
                _context = Context;

                Logger.Log("Context is created. New Context: [" + Context.ToString() + "]");
            }
            else
            {
                Logger.Error("Context is already created");
            }
        }

        internal protected abstract VMat CreateMat();
        internal protected abstract VMat CreateMat(VMat mat, Rect rect);

        internal protected abstract CascadeClassifier CreateCascadeClassifier(string filePath);

        internal protected abstract Capture CreateCapture(int index);
        internal protected abstract Capture CreateCapture(string filePath);

        protected abstract string GetBuildInformation();

        public abstract void DrawCircle(VMat img, Point center, double radius, Scalar color, double thickness = 1, LineType lineType = LineType.Link8, int shift = 0);
        public abstract void DrawEllipse(VMat img, Point center, Size axes, double angle, double startAngle, double endAngle, Scalar color, double thickness = 1, LineType lineType = LineType.Link8, int shift = 0);
        public abstract void DrawText(VMat img, string text, Point org, FontFace fontFace, double fontScale, Scalar color, int thickness = 1, LineType lineType = LineType.Link8, bool bottomLeftOrigin = false);
        public abstract void ConvertColor(VMat src, VMat output, ColorConversion convertMode);
        public abstract void EqualizeHistogram(VMat input, VMat output);
        public abstract void Canny(VMat Input, VMat output, double thresold1, double thresold2, int apertureSize = 3, bool L2gradient = false);
        public abstract void Transpose(VMat input, VMat output);
        public abstract void Resize(VMat input, VMat dist, Size size, double fx = 0, double fy = 0, Interpolation inter = Interpolation.Linear);

        public abstract void ImgShow(string name, VMat img);
        public VMat ImgRead(FileNode node)
        {
            return ImgRead(node.AbosolutePath);
        }
        public VMat ImgRead(string path)
        {
            Logger.Log(this, "Image Read : " + path);

            return InternalImgRead(path);
        }

        public void ImgWrite(FileNode node, VMat img)
        {
            ImgWrite(node.AbosolutePath, img);
        }
        public void ImgWrite(string path, VMat img)
        {
            Logger.Log(this, "Image Write : " + path);

            InternalImgWrite(path, img);
        }
        protected abstract VMat InternalImgRead(string path);
        protected abstract void InternalImgWrite(string name, VMat img);

        public abstract void CloseWindow(string name);
        public abstract void CloseAllWindows();
        public abstract char WaitKey(int duration);
    }
}
