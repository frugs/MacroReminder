using System;
using System.IO;
using System.Media;
using System.Reflection;
using JetBrains.Annotations;

namespace MacroReminder
{
    public class NotificationPlayer
    {
        private const string GenericNotificationResourcePath = "MacroReminder.Resources.notification.wav";
        private const string StartNotificationResourcePath = "MacroReminder.Resources.start.wav";
        private const string StopNotificationResourcePath = "MacroReminder.Resources.stop.wav";

        private readonly Lazy<SoundPlayer> _genericNotificationSound =
            new Lazy<SoundPlayer>(() => CreateFromResource(GenericNotificationResourcePath));

        private readonly Lazy<SoundPlayer> _startNotificationSound =
            new Lazy<SoundPlayer>(() => CreateFromResource(StartNotificationResourcePath));

        private readonly Lazy<SoundPlayer> _stopNotificationSound =
            new Lazy<SoundPlayer>(() => CreateFromResource(StopNotificationResourcePath));

        [CanBeNull] private SoundPlayer _customNotificationSound;

        private static SoundPlayer CreateFromResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            return new SoundPlayer(stream);
        }

        public void PlayNotification()
        {
            if (_customNotificationSound != null)
            {
                _customNotificationSound.Play();
            }
            else
            {
                _genericNotificationSound.Value.Play();
            }
        }

        public void PlayStartNotification()
        {
            _startNotificationSound.Value.Play();
        }

        public void PlayStopNotification()
        {
            _stopNotificationSound.Value.Play();
        }

        public void SetCustomNotificationSound(Stream fileStream)
        {
            _customNotificationSound = new SoundPlayer(fileStream);
            _customNotificationSound.Load();
        }

        public void SetDefaultNotificationSound()
        {
            _customNotificationSound = null;
        }
    }
}