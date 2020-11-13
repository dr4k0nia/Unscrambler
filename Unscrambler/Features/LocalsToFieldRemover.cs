using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Unscrambler.Features
{
    public class LocalsToFieldRemover : IFeature
    {
        private int _count;
        private static readonly HashSet<FieldDefinition> FieldsInModule = new HashSet<FieldDefinition>();

        private static readonly Dictionary<FieldDefinition, CilLocalVariable> CreatedLocals =
            new Dictionary<FieldDefinition, CilLocalVariable>();

        public void Process( TypeDefinition type )
        {
            // On first execution, grab fields from global constructor
            if ( FieldsInModule.Count == 0 )
            {
                var globalType = type.Module.GetOrCreateModuleType();

                foreach ( var field in globalType.Fields )
                {
                    // This assumes that the fields have no default value (which the forks Ive checked didnt have)
                    if ( !field.IsStatic || field.IsPrivate || field.HasDefault )
                        continue;
                    
                    CheckUsage( field, type.Module );
                }
            }

            foreach ( var method in type.Methods.Where( m => m.CilMethodBody != null ) )
            {
                var instr = method.CilMethodBody.Instructions;
                for ( int i = 0; i < instr.Count; i++ )
                {
                    if ( instr[i].OpCode.OperandType != CilOperandType.InlineField )
                        continue;
                    if ( !( instr[i].Operand is FieldDefinition field ) || !FieldsInModule.Contains( field ) )
                        continue;

                    // Check if dictionary already contains a local for the field, if not create a new one and add it to the dictionary
                    if ( !CreatedLocals.TryGetValue( field, out var createdLocal ) )
                    {
                        createdLocal = new CilLocalVariable( field.Signature.FieldType );
                        method.CilMethodBody.LocalVariables.Add( createdLocal );
                        CreatedLocals.Add( field, createdLocal );
                    }

                    // Get local from dictionary
                    instr[i].OpCode = GetOpCode( instr[i].OpCode.Code );
                    instr[i].Operand = createdLocal;
                    _count++;
                }

                instr.OptimizeMacros();
            }
        }

        public void PostProcess( ModuleDefinition module )
        {
            var globalType = module.GetModuleType();

            foreach ( var item in CreatedLocals )
            {
                globalType.Fields.Remove( item.Key );
            }
        }

        public IEnumerable<Summary> GetSummary()
        {
            if ( _count > 0 )
                yield return new Summary( $"Removed {_count} Local to Field implementations", Logger.LogType.Success );
        }

        private CilOpCode GetOpCode( CilCode opcode )
        {
            return opcode switch
            {
                CilCode.Stsfld => CilOpCodes.Stloc,
                CilCode.Ldsfld => CilOpCodes.Ldloc,
                CilCode.Ldsflda => CilOpCodes.Ldloca,
                _ => CilOpCodes.Nop
            };
        }
        
        private void CheckUsage(FieldDefinition field, ModuleDefinition module)
        {
            int match = 0;
            foreach ( var type in module.GetAllTypes())
            {
                foreach ( var method in type.Methods.Where( m => m.CilMethodBody != null ) )
                {
                    if ( method.CilMethodBody.Instructions.Any( i =>
                        i.Operand is FieldDefinition matchedField && matchedField == field ) )
                        match++;
                }
            }
            if ( match == 1 )
                FieldsInModule.Add( field );
        }
    }
}