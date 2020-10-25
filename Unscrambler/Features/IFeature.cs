using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;

namespace Unscrambler.Features
{
    public interface IFeature
    {
        void Process( TypeDefinition type );
        void PostProcess( ModuleDefinition module );

        IEnumerable<Summary> GetSummary();
    }

    public interface IMethodFeature
    {
        void Process( MethodDefinition method );

        IEnumerable<Summary> GetSummary();
    }

    public readonly struct Summary
    {
        public Summary( string message, Logger.LogType logType )
        {
            Message = message;
            LogType = logType;
        }

        public string Message { get; }
        public Logger.LogType LogType { get; }
    }

    public class MethodProcessor : IFeature
    {
        public ICollection<IMethodFeature> Features { get; } = new List<IMethodFeature>();

        public void Process( TypeDefinition type )
        {
            foreach ( var method in type.Methods.Where( m => m.CilMethodBody != null ) )
            {
                foreach ( var feature in Features )
                    feature.Process( method );
            }
        }

        void IFeature.PostProcess( ModuleDefinition module )
        {
        }

        public IEnumerable<Summary> GetSummary()
        {
            return Features.SelectMany( feature => feature.GetSummary() );
        }
    }
}