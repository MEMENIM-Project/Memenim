using System;
using System.IO;
using System.Windows;
using Memenim.Core.Schema;
using RIS.Settings;
using RIS.Settings.Ini;
using Environment = RIS.Environment;

namespace Memenim.Settings
{
    public sealed class AppSettings : IniSettings
    {
        public const string SettingsFileName = "AppSettings.config";



        [SettingCategory("Localization")]
        public string Language { get; set; }
        [SettingCategory("Window")]
        public double WindowPositionX { get; set; }
        public double WindowPositionY { get; set; }
        public int WindowState { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        [SettingCategory("Feed")]
        public int PostsType { get; set; }
        [SettingCategory("Comments")]
        public int CommentReplyMode { get; set; }
        [SettingCategory("SpecialEvents")]
        public bool ChristmasEventEnabled { get; set; }
        public double BgmVolume { get; set; }
        [SettingCategory("Log")]
        public int LogRetentionDaysPeriod { get; set; }



        [ExcludedSetting]
        public WindowState WindowStateEnum
        {
            get
            {
                if (Enum.GetName(typeof(WindowState), WindowState) == null)
                    WindowState = (int)System.Windows.WindowState.Normal;

                return (WindowState)WindowState;
            }
            set
            {
                WindowState = (int)WindowStateEnum;
            }
        }
        [ExcludedSetting]
        public PostType PostsTypeEnum
        {
            get
            {
                if (Enum.GetName(typeof(PostType), PostsType) == null)
                    PostsType = (int)PostType.Popular;

                return (PostType)PostsType;
            }
            set
            {
                PostsType = (int)PostsTypeEnum;
            }
        }
        [ExcludedSetting]
        public CommentReplyModeType CommentReplyModeEnum
        {
            get
            {
                if (Enum.GetName(typeof(CommentReplyModeType), CommentReplyMode) == null)
                    CommentReplyMode = (int)CommentReplyModeType.Legacy;

                return (CommentReplyModeType)CommentReplyMode;
            }
            set
            {
                CommentReplyMode = (int)CommentReplyModeEnum;
            }
        }



        public AppSettings()
            : base(Path.Combine(Environment.ExecProcessDirectoryName, SettingsFileName))
        {
            Language = "en-US";
            WindowPositionX = SystemParameters.PrimaryScreenWidth / 2.0;
            WindowPositionY = SystemParameters.PrimaryScreenHeight / 2.0;
            WindowState = (int)System.Windows.WindowState.Normal;
            WindowWidth = 900;
            WindowHeight = 550;
            PostsType = (int)PostType.Popular;
            CommentReplyMode = (int)CommentReplyModeType.Legacy;
            ChristmasEventEnabled = true;
            BgmVolume = 0.5;
            LogRetentionDaysPeriod = 7;

            Load();
        }
    }
}
