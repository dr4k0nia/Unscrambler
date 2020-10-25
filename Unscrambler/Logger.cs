using System;
using System.Drawing;
using Colorful;
using Console = Colorful.Console;

namespace Unscrambler
{
    public static class Logger
    {
        public static void Log( string text, LogType type )
        {
            const string template = "{0} {1}";
            switch ( type )
            {
                case LogType.Success:
                {
                    Formatter[] success =
                    {
                        new Formatter( "[success]:", Color.LimeGreen ),
                        new Formatter( text, Color.White )
                    };
                    Console.WriteLineFormatted( template, Color.Gray, success );
                    break;
                }
                case LogType.Debug:
                {
                    Formatter[] info =
                    {
                        new Formatter( "[debug]:", Color.DodgerBlue ),
                        new Formatter( text, Color.White )
                    };
                    Console.WriteLineFormatted( template, Color.Gray, info );
                    break;
                }
                case LogType.Error:
                {
                    Formatter[] error =
                    {
                        new Formatter( "[error]:", Color.Red ),
                        new Formatter( text, Color.Crimson )
                    };
                    Console.WriteLineFormatted( template, Color.Gray, error );
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException( nameof(type), type, null );
            }
        }

        public enum LogType
        {
            Success = 0,
            Debug = 1,
            Error = 3
        }
    }
}