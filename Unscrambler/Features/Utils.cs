using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features
{
    public static class Utils
    {
        public static readonly List<CilOpCode> CalculationOpCodes = new List<CilOpCode>()
            {CilOpCodes.Add, CilOpCodes.Sub, CilOpCodes.Mul, CilOpCodes.Div, CilOpCodes.Xor, CilOpCodes.Rem};

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