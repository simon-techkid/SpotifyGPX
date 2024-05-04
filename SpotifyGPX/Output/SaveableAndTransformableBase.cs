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
/// Provides instructions for serializing <typeparamref name="T"/> data to <see langword="byte"/>[] and saving it in the target format.
/// </summary>
/// <typeparam name="T">The source format type</typeparam>
public abstract class SaveableBase<T> : IFileOutput
{
    /// <summary>
    /// The byte encoding of the exported document.
    /// </summary>
    protected abstract Encoding OutputEncoding { get; }

    public abstract string FormatName { get; }

    /// <summary>
    /// The document in format <typeparamref name="T"/> that will be serialized and saved to the disk.
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
    /// Converts <typeparamref name="T"/> to <see langword="byte"/>[].
    /// </summary>
    /// <returns>This <see cref="Document"/>, as <see langword="byte"/>[].</returns>
    protected abstract byte[] ConvertToBytes();
}

/// <summary>
/// Provides instructions for serializing <typeparamref name="T"/> data to <see langword="byte"/>[] and transforming and/or saving it in the target format.
/// </summary>
/// <typeparam name="T">The source format type.</typeparam>
public abstract partial class SaveableAndTransformableBase<T> : SaveableBase<T>, ITransformableOutput
{
    /// <summary>
    /// The <see cref="ReaderOptions"/> when reading the converted XML before transformation.
    /// </summary>
    protected abstract ReaderOptions XmlReaderOptions { get; }

    /// <summary>
    /// If true, and if an XSLT stylesheet doesn't exist, include a stylesheet reference in the exported XML conversion.
    /// </summary>
    protected abstract bool IncludeStylesheetHref { get; }

    /// <summary>
    /// Force the using of defined <see cref="XmlWriterSettings"/> <see cref="XmlSettings"/> for this transformation,
    /// instead of using <see cref="XmlWriterSettings"/> parsed from the XSLT stylesheet output tag.
    /// </summary>
    protected abstract bool ForceUseOfSpecifiedSettings { get; }

    /// <summary>
    /// The reference to the XSLT stylesheet, if <see cref="IncludeStylesheetHref"/> is <see langword="true"/>.
    /// </summary>
    protected XProcessingInstruction? StylesheetReference => IncludeStylesheetHref ? new("xml-stylesheet", $"href=\"{StylesheetPath}\" type=\"text/xsl\"") : null;

    /// <summary>
    /// The path to the target XSLT stylesheet for this XML (if <typeparamref name="T"/> is <see cref="XDocument"/>)
    /// or for this file in its XML transformed (<see cref="TransformToXml"/>) form.
    /// </summary>
    protected abstract string StylesheetPath { get; }

    /// <summary>
    /// The settings for the writing of this XML document or XML transformation.
    /// </summary>
    protected abstract XmlWriterSettings XmlSettings { get; }

    /// <summary>
    /// Transforms the document, in format <typeparamref name="T"/>, to XML and saves it to the disk.
    /// </summary>
    /// <param name="name">The file name of the target transformed document.</param>
    /// <param name="xsltPath">The path to an XSLT stylesheet that, if it exists, will be used for transformation.</param>
    public void TransformAndSave(string name)
    {
        string transformation;
        string outputPath;

        if (File.Exists(StylesheetPath))
        {
            // Built in XSL transformer
            XslCompiledTransform transformer = CreateTransformer(StylesheetPath);
            transformation = TransformToXmlToStringWithXslt(transformer);
            outputPath = $"{name}.{GetFormat(transformer.OutputSettings?.OutputMethod)}";
        }
        else
        {
            transformation = TransformToXmlToString();
            outputPath = $"{name}.xml";
        }

        Save(outputPath, OutputEncoding.GetBytes(transformation));
    }

    private static XslCompiledTransform CreateTransformer(string xsltPath)
    {
        XslCompiledTransform xslt = new(EnableDebugXsltTransformations);
        XsltSettings settings = new(EnableXsltDocumentFunction, EnableXsltScript);
        XmlUrlResolver resolver = new();
        xslt.Load(xsltPath, settings, resolver);
        return xslt;
    }

    private string TransformToXmlToStringWithXslt(XslCompiledTransform transformer)
    {
        XDocument document = TransformToXml(); // Create the XML document
        XmlWriterSettings? settings = ForceUseOfSpecifiedSettings == true ? XmlSettings : transformer.OutputSettings; // Use the specified settings if forced
        using MemoryStream ms = new(); // Stream for the transformed document
        using XmlWriter xw = XmlWriter.Create(ms, settings); // Writer for the transformed document
        transformer.Transform(document.CreateReader(XmlReaderOptions), xw); // Transform the document
        ms.Seek(0, SeekOrigin.Begin); // Reset the stream position
        using StreamReader sr = new(ms); // Reader for the transformed document
        string result = sr.ReadToEnd(); // Read the transformed document
        return result; // Return the transformed document
    }

    private static string GetFormat(XmlOutputMethod? method) => method switch
    {
        XmlOutputMethod.Xml => "xml",
        XmlOutputMethod.Text => "txt",
        XmlOutputMethod.Html => "html",
        _ => "txt" // Force plain text if unknown format
    };

    /// <summary>
    /// Converts <typeparamref name="T"/> to <see cref="XDocument"/>.
    /// </summary>
    /// <returns><typeparamref name="T"/> as an XML document, <see cref="XDocument"/>.</returns>
    protected abstract XDocument TransformToXml();

    /// <summary>
    /// Converts <typeparamref name="T"/> to <see cref="XDocument"/> and then converts it to <see langword="string"/>.
    /// </summary>
    /// <returns>A <see langword="string"/>, representing the string representation of the XML <see cref="XDocument"/> of this file.</returns>.
    protected string TransformToXmlToString()
    {
        using MemoryStream ms = new();
        using (XmlWriter xmlWriter = XmlWriter.Create(ms, XmlSettings))
        {
            new XDocument(StylesheetReference, TransformToXml().Root).Save(xmlWriter);
            xmlWriter.Flush();
        }

        return OutputEncoding.GetString(ms.ToArray());
    }
}

/// <summary>
/// Provides instructions for serializing and transforming JSON data using System.Text.Json.
/// </summary>
public abstract class JsonSaveable : SaveableAndTransformableBase<List<JsonDocument>>
{
    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> for the exported contents of this JSON document.
    /// </summary>
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
    protected override Encoding OutputEncoding => XmlSettings.Encoding;

    protected override byte[] ConvertToBytes()
    {
        string xmlString = TransformToXmlToString();
        return OutputEncoding.GetBytes(xmlString);
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
