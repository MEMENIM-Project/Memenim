using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Memenim.Generating;
using Memenim.Settings;
using RIS.Extensions;
using RIS.Logging;
using RIS.Randomizing;
using Environment = RIS.Environment;

namespace Memenim.SpecialEvents.Layers
{
    public sealed partial class ChristmasEventLayer : SpecialEventLayerContent
    {
        private const double FadeAnimationSeconds = 3;

        private string _padoruSongPath;
        private string[] _songsPaths;
        private string _currentSongPath;
        private DoubleAnimation _moveRightAnimation;
        private DoubleAnimation _fadeInAnimation;
        private DoubleAnimation _fadeOutAnimation;
        private Timer _padoruTimer;

        public override string DefaultMusicDirectoryPath { get; }
        public override string CustomMusicDirectoryPath { get; }
        public override bool EventEnabled
        {
            get
            {
                return SettingsManager.AppSettings.ChristmasEventEnabled;
            }
            protected set
            {
                SettingsManager.AppSettings.ChristmasEventEnabled = value;
                SettingsManager.AppSettings.Save();
            }
        }
        public override bool EventLoaded { get; protected set; }



        public ChristmasEventLayer()
        {
            InitializeComponent();
            DataContext = this;

            var baseAppDirectory = Environment.ExecAppDirectoryName;
            var baseProcessDirectory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(baseAppDirectory) || baseAppDirectory == "Unknown")
                baseAppDirectory = string.Empty;
            if (string.IsNullOrEmpty(baseProcessDirectory) || baseProcessDirectory == "Unknown")
                baseProcessDirectory = string.Empty;

            DefaultMusicDirectoryPath = Path.Combine(
                baseAppDirectory, "Resources", "Music", "ChristmasEvent");
            CustomMusicDirectoryPath = Path.Combine(
                baseProcessDirectory, "special_events", "ChristmasEvent", "Music");

            if (!Directory.Exists(DefaultMusicDirectoryPath))
                Directory.CreateDirectory(DefaultMusicDirectoryPath);
            if (!Directory.Exists(CustomMusicDirectoryPath))
                Directory.CreateDirectory(CustomMusicDirectoryPath);
        }



        private List<string> GetSongsPaths(
            string directoryBasePath)
        {
            var songsPaths = new List<string>(10);
            var directory = directoryBasePath;

            if (string.IsNullOrEmpty(directory)
                || directory == "Unknown"
                || !Directory.Exists(directory))
            {
                return songsPaths;
            }

            if (!Directory.Exists(directory))
                return songsPaths;

            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                var extension = Path.GetExtension(filePath);

                if (extension == null || extension != ".mp3")
                    continue;
                if (filePath == _padoruSongPath)
                    continue;

                songsPaths.Add(filePath);
            }

