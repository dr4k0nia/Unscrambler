using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class DoubleParseReplace : IMethodFeature
    {
        private int _count;

        public void Process( MethodDefinition method )
        {
            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                // Search for Call opcode
                if ( instr[i].OpCode != CilOpCodes.Call ||
                     instr[i + 1].OpCode != CilOpCodes.Conv_I4 ||
                     instr[i - 1].OpCode != CilOpCodes.Ldstr ) continue;

                if ( instr[i].Operand.ToString() != "System.Double System.Double::Parse(String)" ) continue;

                // Get string used by original Double.Parse()
                string input = (string) instr[i - 1].Operand;
                // Solve Double.Parse()
                double.TryParse( input, out double result );

                // Change Call to Ldc_I4 and set result of Double.Parse() as operand
                instr[i].OpCode = CilOpCodes.Ldc_I4;
                instr[i].Operand = (int) result;

                // Nop Ldstr and Conv_I4
                instr[i - 1].OpCode = CilOpCodes.Nop;
                instr[i + 1].OpCode = CilOpCodes.Nop;
                
                //Utils.RemoveInstructionRange( instr, new []{1, -1}, ref i );

                _count++;
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} Double.Parse() implementations", Logger.LogType.Success );
        }
    }
}