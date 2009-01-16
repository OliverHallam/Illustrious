//------------------------------------------------------------------------------------------------- 
// <copyright file="InlineFunctionCallOptimization.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the InlineFunctionCallOptimization type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Inlines function calls.
    /// </summary>
    public class InlineFunctionCallOptimization : Optimization
    {
        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly Configuration configuration;

        /// <summary>
        /// A cache of the value indicating whether a method should be inlined.
        /// </summary>
        private readonly Dictionary<MethodDefinition, bool> shouldInline = new Dictionary<MethodDefinition, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineFunctionCallOptimization"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public InlineFunctionCallOptimization(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="optimizer">The optimizer calling this method.</param>
        /// <param name="worker">A CIL worker for the current method.</param>
        /// <param name="instruction">
        /// The instruction to be visited.  Receives the first changed instruction of the optimized version.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an optimization was performed; otherwise <see langword="false"/>.
        /// </returns>
        public override bool OptimizeInstruction(Optimizer optimizer, CilWorker worker, ref Instruction instruction)
        {
            // TODO: allow local variables in inlined method
            // TODO: allow arguments
            // TODO: allow return values
            // TODO: allow generic methods
            // TODO: allow non-static methods
            var opCode = instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Call)
            {
                var methodRef = (MethodReference) instruction.Operand;
                var typeRef = methodRef.DeclaringType;
                var module = typeRef.Module;

                var type = module.Types[typeRef.FullName];
                if (type == null)
                {
                    return false;
                }

                var method = type.Methods.GetMethod(methodRef.Name, methodRef.Parameters);
                bool shouldInlineMethod;
                if (!this.shouldInline.TryGetValue(method, out shouldInlineMethod))
                {
                    shouldInlineMethod = this.configuration.ShouldInline(method);
                    this.shouldInline[method] = shouldInlineMethod;
                    optimizer.Visit(method);
                }

                if (shouldInlineMethod)
                {
                    var next = instruction.Next;
                    worker.Remove(instruction);

                    var instructions = method.Body.Instructions;
                    var instructionCount = instructions.Count;
                    if (instructionCount == 0)
                    {
                        instruction = next;
                        return true;
                    }

                    instruction = instructions[0];
                    if (instruction.OpCode.FlowControl == FlowControl.Return)
                    {
                        instruction = next;
                        return true;
                    }

                    worker.InsertBefore(next, instruction);
                    for (var i = 1; i < instructionCount; i++)
                    {
                        var currentInstruction = instructions[i];
                        if (currentInstruction.OpCode.FlowControl != FlowControl.Return)
                        {
                            worker.InsertBefore(next, currentInstruction);
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}