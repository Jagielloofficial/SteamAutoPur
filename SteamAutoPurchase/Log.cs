namespace SteamAutoPurchase
{
    public class Log
    {
        private readonly System.IO.StreamWriter _logFile;

        public Log()
        {
            _logFile = new System.IO.StreamWriter("Log.txt");
        }
        public async void Logging(string text)
        {
            await _logFile.WriteLineAsync(System.DateTime.Now + "  " + text);
        }

        ~Log()
        {
            _logFile.Close();
        }

    }
}