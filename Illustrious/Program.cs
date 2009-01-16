//------------------------------------------------------------------------------------------------- 
// <copyright file="Program.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the Program type.</summary>
//-------------------------------------------------------------------------------------------------
namespace Inliner
{
    using Mono.Cecil;

    /// <summary>
    /// Contains the application entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The application entry point.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        public static void Main(string[] arguments)
        {
            // TODO: sanitize arguments
            var assemblyPath = arguments[0];

            var assembly = AssemblyFactory.GetAssembly(assemblyPath);

            var configuration = new Configuration();
            var optimization = new InlineFunctionCallOptimization(configuration);
            var rewriter = new Optimizer(optimization);
            rewriter.Visit(assembly);

            var assemblyName = assembly.Name.Name;
            var assemblyKind = assembly.Kind;

            // TODO: accept output file name from command line
            var outputFileName = string.Format(
                "{0}.inlined.{1}",
                assemblyName,
                assemblyKind == AssemblyKind.Dll ? "dll" : "exe");

            AssemblyFactory.SaveAssembly(assembly, outputFileName);
        }
    }
}
