﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using EDEngineer.Models;
using Newtonsoft.Json;

namespace EDEngineer.Utils.System
{
    public static class SettingsManager
    {
        public static void Init()
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;

                Properties.Settings.Default.Save();
            }
        }

        public static void Reset(this WindowDimensions dimensions)
        {
            Properties.Settings.Default.WindowHeight = 0;
            Properties.Settings.Default.Save();
        }

        public static WindowDimensions Dimensions
        {
            get
            {
                double height, width, left, top, leftSideWidth, rightSideWidth;
                if (Properties.Settings.Default.WindowHeight == 0 || Properties.Settings.Default.ResetUI)
                {
                    Properties.Settings.Default.WindowHeight = height = SystemParameters.PrimaryScreenHeight*0.6d;
                    Properties.Settings.Default.WindowWidth = width = SystemParameters.PrimaryScreenWidth*0.6d;
                    Properties.Settings.Default.WindowLeft = left = SystemParameters.PrimaryScreenWidth / 2d - width / 2d;
                    Properties.Settings.Default.WindowTop = top = SystemParameters.PrimaryScreenHeight / 2d - height / 2d;
                    Properties.Settings.Default.LeftSideWidth = leftSideWidth = 1;
                    Properties.Settings.Default.RightSideWidth = rightSideWidth = 1;
                    Properties.Settings.Default.ResetUI = false;

                    Properties.Settings.Default.Save();
                }
                else
                {
                    height = Properties.Settings.Default.WindowHeight;
                    width = Properties.Settings.Default.WindowWidth;
                    left = Properties.Settings.Default.WindowLeft;
                    top = Properties.Settings.Default.WindowTop;
                    leftSideWidth = Properties.Settings.Default.LeftSideWidth;
                    rightSideWidth = Properties.Settings.Default.RightSideWidth;
                }

                return new WindowDimensions() { Height = height, Left = left, Top = top, Width = width, LeftSideWidth = leftSideWidth, RightSideWidth = rightSideWidth};
            }
            set
            {
                Properties.Settings.Default.WindowHeight = value.Height;
                Properties.Settings.Default.WindowWidth = value.Width;
                Properties.Settings.Default.WindowLeft = value.Left;
                Properties.Settings.Default.WindowTop = value.Top;
                Properties.Settings.Default.LeftSideWidth = value.LeftSideWidth;
                Properties.Settings.Default.RightSideWidth = value.RightSideWidth;

                Properties.Settings.Default.Save();
            }
        }

        public static bool BlueprintReadyToastEnabled
        {
            get { return Properties.Settings.Default.BlueprintReadyToastEnabled; }
            set
            {
                Properties.Settings.Default.BlueprintReadyToastEnabled = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool SilentLaunch
        {
            get { return Properties.Settings.Default.SilentLaunch; }
            set
            {
                Properties.Settings.Default.SilentLaunch = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool CargoAlmostFullWarningEnabled
        {
            get { return Properties.Settings.Default.CargoAlmostFullWarningEnabled; }
            set
            {
                Properties.Settings.Default.CargoAlmostFullWarningEnabled = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool ThresholdWarningEnabled
        {
            get { return Properties.Settings.Default.ThresholdWarningEnabled; }
            set
            {
                Properties.Settings.Default.ThresholdWarningEnabled = value;
                Properties.Settings.Default.Save();
            }
        }
        
        public static ushort ServerPort
        {
            get { return Properties.Settings.Default.ServerPort; }
            set
            {
                Properties.Settings.Default.ServerPort = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool AutoRunServer
        {
            get { return Properties.Settings.Default.AutoRunServer; }
            set
            {
                Properties.Settings.Default.AutoRunServer = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int CargoTabIndex
        {
            get { return Properties.Settings.Default.CargoTabIndex; }
            set
            {
                Properties.Settings.Default.CargoTabIndex = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string SelectedCommander
        {
            get { return Properties.Settings.Default.SelectedCommander; }
            set
            {
                Properties.Settings.Default.SelectedCommander = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string Shortcut
        {
            get
            {
                var shortcut = Properties.Settings.Default.Shortcut;
                if (string.IsNullOrEmpty(shortcut))
                {
                    var converter = new KeysConverter();
                    shortcut = Properties.Settings.Default.Shortcut = converter.ConvertToString(Keys.F10 | Keys.Control);
                    Properties.Settings.Default.Save();
                }

                return shortcut;
            }
            set
            {
                Properties.Settings.Default.Shortcut = value;
                Properties.Settings.Default.Save();
            }
        }

        public static Dictionary<string, int?> Thresholds
        {
            get
            {
                var serialized = Properties.Settings.Default.Thresholds;
                if (serialized == null)
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<Dictionary<string, int?>>(serialized);
            }
            set
            {
                var serialized = JsonConvert.SerializeObject(value);
                Properties.Settings.Default.Thresholds = serialized;
                Properties.Settings.Default.Save();
            }
        }

        public static string Comparer
        {
            get
            {
                var comparer = Properties.Settings.Default.Comparer;
                if (string.IsNullOrEmpty(comparer))
                {
                    Comparer = comparer = State.NAME_COMPARER;
                }

                return comparer;
            }
            set
            {
                Properties.Settings.Default.Comparer = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
