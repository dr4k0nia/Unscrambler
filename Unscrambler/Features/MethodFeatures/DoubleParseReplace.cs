using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class DoubleParseReplace : IMethodFeature
    {
        private int _count;
        
        private readonly List<CilInstruction> InstructionsToRemove = new List<CilInstruction>();

        public void Process( MethodDefinition method )
        {
            var instr = method.CilMethodBody.Instructions;
            instr.ExpandMacros();
            for ( int i = 0; i < instr.Count; i++ )
            {
                if ( !( instr[i].Operand is MemberReference memberRef ) ||
                     !memberRef.DeclaringType.IsTypeOf( "System", "Double" ) )
                    continue;

                var methodBase =
                    typeof(double).Assembly.ManifestModule.ResolveMethod( memberRef.Resolve().MetadataToken
                        .ToInt32() );
                
                var arguments = GetArguments( methodBase, instr, i );

                if ( arguments.Any(o => o is null))
                {
                    InstructionsToRemove.Clear();
                    continue;
                }
                
                var result = methodBase.Invoke( null, arguments );

                instr[i].OpCode = CilOpCodes.Ldc_R8;
                instr[i].Operand = result;
                _count++;
                
                foreach ( var instruction in InstructionsToRemove )
                {
                    instruction.OpCode = CilOpCodes.Nop;
                }
                InstructionsToRemove.Clear();
            }
            instr.OptimizeMacros();
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} Double.Parse() implementations", Logger.LogType.Success );
        }
        
        private object[] GetArguments( MethodBase methodBase, CilInstructionCollection instr, int i )
        {
            var arguments = new object[methodBase.GetParameters().Length];
            for ( int j = 0; j < arguments.Length; j++ )
            {
                switch ( instr[i - j - 1].OpCode.OperandType )
                {
                    case CilOperandType.InlineMethod:
                    case CilOperandType.InlineI:
                    case CilOperandType.InlineString:
                        arguments[arguments.Length - j - 1] = instr[i - j - 1].Operand;
                        InstructionsToRemove.Add( instr[i - j - 1]  );
                        break;
                    default:
                        arguments[arguments.Length - j - 1] = null;
                        break;
                }
            }

            return arguments;
        }
        
        
    }
}