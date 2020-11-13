using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class HideCallsRemover : IMethodFeature
    {
        private int _count;

        public void Process( MethodDefinition method )
        {
            // Skip all methods without exception handlers
            if ( method.CilMethodBody.ExceptionHandlers == null )
                return;

            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                if ( instr[i].OpCode != CilOpCodes.Calli || instr[i + 1].OpCode != CilOpCodes.Sizeof ) 
                    continue;
                
                //Check if instruction operand is null
                if ( instr[i].Operand is null ) 
                    continue;

                // Search for a finally handler
                var handlers = method.CilMethodBody.ExceptionHandlers;
                for ( int j = 0; j < handlers.Count; j++ )
                {
                    if ( handlers[j].HandlerType != CilExceptionHandlerType.Finally ) 
                        continue;
                    handlers.RemoveAt( j );
                    j--;
                    _count++;
                }

                instr[i].OpCode = CilOpCodes.Nop;
                instr[i + 1].OpCode = CilOpCodes.Nop;
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Removed {_count} HideCalls implementations", Logger.LogType.Success );
        }
    }
}