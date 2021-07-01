﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fusee.Engine.Core.Effects
{
    /// <summary>
    /// A parameter declaration contains the name and type of the shader parameter, as well as a flag, that indicates in which types of shaders this parameter is used.
    /// </summary>
    public interface IFxParamDeclaration
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The Type of the parameter.
        /// </summary>
        Type ParamType { get; }

        /// <summary>
        /// Defines in which type of shader this parameter is used in.
        /// </summary>
        IEnumerable<ShaderCategory> UsedInShaders { get; }
    }

    /// <summary>
    /// A data type for the list of (uniform) parameters possibly occurring in one of the shaders in the various passes.
    /// Each of this array entry consists of the parameter's name and its initial value. The concrete type of the object also indicates the
    /// parameter's type.
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
    public struct FxParamDeclaration<T> : IFxParamDeclaration
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the parameter.
        /// </summary>
        public T Value;

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public Type ParamType => typeof(T);

        IEnumerable<ShaderCategory> IFxParamDeclaration.UsedInShaders => _usedInShaders;

        private readonly IEnumerable<ShaderCategory> _usedInShaders;
    }
}