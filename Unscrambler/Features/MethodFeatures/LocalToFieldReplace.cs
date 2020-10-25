using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features.MethodFeatures
{
    public class LocalToFieldReplace : IMethodFeature
    {
        private int _count;
        private static readonly List<FieldDefinition> FieldsInModule = new List<FieldDefinition>();

        private static readonly Dictionary<FieldDefinition, CilLocalVariable> CreatedLocals =
            new Dictionary<FieldDefinition, CilLocalVariable>();

        public void Process( MethodDefinition method )
        {
            // On first execution, grab fields from global constructor
            if ( FieldsInModule.Count == 0 )
            {
                var type = method.Module.GetOrCreateModuleType();
                if ( type.Fields == null ) return;

                foreach ( var field in type.Fields )
                {
                    // This assumes that the fields have no default value (which the forks ive checked didnt have)
                    if ( !field.IsStatic || field.IsPrivate || field.HasDefault )
                        continue;
                    FieldsInModule.Add( field );
                }
            }

            var instr = method.CilMethodBody.Instructions;
            for ( int i = 0; i < instr.Count; i++ )
            {
                if ( instr[i].OpCode != CilOpCodes.Stsfld && instr[i].OpCode != CilOpCodes.Ldsfld &&
                     instr[i].OpCode != CilOpCodes.Ldsflda )
                    continue;
                if ( !( instr[i].Operand is FieldDefinition field ) || !FieldsInModule.Contains( field ) )
                    continue;

                var fieldToReplace = FieldsInModule.Find( f => f == field );
                CilLocalVariable local;

                // Check if dictionary already contains a local for the field, if not create a new one and add it to the dictionary
                if ( !CreatedLocals.ContainsKey( fieldToReplace ) )
                {
                    local = new CilLocalVariable( fieldToReplace.Signature.FieldType );
                    method.CilMethodBody.LocalVariables.Add( local );
                    CreatedLocals.Add( fieldToReplace, local );
                }

                // Get local from dictionary
                CreatedLocals.TryGetValue( fieldToReplace, out var createdLocal );
                local = createdLocal;

                instr[i].OpCode = GetOpCode( instr[i].OpCode );
                instr[i].Operand = local;
                instr.OptimizeMacros();
                _count++;
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Replaced {_count} Local to Field implementations", Logger.LogType.Success );
        }


        private static CilOpCode GetOpCode( CilOpCode opcode )
        {
            if ( opcode == CilOpCodes.Stsfld )
                return CilOpCodes.Stloc;
            if ( opcode == CilOpCodes.Ldsfld )
                return CilOpCodes.Ldloc;
            if ( opcode == CilOpCodes.Ldsflda )
                return CilOpCodes.Ldloca;

            return CilOpCodes.Nop;
        }
    }
}