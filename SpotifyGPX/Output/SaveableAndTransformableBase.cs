// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for serializing data in the source format to a byte array and saving it in the target format.
/// </summary>
/// <typeparam name="T">The source format type</typeparam>
public abstract class SaveableBase<T> : IFileOutput
{
    /// <summary>
    /// The byte encoding of the exported document.
    /// </summary>
    protected abstract Encoding OutputEncoding { get; }

    /// <summary>
    /// The document in the associated format that will be serialized and saved to the disk.
    /// </summary>
    protected abstract T Document { get; }
    public abstract int Count { get; }

    public void Save(string path)
    {
        byte[] doc = ConvertToBytes();
        Save(path, doc);
    }

    protected void Save(string path, byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// Converts the format document to bytes.
    /// </summary>
    /// <returns>This document, as a byte array.</returns>
    protected abstract byte[] ConvertToBytes();
}

/// <summary>
/// Provides instructions for serializing data in the source format to a byte array and saving and/or transforming data in the target format.
/// </summary>
/// <typeparam name="T">The source format type.</typeparam>
public abstract class SaveableAndTransformableBase<T> : SaveableBase<T>, ITransformableOutput
{
    protected abstract SaveOptions XmlOptions { get; }

    /// <summary>
    /// Transforms the document to the target format and saves it to the disk.
    /// </summary>
    /// <param name="name">The file name of the target transformed document.</param>
    /// <param name="xsltPath">The path to an XSLT stylesheet that, if it exists, will be used for transformation.</param>
    public void TransformAndSave(string name, string xsltPath)
    {
        string transformation;
        string outputPath;

        if (File.Exists(xsltPath))
        {
            XslCompiledTransform xslt = new();
            XsltSettings sets = new(true, true);
            XmlUrlResolver resolver = new();
            xslt.Load(xsltPath, sets, resolver);
            transformation = XsltTransform(xslt);

            outputPath = $"{name}.{GetFormat(xslt.OutputSettings?.OutputMethod)}";
        }
        else
        {
            transformation = TransformToXml().ToString(XmlOptions);
            outputPath = $"{name}.xml";
        }

        Save(outputPath, OutputEncoding.GetBytes(transformation));
    }

    private string XsltTransform(XslCompiledTransform xslt)
    {
        XDocument document = TransformToXml();
        using var ms = new MemoryStream();
        using var xw = XmlWriter.Create(ms, xslt.OutputSettings);
        xslt.Transform(document.CreateReader(), xw);
        ms.Seek(0, SeekOrigin.Begin);
        using var sr = new StreamReader(ms);
        string result = sr.ReadToEnd();
        return result;
    }

    private static string GetFormat(XmlOutputMethod? method) => method switch
    {
        XmlOutputMethod.Xml => "xml",
        XmlOutputMethod.Text => "txt",
        XmlOutputMethod.Html => "html",
        _ => "xml"
    };

    /// <summary>
    /// Converts the format document to XML.
    /// </summary>
    /// <returns>This document, as an XML document.</returns>
    protected abstract XDocument TransformToXml();
}

/// <summary>
/// Provides instructions for serializing and transforming JSON data using System.Text.Json.
/// </summary>
public abstract class JsonSaveable : SaveableAndTransformableBase<List<JsonDocument>>
{
    protected abstract JsonSerializerOptions JsonOptions { get; }

    protected override byte[] ConvertToBytes()
    {
        JsonArray allTracks = new();
        Document.ForEach(doc => allTracks.Add(doc.RootElement));
        JsonElement encapsulatedTracks = JsonDocument.Parse(allTracks.ToJsonString()).RootElement;
        string document = JsonSerializer.Serialize(encapsulatedTracks, JsonOptions);

        return OutputEncoding.GetBytes(document);
    }

    protected override XDocument TransformToXml()
    {
        XElement root = new("Root");

        foreach (JsonDocument doc in Document)
        {
            JsonElement obj = doc.RootElement;
            XElement element = JsonToXElement(obj);
            root.Add(element);
        }

        return new XDocument(root);
    }

    private static XElement JsonToXElement(JsonElement element)
    {
        XElement xElement;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                xElement = new XElement("Object");
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    XElement childElement = JsonToXElement(property.Value);
                    childElement.Name = property.Name;
                    xElement.Add(childElement);
                }
                break;
            case JsonValueKind.Array:
                xElement = new XElement("Array");
                foreach (JsonElement item in element.EnumerateArray())
                {
                    XElement childElement = JsonToXElement(item);
                    xElement.Add(childElement);
                }
                break;
            default:
                xElement = new XElement("Value", element.ToString());
                break;
        }

        return xElement;
    }
}

/// <summary>
/// Provides instructions for serializing and transforming XML data.
/// </summary>
public abstract class XmlSaveable : SaveableAndTransformableBase<XDocument>
{
    protected override byte[] ConvertToBytes()
    {
        string doc = Document.ToString(XmlOptions);
        return OutputEncoding.GetBytes(doc);
    }

    protected override XDocument TransformToXml()
    {
        return Document;
    }
}

/// <summary>
/// Provides instructions for serializing and transforming TXT data.
/// </summary>
public abstract class TxtSaveable : SaveableAndTransformableBase<string?[]>
{
    protected override byte[] ConvertToBytes()
    {
        string doc = string.Join(Environment.NewLine, Document);
        return OutputEncoding.GetBytes(doc);
    }

    protected override XDocument TransformToXml()
    {
        XElement root = new("Root");

        Document.Select(line => new XElement("Line", line))
            .ToList()
            .ForEach(root.Add);

        return new XDocument(root);
    }
}

/// <summary>
/// Provides instructions for serializing an array of bytes.
/// </summary>
public abstract class ByteSaveable : SaveableBase<byte[]>
{
    protected override byte[] ConvertToBytes()
    {
        return Document;
    }
}
