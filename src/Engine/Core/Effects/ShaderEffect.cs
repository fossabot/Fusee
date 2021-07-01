﻿using System;
using System.Collections.Generic;

namespace Fusee.Engine.Core.Effects
{
    /// <summary>
    /// A ShaderEffect contains a string for each, the vertex, fragment and geometry shader and a set of render states.
    /// Use this if you want to write the shader code on your own. 
    /// The values of uniform variables you defined (<see cref="Effect.ParamDecl"/>) can be set using <see cref="Effect.SetFxParam{T}(string, T)"/>. 
    /// </summary>
    public class ShaderEffect : Effect, IDisposable
    {
        /// <summary>
        /// The Vertex shader code.
        /// </summary>
        public string VertexShaderSrc { get; protected set; }

        /// <summary>
        /// The Fragment shader code.
        /// </summary>
        public string PixelShaderSrc { get; protected set; }

        /// <summary>
        /// The Geometry shader code.
        /// </summary>
        public string GeometryShaderSrc { get; protected set; }

        /// <summary>
        /// The constructor to create a shader effect.
        /// </summary>
        /// <param name="effectPass">See <see cref="FxPassDeclaration"/>.</param>
        /// <param name="effectParameters">The list of (uniform) parameters. The concrete type of the object also indicates the parameter's type.
        /// </param>
        /// <remarks> Make sure to insert all uniform variable in "effectParameters" that are declared in the shader code.</remarks>
        public ShaderEffect(FxPassDeclaration effectPass, IEnumerable<IFxParamDeclaration> effectParameters)
        {
            ParamDecl = new Dictionary<string, IFxParamDeclaration>();

            RendererStates = effectPass.StateSet;
            VertexShaderSrc = effectPass.VS;
            PixelShaderSrc = effectPass.PS;
            GeometryShaderSrc = effectPass.GS;

            if (effectParameters != null)
            {
                foreach (var param in effectParameters)
                    ParamDecl.Add(param.Name, param);
            }

            EffectManagerEventArgs = new EffectManagerEventArgs(UniformChangedEnum.Unchanged);
        }

        /// <summary>
        /// Destructor calls <see cref="Dispose"/> in order to fire MeshChanged event.
        /// </summary>
        ~ShaderEffect()
        {
            Dispose();
        }

        /// <summary>
        /// Is called when GC of this shader effect kicks in
        /// </summary>
        public void Dispose()
        {
            EffectChanged?.Invoke(this, new EffectManagerEventArgs(UniformChangedEnum.Dispose));
        }
    }
}