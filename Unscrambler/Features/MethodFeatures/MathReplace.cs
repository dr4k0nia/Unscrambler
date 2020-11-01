using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;

namespace Unscrambler.Features.MethodFeatures
{
    public class MathReplace : IMethodFeature
    {
        private int _count;
        
        private readonly List<CilInstruction> _instructionsToRemove = new List<CilInstruction>();
        
        // Thanks to Anonymoose for helping out
        public void Process( MethodDefinition method )
        {
            method.CilMethodBody.Instructions.ExpandMacros();
            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                if ( !( instr[i].Operand is MemberReference memberRef ) ||
                     !memberRef.DeclaringType.IsTypeOf( "System", "Math" ) )
                    continue;
                var mathMethod =
                    typeof(Math).Assembly.ManifestModule.ResolveMethod( memberRef.Resolve().MetadataToken
                        .ToInt32() );

                var arguments = GetArguments( mathMethod, instr, i );

                if ( arguments.Any(o => o is null))
                {
                    _instructionsToRemove.Clear();
                    continue;
                }

                var result = mathMethod.Invoke( null, arguments );
                var opcode = Utils.GetOpCode( method.Module.CorLibTypeFactory.FromType(
                    ( (MethodSignature) memberRef.Signature )
                    .ReturnType ).ElementType );
                instr[i].OpCode = opcode;
                instr[i].Operand = result;
                _count++;
                
                foreach ( var instruction in _instructionsToRemove )
                {
                    instruction.OpCode = CilOpCodes.Nop;
                }
                _instructionsToRemove.Clear();
            }

            method.CilMethodBody.Instructions.OptimizeMacros();
        }
        
        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} Math implementations", Logger.LogType.Success );
        }

        private object[] GetArguments( MethodBase mathMethod, CilInstructionCollection instr, int i )
        {
            var arguments = new object[mathMethod.GetParameters().Length];
            for ( int j = 0; j < arguments.Length; j++ )
            {
                switch ( instr[i - j - 1].OpCode.OperandType )
                {
                    case CilOperandType.InlineI:
                    case CilOperandType.InlineI8:
                    case CilOperandType.InlineR:
                    case CilOperandType.ShortInlineR:
                        arguments[arguments.Length - j - 1] = instr[i - j - 1].Operand;
                        _instructionsToRemove.Add( instr[i - j - 1]  );
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