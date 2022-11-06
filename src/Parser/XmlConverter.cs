using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Nintendo.Byml.IO;
using Syroot.BinaryData.Core;

namespace Nintendo.Byml.Parser
{
    internal static class XmlConverter
    {
        public static string ToXml(BymlFile data)
        {
            using var stream = new MemoryStream();
            using XmlTextWriter xr = new(stream, Encoding.Unicode);

            xr.Formatting = Formatting.Indented;

            xr.WriteStartDocument();
            xr.WriteStartElement("Root");
            xr.WriteStartElement("isBigEndian");
            xr.WriteAttributeString("Value", (data.Endianness == Endian.Big).ToString());
            xr.WriteEndElement();
            xr.WriteStartElement("BymlFormatVersion");
            xr.WriteAttributeString("Value", data.Version.ToString());
            xr.WriteEndElement();
            xr.WriteStartElement("SupportPaths");
            xr.WriteAttributeString("Value", data.SupportPaths.ToString());
            xr.WriteEndElement();

            xr.WriteStartElement("BymlRoot");
            WriteNode(data.RootNode, null, xr);
            xr.WriteEndElement();

            xr.WriteEndElement();
            xr.Close();
            return Encoding.Unicode.GetString(stream.ToArray());
        }

        public static BymlFile FromXml(string xmlString)
        {
            BymlFile byml = new();
            XmlDocument xml = new();
            xml.LoadXml(xmlString);
            XmlNode n = xml.SelectSingleNode("/Root/isBigEndian");
            byml.Endianness = n.Attributes["Value"].Value.ToLower() == "true" ? Endian.Big : Endian.Little;
            n = xml.SelectSingleNode("/Root/BymlFormatVersion");
            byml.Version = ushort.Parse(n.Attributes["Value"].Value);
            n = xml.SelectSingleNode("/Root/SupportPaths");
            byml.SupportPaths = n.Attributes["Value"].Value.ToLower() == "true";

            n = xml.SelectSingleNode("/Root/BymlRoot");
            if (n.ChildNodes.Count != 1) throw new Exception("A byml can have only one root");
            byml.RootNode = ParseNode(n.FirstChild);

            return byml;
        }

        //
        // Xml Writer
        #region Expand

        static void WriteNode(dynamic node, string name, XmlTextWriter xmlWriter)
        {
            if (node == null)
            {
                if (name == null) return;
                xmlWriter.WriteStartElement("NULL");
                xmlWriter.WriteAttributeString("N", name);
                xmlWriter.WriteEndElement();
            }
            else if (node is IList<dynamic> castListNode)
            {
                WriteArrNode(castListNode, name, xmlWriter);
            }
            else if (node is IDictionary<string, dynamic> castDictNode)
            {
                WriteDictNode(castDictNode, name, xmlWriter);
            }
            else
            {
                xmlWriter.WriteStartElement(GetNodeName(node));
                if (name != null)
                    xmlWriter.WriteAttributeString("N", name);

                xmlWriter.WriteAttributeString("V", node.ToString());
                xmlWriter.WriteEndElement();
            }
        }

        static void WriteArrNode(IList<dynamic> node, string name, XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(GetNodeName(node));
            if (name != null)
                xmlWriter.WriteAttributeString("N", name);

            for (int i = 0; i < node.Count; i++)
                WriteNode(node[i], null, xmlWriter);

            xmlWriter.WriteEndElement();
        }

        static void WriteDictNode(IDictionary<string, dynamic> node, string name, XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(GetNodeName(node));
            if (name != null)
                xmlWriter.WriteAttributeString("N", name);

            var keys = node.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
                WriteNode(node[keys[i]], keys[i], xmlWriter);

            xmlWriter.WriteEndElement();
        }

        static string GetNodeName(dynamic node) => "T" + ((byte)NodeTypeExtension.GetNodeType(node)).ToString();

#endregion

        //
        // XML Reader
        #region Expand

        internal static dynamic ParseNode(XmlNode xmlNode)
        {
            if (xmlNode.Name == "NULL") return null;

            NodeType nodeType = (NodeType)byte.Parse(xmlNode.Name[1..]);

            return nodeType switch
            {
                NodeType.Array => ParseArrNode(xmlNode),
                NodeType.Hash => ParseDictNode(xmlNode),
                _ => XmlTypeConverter.ConvertValue(nodeType.GetInstanceType(), xmlNode.Attributes["V"].Value),
            };
        }

        internal static IDictionary<string, dynamic> ParseDictNode(XmlNode xmlNode)
        {
            Dictionary<string, dynamic> res = new();
            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                var c = xmlNode.ChildNodes[i];
                res.Add(c.Attributes["N"].Value, ParseNode(c));
            }

            return res;
        }

        internal static IList<dynamic> ParseArrNode(XmlNode n)
        {
            List<dynamic> res = new();
            for (int i = 0; i < n.ChildNodes.Count; i++)
                res.Add(ParseNode(n.ChildNodes[i]));

            return res;
        }

        #endregion
    }
}
