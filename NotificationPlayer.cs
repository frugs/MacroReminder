using System.Media;
using System.Reflection;

namespace MacroReminder
{
    public class NotificationPlayer
    {
        private const string NotificationResourcePath = "MacroReminder.Resources.notification.wav";
        
        private readonly SoundPlayer _soundPlayer;
        
        public NotificationPlayer()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(NotificationResourcePath);
            _soundPlayer = new SoundPlayer(stream);
        }
        
        public void PlayNotification()
        {
            _soundPlayer.Play();
        }
    }
}