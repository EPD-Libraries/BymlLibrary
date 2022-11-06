using System;
using System.Collections;
using System.Collections.Generic;

namespace Nintendo.Byml.IO
{
    /// <summary>
    /// BymlNodeType extension methods
    /// </summary>
    internal static class NodeTypeExtension
	{
		internal static bool IsEnumerable(this NodeType nodeType) => nodeType >= NodeType.Array && nodeType <= NodeType.StringArray;

		/// <summary>
		/// Gets the corresponding, instantiatable <see cref="Type"/> for the given <paramref name="nodeType"/>.
		/// </summary>
		/// <param name="nodeType">The <see cref="NodeTypeExtension"/> which should be instantiated.</param>
		/// <returns>The <see cref="Type"/> to instantiate for the node.</returns>
		internal static Type GetInstanceType(this NodeType nodeType)
		{
            return nodeType switch
            {
                NodeType.String => typeof(string),
				// TODO: Check if this could be loaded as an object array.
				NodeType.Array => throw new BymlException("Cannot instantiate an array of unknown element type."),
				// TODO: Check if this could be loaded as a string-object dictionary.
                NodeType.Hash => throw new BymlException("Cannot instantiate an object of unknown type."),
                NodeType.Bool => typeof(bool),
                NodeType.Int => typeof(int),
                NodeType.Float => typeof(float),
                NodeType.UInt => typeof(uint),
                NodeType.Int64 => typeof(long),
                NodeType.UInt64 => typeof(ulong),
                NodeType.Double => typeof(double),
                NodeType.Null => typeof(object),
                _ => throw new BymlException($"Unknown node type {nodeType}."),
            };
        }

        /// <summary>
        /// Gets the BymlNodeType of the given <paramref name="node"/> object.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isInternalNode"></param>
        /// <returns></returns>
        /// <exception cref="BymlException"></exception>
        internal static NodeType GetNodeType(dynamic node, bool isInternalNode = false)
        {
            if (isInternalNode)
            {
                return node switch
                {
                    IEnumerable<string> => NodeType.StringArray,
                    _ => throw new BymlException($"Type '{node.GetType()}' is not supported as a main BYAML node."),
                };
            }
            else
            {
                return node switch
                {
                    string => NodeType.String,
                    IDictionary<string, dynamic> => NodeType.Hash,
                    IEnumerable => NodeType.Array,
                    bool => NodeType.Bool,
                    int => NodeType.Int,
                    float => NodeType.Float, // TODO decimal is float or double?
                    uint => NodeType.UInt,
                    long => NodeType.Int64,
                    ulong => NodeType.UInt64,
                    double => NodeType.Double,
                    null => NodeType.Null,
                    _ => throw new BymlException($"Type '{node.GetType()}' is not supported as a BYAML node."),
                };
            }
        }
    }
}
