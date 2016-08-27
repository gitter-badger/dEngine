using System.Windows.Media;
using dEngine;

namespace dEditor.Widgets.Output
{
    public class OutputItem
    {
        public string Message { get; }
        public LogLevel Level { get; }
        public string Logger { get; }
        public Brush Brush { get; }

        public OutputItem(ref string message, ref LogLevel level, ref string logger)
        {
            Brush = Brushes.Black;

            switch (level)
            {
                case LogLevel.Error:
                    Brush = Brushes.Red;
                    break;
                case LogLevel.Warn:
                    Brush = Brushes.Orange;
                    break;
                case LogLevel.Trace:
                    Brush = Brushes.Blue;
                    break;
                case LogLevel.Info:
                    if (logger.StartsWith("dEngine.Instances.Network") ||
                        logger.StartsWith("dEngine.Services.ContentProvider"))
                        Brush = Brushes.Purple;
                    message = message.Substring(message.IndexOf(" - ") + 3);
                    break;
            }

            Message = message;
            Level = level;
            Logger = logger;
        }
    }
}