using System.Drawing;
using System.IO;
using System.Linq;
using AsmResolver.DotNet;
using Unscrambler.Features;
using Unscrambler.Features.MethodFeatures;
using Console = Colorful.Console;

namespace Unscrambler
{
    internal static class Program
    {
        private static readonly IFeature[] Features =
        {
            new MethodProcessor
            {
                Features =
                {
                    new HideCallsRemover(), new NopRemover(), new CalliReplace(),
                    new MathReplace(), new EmptyTypeReplace(),
                    new DoubleParseReplace(),
                    new SizeOfReplace(), new NopRemover(), new ConvertToIntReplace()
                }
            },
            new InterfaceLoopRemover(),
            new LocalsToFieldRemover(),
        };

        public static void Main( string[] args )
        {
            Console.WriteAscii( "Unscrambler" );
            Console.WriteLine( "Unscrambler v1.0 by drakonia | github.com/dr4k0nia \n", Color.Gold );
            if ( args.Length == 0 )
            {
                Console.WriteLine( "Usage: unscrambler.exe <file>" );
                Console.ReadKey();
                return;
            }

            string inputPath = args[0];
            if ( !File.Exists( inputPath ) )
            {
                Logger.Log( $"Unable to find file: {inputPath}", Logger.LogType.Error );
                Console.ReadKey();
                return;
            }

            var module = ModuleDefinition.FromFile( inputPath );

            Process( module );

            WriteSummary();

            string filepath = inputPath.Insert( inputPath.Length - 4, "_unscrambled" );
            module.Write( filepath );
            
            Logger.Log( $"Saved unscrambled file at: {filepath}", Logger.LogType.Success );
            Console.ReadKey();
        }

        private static void Process( ModuleDefinition module )
        {
            foreach ( var type in module.GetAllTypes() )
            {
                foreach ( var feature in Features )
                {
                    feature.Process( type );
                }
            }

            foreach ( var feature in Features )
            {
                feature.PostProcess( module );
            }
        }

        private static void WriteSummary()
        {
            foreach ( var summary in Features.SelectMany( f => f.GetSummary() ) )
            {
                Logger.Log( summary.Message, summary.LogType );
            }
        }
    }
}