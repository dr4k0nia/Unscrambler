using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace Unscrambler.Features.MethodFeatures
{
    public class MathReplace : IMethodFeature
    {
        private int _count;
        
        private static readonly List<CilInstruction> InstructionsToRemove = new List<CilInstruction>();
        
        // Thanks to Anonymoose for helping out
        public void Process( MethodDefinition method )
        {
            method.CilMethodBody.Instructions.ExpandMacros();
            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                if ( !( instr[i].Operand is MemberReference memberRef ) ||
                     memberRef.DeclaringType.FullName != "System.Math" )
                    continue;
                var mathMethod =
                    typeof(Math).Assembly.ManifestModule.ResolveMethod( memberRef.Resolve().MetadataToken
                        .ToInt32() );

                var arguments = GetArguments( mathMethod, instr, i );

                if ( arguments.Any(o => o is null))
                {
                    InstructionsToRemove.Clear();
                    continue;
                }

                var result = mathMethod.Invoke( null, arguments );
                var opcode = GetOpCode( method.Module.CorLibTypeFactory.FromType(
                    ( (MethodSignature) memberRef.Signature )
                    .ReturnType ).ElementType );
                instr[i].OpCode = opcode;
                instr[i].Operand = result;
                _count++;
                
                foreach ( var instruction in InstructionsToRemove )
                {
                    instruction.OpCode = CilOpCodes.Nop;
                }
                InstructionsToRemove.Clear();
            }

            method.CilMethodBody.Instructions.OptimizeMacros();
        }
        
        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} Math implementations", Logger.LogType.Success );
        }

        private static object[] GetArguments( MethodBase mathMethod, CilInstructionCollection instr, int i )
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
                        InstructionsToRemove.Add( instr[i - j - 1]  );
                        break;
                    default:
                        arguments[arguments.Length - j - 1] = null;
                        break;
                }
            }

            return arguments;
        }

        // Ported to AsmResolver, original taken from
        // https://github.com/wwh1004/ConfuserExTools/blob/master/ConstantKiller/ConstantKillerImpl.cs#L110
        private static CilOpCode GetOpCode( ElementType returnType )
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
    }
}