            return songsPaths;
        }

        private string[] GetSongsPaths()
        {
            var songsPaths = new List<string>(10);

            foreach (var songPath in GetSongsPaths(
                DefaultMusicDirectoryPath))
            {
                songsPaths.Add(songPath);
            }

            foreach (var songPath in GetSongsPaths(
                CustomMusicDirectoryPath))
            {
                songsPaths.Add(songPath);
            }

            return songsPaths.ToArray();
        }

        private string GetDefaultSongPath(
            string name)
        {
            return Path.Combine(
                DefaultMusicDirectoryPath, name);
        }

        private string GetRandomSong()
        {
            var songIndex = (int)GeneratingManager.CachedRandomGenerator
                .GetNormalizedIndex((uint)_songsPaths.Length);
            var songPath = _songsPaths[songIndex];

            if (songPath != _currentSongPath)
            {
                _currentSongPath = songPath;
                return songPath;
            }

            if (songIndex == 0)
            {
                ++songIndex;
            }
            else if (songIndex == _songsPaths.Length - 1)
            {
                --songIndex;
            }
            else
            {
                if (Rand.Current.NextBoolean(0.5))
                    ++songIndex;
                else
                    --songIndex;
            }

            songPath = _songsPaths[songIndex];

            _currentSongPath = songPath;
            return songPath;
        }

        private void SetRandomNextPadoruInterval()
        {
            const int biasZone =
                int.MaxValue - (int.MaxValue % 420) - 1;
            int randomSeconds =
                (int)GeneratingManager.CachedRandomGenerator
                    .GetUInt32((uint)biasZone) % 420;
            int seconds = 180 + randomSeconds;

            LogManager.Default.Info($"Next padoru in {seconds} seconds");

            _padoruTimer.Interval = seconds * 1000;
        }

        private void PlayRandomSong()
        {
            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeOutAnimation);
            MusicPlayer.Stop();

            MusicPlayer.Source = new Uri(GetRandomSong());

            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeInAnimation);
            MusicPlayer.Play();
        }

        private void PlayPadoru()
        {
            if (!PadoruPlayer.IsEnabled)
                return;

            _padoruTimer.Stop();

            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeOutAnimation);
            MusicPlayer.Pause();

            PadoruPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeInAnimation);
            PadoruPlayer.Play();

            PadoruImage.Visibility = Visibility.Visible;

            PadoruImage.BeginAnimation(Canvas.LeftProperty, _moveRightAnimation);
        }

        private void StopPadoru()
        {
            if (!PadoruPlayer.IsEnabled)
                return;

            PadoruPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeOutAnimation);
            PadoruPlayer.Stop();

            PadoruImage.Visibility = Visibility.Collapsed;

            SetRandomNextPadoruInterval();

            _padoruTimer.Start();

            if (!MusicPlayer.IsEnabled)
                return;

            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeInAnimation);
            MusicPlayer.Play();
        }



        internal override bool EventTimeSatisfied(
            DateTime currentTime)
        {
            var eventStartTime = new DateTime(
                    day: 20, month: 12, year: 1,
                    hour: 0, minute: 0, second: 0)
                .AddYears(currentTime.Year - 1);
            var eventEndTime = new DateTime(
                    day: 10, month: 1, year: 1,
                    hour: 23, minute: 59, second: 59)
                .AddYears(currentTime.Year - 1);

            if (!(eventStartTime <= currentTime
                  || currentTime <= eventEndTime))
            {
                return false;
            }

            return true;
        }

        internal override bool LoadEvent()
        {
            if (EventLoaded)
                return true;

            _padoruSongPath = GetDefaultSongPath("padoru.mp3");
            _songsPaths = GetSongsPaths();
            _currentSongPath = string.Empty;

            _moveRightAnimation = new DoubleAnimation
            {
                From = -550,
                To = ActualWidth + 250,
                Duration = new Duration(TimeSpan.FromSeconds(13))
            };
            _fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = SettingsManager.AppSettings.BgmVolume,
                Duration = TimeSpan.FromSeconds(FadeAnimationSeconds),
                FillBehavior = FillBehavior.Stop
            };
            _fadeOutAnimation = new DoubleAnimation
            {
                From = SettingsManager.AppSettings.BgmVolume,
                To = 0,
                Duration = TimeSpan.FromSeconds(FadeAnimationSeconds),
                FillBehavior = FillBehavior.Stop
            };

            _padoruTimer = new Timer();
            _padoruTimer.Elapsed += PadoruTimer_Tick;

            SetRandomNextPadoruInterval();

            MusicPlayer.Source = new Uri(GetRandomSong());
            PadoruPlayer.Source = new Uri(_padoruSongPath);

            _padoruTimer.Start();

            Activate(EventEnabled);
            SetVolume(SettingsManager.AppSettings.BgmVolume);

            EventLoaded = true;

            return true;
        }

        internal override void UnloadEvent()
        {
            if (!EventLoaded)
                return;

            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeOutAnimation);
            MusicPlayer.Pause();

            _padoruTimer.Stop();

            PadoruImage.Visibility = Visibility.Collapsed;

            PadoruPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeOutAnimation);
            PadoruPlayer.Stop();

            MusicPlayer.IsEnabled = false;
            PadoruPlayer.IsEnabled = false;

            EventLoaded = false;
        }

        public override void Activate(
            bool state)
        {
            if (state)
            {
                MusicPlayer.IsEnabled = true;
                PadoruPlayer.IsEnabled = true;

                MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeInAnimation);
                MusicPlayer.Play();

                _padoruTimer.Start();

                LogManager.Default.Info("Christmas event - On");
            }
            else
            {
                MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeOutAnimation);
                MusicPlayer.Pause();

                _padoruTimer.Stop();

                PadoruImage.Visibility = Visibility.Collapsed;

                PadoruPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeOutAnimation);
                PadoruPlayer.Stop();

                MusicPlayer.IsEnabled = false;
                PadoruPlayer.IsEnabled = false;

                LogManager.Default.Info("Christmas event - Off");
            }

            EventEnabled = state;
        }

        public override void SetVolume(
            double value)
        {
            if (value < 0.0)
                value = 0.0;
            if (value > 1.0)
                value = 1.0;

            _fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = value,
                Duration = TimeSpan.FromSeconds(FadeAnimationSeconds),
                FillBehavior = FillBehavior.Stop
            };
            _fadeOutAnimation = new DoubleAnimation
            {
                From = value,
                To = 0,
                Duration = TimeSpan.FromSeconds(FadeAnimationSeconds),
                FillBehavior = FillBehavior.Stop
            };

            MusicPlayer.Volume = value;
            PadoruPlayer.Volume = value;

            SettingsManager.AppSettings.BgmVolume = value;
            SettingsManager.AppSettings.Save();
        }



        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            if (EventEnabled)
                PlayRandomSong();
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }



        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _moveRightAnimation = new DoubleAnimation
            {
                From = -550,
                To = e.NewSize.Width + 250,
                Duration = new Duration(TimeSpan.FromSeconds(13))
            };
        }

        private void PadoruTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayPadoru();
            });
        }

        private void MusicPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayRandomSong();
        }

        private void PadoruPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
           StopPadoru();
        }
    }
}
