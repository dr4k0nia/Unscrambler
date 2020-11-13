using System;
using System.Collections.Generic;
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
            return returnType switch
            {
                ElementType.Boolean => CilOpCodes.Ldc_I4,
                ElementType.Char => CilOpCodes.Ldc_I4,
                ElementType.I1 => CilOpCodes.Ldc_I4,
                ElementType.U1 => CilOpCodes.Ldc_I4,
                ElementType.I2 => CilOpCodes.Ldc_I4,
                ElementType.U2 => CilOpCodes.Ldc_I4,
                ElementType.I4 => CilOpCodes.Ldc_I4,
                ElementType.U4 => CilOpCodes.Ldc_I4,
                ElementType.I8 => CilOpCodes.Ldc_I8,
                ElementType.U8 => CilOpCodes.Ldc_I8,
                ElementType.R4 => CilOpCodes.Ldc_R4,
                ElementType.R8 => CilOpCodes.Ldc_R8,
                _ => throw new InvalidOperationException()
            };
        }
    }
}