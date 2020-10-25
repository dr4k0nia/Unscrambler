using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class SizeOfReplace : IMethodFeature
    {
        private int _count;

        public void Process( MethodDefinition method )
        {
            // Check if module is 32Bit, required for GetImpliedMemoryLayout()
            bool is32Bit = method.Module.IsBit32Preferred || method.Module.IsBit32Required;

            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                // Search for Sizeof opcode
                if ( instr[i].OpCode != CilOpCodes.Sizeof ) 
                    continue;

                var op = (ITypeDefOrRef) instr[i].Operand;
                // Determine integer value of operand type
                int value = (int) op.GetImpliedMemoryLayout( is32Bit ).Size;

                instr[i].OpCode = CilOpCodes.Ldc_I4;
                instr[i].Operand = value;
                _count++;

                // Optimize IL
                instr.OptimizeMacros();
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} sizeof() implementations", Logger.LogType.Success );
        }
    }
}