// SpotifyGPX by Simon Field

namespace SpotifyGPX.Clipboard
{
    public class Clipboard
    {
        public static void SetClipboard(string content)
        {
            System.Windows.Forms.Clipboard.SetText(content);
        }
    }
}
