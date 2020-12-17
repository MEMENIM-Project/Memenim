using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Memenim.Logs;
using Memenim.Settings;

namespace Memenim.Misc
{
    /// <summary>
    /// Interaction logic for ChristmasLayer.xaml
    /// </summary>
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

        private string[] _songs =
        {
            @"\Resources\Music\christmasinnightopia.mp3",
            @"\Resources\Music\song2.mp3",
            @"\Resources\Music\song1.mp3",
            @"\Resources\Music\song3.mp3",
            @"\Resources\Music\song4.mp3",
            @"\Resources\Music\Gigawing_Xmas.mp3",
        };

        private string _CurrentSong = "";

        private Timer _PadoruTimer = new Timer();


        DoubleAnimation leftAnim;
        DoubleAnimation fadeIn;
        DoubleAnimation fadeOut;

        private string _ExecutablePath = Directory.GetCurrentDirectory();

        const int FADE_SECONDS = 3;

        public SpecialEventLayer()
        {
            InitializeComponent();
            DataContext = this;
            PadoruPlayer.Source = new Uri(_ExecutablePath + @"\Resources\Padoru.mp3");
            _PadoruTimer.Elapsed += _PadoruTimer_Tick;
            Random rnd = new Random(DateTime.Now.Millisecond);
            int seconds = 180 + rnd.Next(180, 600);
            LogManager.Log.Info(string.Format("Next padoru in {0} seconds", seconds));
            _PadoruTimer.Interval = seconds * 1000;
            
            _PadoruTimer.Start();

            leftAnim = new DoubleAnimation();
            leftAnim.From = 0;
            leftAnim.To = 1920;
            leftAnim.Duration = new Duration(TimeSpan.FromSeconds(10));

            fadeIn = new DoubleAnimation()
            {
                From = 0,
                To = SettingsManager.AppSettings.BgmVolume,
                Duration = TimeSpan.FromSeconds(FADE_SECONDS)
            };
            fadeOut = new DoubleAnimation()
            {
                From = SettingsManager.AppSettings.BgmVolume,
                To = 0,
                Duration = TimeSpan.FromSeconds(FADE_SECONDS)
            };
        }

        private void _PadoruTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(TriggerPadoru);
        }

        private void MusicPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Source = new Uri(_ExecutablePath + GetRandomSong());
            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, fadeIn);
        }

        string GetRandomSong()
        {
            string song = _CurrentSong;
            Random rnd = new Random(DateTime.Now.Millisecond);
            song = _songs[rnd.Next(0, _songs.Length - 1)];
            _CurrentSong = song;
            return song;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Source = new Uri(_ExecutablePath + GetRandomSong());
            MusicPlayer.Play();
            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, fadeIn);
        }

        private void TriggerPadoru()
        {
            MusicPlayer.Pause();
            nero.Visibility = Visibility.Visible;
            nero.BeginAnimation(Canvas.LeftProperty, leftAnim);
            PadoruPlayer.Play();
            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, fadeIn);
        }

        private void PadoruPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            PadoruPlayer.Stop();
            Random rnd = new Random(DateTime.Now.Millisecond);
            int seconds = 180 + rnd.Next(180, 600);
            LogManager.Log.Info(string.Format("Next padoru in {0} seconds", seconds));
            _PadoruTimer.Interval = seconds * 1000;
            nero.Visibility = Visibility.Collapsed;
            MusicPlayer.Play();
            MusicPlayer.BeginAnimation(MediaElement.VolumeProperty, fadeIn);
        }

        public void SetVolume(double value)
        {
            MusicPlayer.Volume = value;
        }

        public void Enable()
        {
            MusicPlayer.IsEnabled = true;
            PadoruPlayer.IsEnabled = true;

        }

        public void Disable()
        {
            MusicPlayer.IsEnabled = false;
            PadoruPlayer.IsEnabled = false;
        }
    }
}
