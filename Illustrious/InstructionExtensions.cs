//------------------------------------------------------------------------------------------------- 
// <copyright file="InstructionExtensions.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the InstructionExtensions type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// Extension methods for the <see cref="OpCode"/> struct.
    /// </summary>
    public static class InstructionExtensions
    {
        /// <summary>
        /// Determines whether the specified instruction is a <c>ldloc</c> instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="location">The target location of the <c>ldloc</c> instruction.</param>
        /// <returns>
        /// <c>true</c> if the specified instruction is a <c>ldloc</c> instruction; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLdloc(this Instruction instruction, out int location)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldloc_0:
                    location = 0;
                    return true;

                case Code.Ldloc_1:
                    location = 1;
                    return true;

                case Code.Ldloc_2:
                    location = 2;
                    return true;

                case Code.Ldloc_3:
                    location = 3;
                    return true;

                case Code.Ldloc_S:
                    location = ((VariableDefinition) instruction.Operand).Index;
                    return true;

                case Code.Ldloc:
                    location = ((VariableDefinition) instruction.Operand).Index;
                    return true;

                default:
                    location = 0;
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified instruction is a <c>ldloc</c> instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="location">The target location of the <c>ldloc</c> instruction.</param>
        /// <returns>
        /// <c>true</c> if the specified instruction is a <c>ldloc</c> instruction; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLdloca(this Instruction instruction, out int location)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldloca_S:
                    location = ((VariableDefinition)instruction.Operand).Index;
                    return true;

                case Code.Ldloca:
                    location = ((VariableDefinition)instruction.Operand).Index;
                    return true;

                default:
                    location = 0;
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified instruction is a <c>stloc</c> instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="location">The target location of the <c>stloc</c> instruction.</param>
        /// <returns>
        /// <c>true</c> if the specified instruction is a <c>stloc</c> instruction; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStloc(this Instruction instruction, out int location)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Stloc_0:
                    location = 0;
                    return true;

                case Code.Stloc_1:
                    location = 1;
                    return true;

                case Code.Stloc_2:
                    location = 2;
                    return true;

                case Code.Stloc_3:
                    location = 3;
                    return true;
                    
                case Code.Stloc_S:
                    location = ((VariableDefinition)instruction.Operand).Index;
                    return true;

                case Code.Stloc:
                    location = ((VariableDefinition)instruction.Operand).Index;
                    return true;

                default:
                    location = 0;
                    return false;
            }
        }
    }
}
