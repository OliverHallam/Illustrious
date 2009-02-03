//------------------------------------------------------------------------------------------------- 
// <copyright file="Optimizer.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the Optimizer type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Performs optimization of an assembly.
    /// </summary>
    public class Optimizer : Visitor
    {
        /// <summary>
        /// The optimization to apply.
        /// </summary>
        private readonly Optimization[] optimizations;

        /// <summary>
        /// The set of methods that have been optimized.
        /// </summary>
        private readonly HashSet<MethodDefinition> visitedMethods = new HashSet<MethodDefinition>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Optimizer"/> class.
        /// </summary>
        /// <param name="optimizations">The optimizations to apply.</param>
        public Optimizer(params Optimization[] optimizations)
        {
            this.optimizations = optimizations;
        }

        /// <summary>
        /// Visits the specified method.
        /// </summary>
        /// <param name="methodDefinition">The method to visit.</param>
        public override void Visit(MethodDefinition methodDefinition)
        {
            if (this.visitedMethods.Contains(methodDefinition))
            {
                return;
            }

            this.visitedMethods.Add(methodDefinition);

            var body = methodDefinition.Body;
            if (body == null || body.Instructions.Count == 0)
            {
                return;
            }

            var worker = new OptimizationWorker(this, body);
            var instruction = body.Instructions[0];
            do
            {
                var instructionBefore = instruction.Previous;

                if (this.ApplyOptimizations(instruction, worker))
                {
                    instruction = instructionBefore;
                    if (instruction == null)
                    {
                        instruction = body.Instructions[0];
                        continue;
                    }
                }

                instruction = instruction.Next;
            }
            while (instruction != null);
        }

        /// <summary>
        /// Applies all optimizations to the specified instruction.
        /// </summary>
        /// <param name="instruction">The instruction to optimize.</param>
        /// <param name="worker">The worker to apply optimizations on.</param>
        /// <returns><see langword="true"/> if an optimization has been performed; <see langword="false" /> otherwise.</returns>
        private bool ApplyOptimizations(Instruction instruction, OptimizationWorker worker)
        {
            var optimizationCount = this.optimizations.Length;
            for (var i = 0; i < optimizationCount; i++)
            {
                var optimization = this.optimizations[i];
                optimization.OptimizeInstruction(instruction, worker);
                if (worker.WasOptimized)
                {
                    worker.WasOptimized = false;
                    return true;
                }
            }

            return false;
        }
    }
}