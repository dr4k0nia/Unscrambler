using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace Unscrambler.Features.MethodFeatures
{
    public class MathReplace : IMethodFeature
    {
        private int _count;

        public void Process( MethodDefinition method )
        {
            // Thanks to Anonymoose for helping out
            var instr = method.CilMethodBody.Instructions;
            for ( int i = 2; i < instr.Count; i++ )
            {
                if ( !( instr[i].Operand is MemberReference memberRef ) ||
                     memberRef.DeclaringType.FullName != "System.Math" )
                    continue;
                var mathMethod =
                    typeof(Math).Assembly.ManifestModule.ResolveMethod( memberRef.Resolve().MetadataToken
                        .ToInt32() );
                var arguments = new object[mathMethod.GetParameters().Length];
                for ( int j = 0; j < arguments.Length; j++ )
                {
                    arguments[arguments.Length - j - 1] = instr[i - j - 1].Operand;
                    instr[i - j - 1].OpCode = CilOpCodes.Nop;
                }

                var result = mathMethod.Invoke( null, arguments );
                var opcode = GetOpCode( method.Module.CorLibTypeFactory.FromType(
                    ( (MethodSignature) memberRef.Signature )
                    .ReturnType ).ElementType );
                instr[i].OpCode = opcode;
                instr[i].Operand = result;
                _count++;
            }

            method.CilMethodBody.Instructions.OptimizeMacros();
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} Math implementations", Logger.LogType.Success );
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