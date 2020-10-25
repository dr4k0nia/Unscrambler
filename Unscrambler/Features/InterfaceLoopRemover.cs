using System.Collections.Generic;
using AsmResolver.DotNet;

namespace Unscrambler.Features
{
    public class InterfaceLoopRemover : IFeature
    {
        private int _count;
        private readonly List<TypeDefinition> _typesForRemoval = new List<TypeDefinition>();

        public void Process( TypeDefinition type )
        {
            // Only check types with 2 or more interfaces
            if ( type.Interfaces.Count < 2 ) return;

            foreach ( var interfaceImplementation in type.Interfaces )
            {
                // Check if interface type matches implementation type
                if ( interfaceImplementation.Interface != type ) 
                    continue;

                _typesForRemoval.Add( type );
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Removed {_count} Interface Loop implementations", Logger.LogType.Success );
        }

        // Take the Types from the before generated list and remove the from the processed module
        public void PostProcess( ModuleDefinition module )
        {
            foreach ( var type in _typesForRemoval )
            {
                if ( module.TopLevelTypes.Remove( type ) )
                    _count++;
                else
                    Logger.Log( $"Failed to remove Type {type.Name} matched as Interface Loop", Logger.LogType.Error );
            }
        }
    }
}