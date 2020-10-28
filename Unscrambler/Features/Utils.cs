using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace Unscrambler.Features
{
    public static class Utils
    {
        public static readonly List<CilOpCode> CalculationOpCodes = new List<CilOpCode>()
            {CilOpCodes.Add, CilOpCodes.Sub, CilOpCodes.Mul, CilOpCodes.Div, CilOpCodes.Xor, CilOpCodes.Rem};
        
        // Ported to AsmResolver, original taken from
        // https://github.com/wwh1004/ConfuserExTools/blob/master/ConstantKiller/ConstantKillerImpl.cs#L110
        public static CilOpCode GetOpCode( ElementType returnType )
        {
            switch ( returnType )
            {
                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                    return CilOpCodes.Ldc_I4;
                case ElementType.I8:
                case ElementType.U8:
                    return CilOpCodes.Ldc_I8;
                case ElementType.R4:
                    return CilOpCodes.Ldc_R4;
                case ElementType.R8:
                    return CilOpCodes.Ldc_R8;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static void RemoveInstructionRange( CilInstructionCollection instr, IEnumerable<int> removalIndexes,
            ref int index )
        {
            foreach ( var removalIndex in removalIndexes )
            {
                instr.RemoveAt( index + removalIndex );
                index--;
            }
        }
    }
}