// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for serializing and transforming XML data.
/// </summary>
public abstract class XmlSaveable : SaveableAndTransformableBase<XDocument, IEnumerable<XElement>>
{
    protected XmlSaveable(Func<IEnumerable<SongPoint>> pairs, string? trackName, StringBroadcaster bcast) : base(pairs, bcast, trackName)
    {
    }

    protected override IHashProvider<IEnumerable<XElement>> HashProvider => new XmlHashProvider(OutputEncoding);

    protected override Encoding OutputEncoding => XmlSettings.Encoding;

    /// <summary>
    /// The XML namespace for the document type.
    /// </summary>
    protected virtual XNamespace Namespace => XNamespace.Xmlns;

    protected override byte[] ConvertToBytes()
    {
        string xmlString = TransformToXmlToString();
        return OutputEncoding.GetBytes(xmlString);
    }

    protected override XDocument TransformToXml()
    {
        return Document;
    }

    protected override XDocument ClearDocument()
    {
        return new XDocument();
    }
}
