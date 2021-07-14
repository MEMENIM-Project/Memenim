using System;
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

namespace Memenim.Misc
{
    public partial class SpecialEventLayer : UserControl
    {
        private static readonly object InstanceSyncRoot = new object();
        private static volatile SpecialEventLayer _instance;
        public static SpecialEventLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceSyncRoot)
                    {
                        if (_instance == null)
                            _instance = new SpecialEventLayer();
                    }
                }

                return _instance;
            }
        }

        private const double FadeAnimationSeconds = 3;

        private readonly string[] _songs;
        private string _currentSong;
        private DoubleAnimation _moveRightAnimation;
        private DoubleAnimation _fadeInAnimation;
        private DoubleAnimation _fadeOutAnimation;
        private readonly Timer _padoruTimer;

        public SpecialEventLayer()
        {
            InitializeComponent();
            DataContext = this;

            _songs = new[]
            {
                GetSongPath("christmasinnightopia.mp3"),
                GetSongPath("song2.mp3"),
                GetSongPath("song1.mp3"),
                GetSongPath("song3.mp3"),
                GetSongPath("song4.mp3"),
                GetSongPath("song5.mp3"),
                GetSongPath("song6.mp3"),
                GetSongPath("Gigawing_Xmas.mp3")
            };
            _currentSong = string.Empty;

            _moveRightAnimation = new DoubleAnimation
            {
                From = -250,
                To = ActualWidth + 250,
                Duration = new Duration(TimeSpan.FromSeconds(10))
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
            PadoruPlayer.Source = new Uri(GetSongPath("Padoru.mp3"));

            _padoruTimer.Start();

            Activate(SettingsManager.AppSettings.SpecialEventEnabled);
            SetVolume(SettingsManager.AppSettings.BgmVolume);
        }

        private string GetSongPath(string name)
        {
            return Path.Combine(Environment.ExecAppDirectoryName,
                "Resources", "Music", "SpecialEvent", name);
        }

        private string GetRandomSong()
        {
            var biasZone =
                int.MaxValue - (int.MaxValue % _songs.Length) - 1;
            int songIndex =
                (int)GeneratingManager.CachedRandomGenerator
                    .GetUInt32((uint)biasZone) % _songs.Length;
            string song = _songs[songIndex];

            if (song != _currentSong)
            {
                _currentSong = song;
                return song;
            }

            if (songIndex == 0)
            {
                ++songIndex;
            }
            else if (songIndex == _songs.Length - 1)
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

            song = _songs[songIndex];

            _currentSong = song;
            return song;
        }

        private void SetRandomNextPadoruInterval()
        {
            const int biasZone =
                int.MaxValue - (int.MaxValue % 420) - 1;
            int randomSeconds =
                (int)GeneratingManager.CachedRandomGenerator
                    .GetUInt32((uint)biasZone) % 420;
            int seconds = 180 + randomSeconds;

            LogManager.Log.Info($"Next padoru in {seconds} seconds");

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

            if (!MusicPlayer.IsEnabled)
                return;

            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeInAnimation);
            MusicPlayer.Play();
        }

        public void Activate(bool state)
        {
            if (state)
            {
                MusicPlayer.IsEnabled = true;
                PadoruPlayer.IsEnabled = true;

                MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, _fadeInAnimation);
                MusicPlayer.Play();

                _padoruTimer.Start();

                LogManager.Log.Info("Special event - On");
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

                LogManager.Log.Info("Special event - Off");
            }

            SettingsManager.AppSettings.SpecialEventEnabled = state;
            SettingsManager.AppSettings.Save();
        }

        public void SetVolume(double value)
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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsManager.AppSettings.SpecialEventEnabled)
                PlayRandomSong();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _moveRightAnimation = new DoubleAnimation
            {
                From = -250,
                To = e.NewSize.Width + 250,
                Duration = new Duration(TimeSpan.FromSeconds(10))
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
