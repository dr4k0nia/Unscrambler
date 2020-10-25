using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class ConvertToIntReplace : IMethodFeature
    {
        private int _count;

        public void Process( MethodDefinition method )
        {
            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                // Search for Call opcode
                if ( instr[i].OpCode != CilOpCodes.Call ) continue;
                if ( instr[i].Operand.ToString() != "System.Int32 System.Convert::ToInt32(Double)" ) 
                    continue;

                // Solve implementations like this Convert.ToInt32(-28.0 - -95.0)
                // Doing this the ghetto way for now, plan is to use emulation in the future
                if ( Utils.CalculationOpCodes.Contains( instr[i - 1].OpCode ) )
                {
                    if ( instr[i - 2].OpCode != CilOpCodes.Ldc_R8 || instr[i - 3].OpCode != CilOpCodes.Ldc_R8 )
                        continue;

                    if ( Solve( method, i ) )
                    {
                        instr[i - 1].OpCode = CilOpCodes.Nop;
                        instr[i - 2].OpCode = CilOpCodes.Nop;
                        instr[i - 3].OpCode = CilOpCodes.Nop;
                        _count++;
                        continue;
                    }
                }

                if ( instr[i - 1].OpCode != CilOpCodes.Ldc_R8 ) continue;
                instr[i].OpCode = CilOpCodes.Ldc_I4;
                instr[i].Operand = Convert.ToInt32( (double) instr[i - 1].Operand );
                instr[i - 1].OpCode = CilOpCodes.Nop;
                _count++;
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} Convert.ToInt32() implementations",
                    Logger.LogType.Success );
        }
        
        
        private static bool Solve( MethodDefinition method, int i )
        {
            var instr = method.CilMethodBody.Instructions;
            double a = (double) instr[i - 3].Operand;
            double b = (double) instr[i - 2].Operand;
            if ( instr[i - 1].OpCode == CilOpCodes.Add )
            {
                instr[i].OpCode = CilOpCodes.Ldc_I4;
                instr[i].Operand = Convert.ToInt32( a + b );
                return true;
            }

            if ( instr[i - 1].OpCode == CilOpCodes.Sub )
            {
                instr[i].OpCode = CilOpCodes.Ldc_I4;
                instr[i].Operand = Convert.ToInt32( a - b );
                return true;
            }

            if ( instr[i - 1].OpCode == CilOpCodes.Mul )
            {
                instr[i].OpCode = CilOpCodes.Ldc_I4;
                instr[i].Operand = Convert.ToInt32( a * b );
                return true;
            }

            if ( instr[i - 1].OpCode == CilOpCodes.Div )
            {
                instr[i].OpCode = CilOpCodes.Ldc_I4;
                instr[i].Operand = Convert.ToInt32( a / b );
                return true;
            }

            return false;
        }
    }
}