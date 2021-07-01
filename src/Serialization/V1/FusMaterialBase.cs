﻿using ProtoBuf;
using System;

namespace Fusee.Serialization.V1
{
    /// <summary>
    /// Material definition. If contained within a node, the node's (and potentially child node's)
    /// geometry is rendered with the specifies material.
    /// This base material does not contain any information about specular reflection calculation.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(200, typeof(FusMaterialStandard))]
    [ProtoInclude(201, typeof(FusMaterialBRDF))]
    [ProtoInclude(202, typeof(FusMaterialDiffuseBRDF))]
    [ProtoInclude(203, typeof(FusMaterialGlossyBRDF))]
    public class FusMaterialBase : FusComponent, IEquatable<FusMaterialBase>
    {
        #region Albedo
        /// <summary>
        /// Gets a value indicating whether this instance has an albedo channel.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has an albedo channel; otherwise, <c>false</c>.
        /// </value>
        public bool HasAlbedoChannel => Albedo != null;

        /// <summary>
        /// The albedo channel.
        /// </summary>
        [ProtoMember(1)]
        public AlbedoChannel? Albedo;
        #endregion

        #region Emission
        /// <summary>
        /// Gets a value indicating whether this instance has an emissive channel.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has an emissive channel; otherwise, <c>false</c>.
        /// </value>
        public bool HasEmissiveChannel => Emissive != null;

        /// <summary>
        /// The emissive channel.
        /// </summary>
        [ProtoMember(3)]
        public AlbedoChannel? Emissive;
        #endregion

        #region NormalMap
        /// <summary>
        /// Gets a value indicating whether this instance has a normal map channel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a normal map channel; otherwise, <c>false</c>.
        /// </value>
        public bool HasNormalMapChannel => NormalMap != null;

        /// <summary>
        /// The normal map channel.
        /// </summary>
        [ProtoMember(4)]
        public NormapMapChannel? NormalMap;
        #endregion

        #region equals

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="lhs">The first instance.</param>
        /// <param name="rhs">The second instance.</param>
        /// <returns>
        /// True, if left does equal right; false otherwise.
        /// </returns>
        public static bool operator ==(FusMaterialBase lhs, FusMaterialBase rhs)
        {
            // Check for null on left side.
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="lhs">The first instance.</param>
        /// <param name="rhs">The second instance.</param>
        /// <returns>
        /// True, if left does not equal right; false otherwise.
        /// </returns>
        public static bool operator !=(FusMaterialBase lhs, FusMaterialBase rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Indicates whether the NormapMapChannel is equal to another one.
        /// </summary>
        /// <param name="other">The NormapMapChannel to compare with this one.</param>
        /// <returns>
        /// true if the current NormapMapChannel is equal to the other; otherwise, false.
        /// </returns>
        public bool Equals(FusMaterialBase other)
        {
            if (other is null)
                return false;
            return other.Albedo == Albedo && other.Emissive == Emissive && other.NormalMap == NormalMap;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>
        /// True if the instances are equal; false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
                return false;
            else
                return Equals((FusMaterialBase)obj);

        }

        /// <summary>
        /// Returns the hash for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Albedo, Emissive, NormalMap);
        }

        #endregion
    }
}