using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class EmptyTypeReplace : IMethodFeature
    {
        private int _count;

        public void Process( MethodDefinition method )
        {
            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                // Search for Ldsfld opcode followed by Ldlen
                if ( instr[i].OpCode != CilOpCodes.Ldsfld ||
                     instr[i + 1].OpCode != CilOpCodes.Ldlen ||
                     instr[i].Operand.ToString() != "System.Type[] System.Type::EmptyTypes" ) 
                    continue;

                instr[i].OpCode = CilOpCodes.Ldc_I4_0;
                instr.RemoveAt( i + 1 );
                i--;

                _count++;
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} EmptyTypes implementations", Logger.LogType.Success );
        }
    }
}