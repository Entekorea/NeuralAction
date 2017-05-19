﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public abstract class Core
    {
        public static string ProjectInfromation = "Vision Project - Computer Vision via AI - Since 2017";
        public static string VersionInfromation = "0.0.1 dev";
        public static Cv Cv { get { return Cv.Context; } }
        public static Core Current { get; private set; }

        public static void Init(Core core)
        {
            Current = core;

            core.Initialize();
        }

        public abstract void Initialize();

        protected void InitLogger(Logger.WriteMethodDelegate WriteMethod)
        {
            Logger.WriteMethod = WriteMethod;
        }

        protected void InitCv(Cv cv)
        {
            Cv.Init(cv);
        }

        protected void InitStorage(Storage storage)
        {
            Storage.Init(storage);
        }
    }
}
