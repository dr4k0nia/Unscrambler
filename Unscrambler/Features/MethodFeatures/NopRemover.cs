using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class NopRemover : IMethodFeature
    {
        private int _count;

        public void Process( MethodDefinition method )
        {
            var targets = GetTargets( method );
            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                if ( instr[i].OpCode != CilOpCodes.Nop ||
                     targets.Contains( instr[i] ) )
                    continue;
                instr.RemoveAt( i );
                i--;
                _count++;
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Removed {_count} additional Nop instructions", Logger.LogType.Success );
        }

        private static List<CilInstruction> GetTargets( MethodDefinition method )
        {
            var labels = new List<CilInstruction>();
            foreach ( var instruction in method.CilMethodBody.Instructions )
            {
                switch ( instruction.Operand )
                {
                    case CilInstructionLabel label:
                        labels.Add( label.Instruction );
                        break;
                    case IList<CilInstructionLabel> list:
                        labels.AddRange( list.Select( t => t.Instruction ) );
                        break;
                }
            }

            foreach ( var handler in method.CilMethodBody.ExceptionHandlers )
            {
                if ( handler.FilterStart is CilInstructionLabel filterStart )
                    labels.Add( filterStart.Instruction );
                if ( handler.HandlerStart is CilInstructionLabel handlerStart )
                    labels.Add( handlerStart.Instruction );
                if ( handler.HandlerEnd is CilInstructionLabel handlerEnd )
                    labels.Add( handlerEnd.Instruction );
                if ( handler.TryStart is CilInstructionLabel tryStart )
                    labels.Add( tryStart.Instruction );
                if ( handler.TryEnd is CilInstructionLabel tryEnd )
                    labels.Add( tryEnd.Instruction );
            }

            return labels;
        }
    }
}