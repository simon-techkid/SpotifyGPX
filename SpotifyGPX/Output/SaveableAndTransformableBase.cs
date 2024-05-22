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
/// <typeparam name="T">The source format type.</typeparam>
public abstract class SaveableBase<T> : IFileOutput
{
    protected SaveableBase(Func<IEnumerable<SongPoint>> pairs, string? trackName)
    {
        DataProvider = pairs;
        Document = SaveAction(trackName);
    }

    public abstract string FormatName { get; }

    /// <summary>
    /// The document in format <typeparamref name="T"/> that will be serialized and saved to the disk.
    /// </summary>
    protected T Document { get; private set; }
    public abstract int Count { get; }

    /// <summary>
    /// Provides access to the collection of <see cref="SongPoint"/> pairs to be saved to the document in format <typeparamref name="T"/>.
    /// </summary>
    protected Func<IEnumerable<SongPoint>> DataProvider { get; }

    /// <summary>
    /// A delegate to access the contents of the document in format <typeparamref name="T"/>.
    /// </summary>
    /// <returns></returns>
    protected delegate T DocumentAccessor(string? trackName);

    /// <summary>
    /// Provides access to the document in format <typeparamref name="T"/> that will be serialized and saved to the disk.
    /// </summary>
    protected abstract DocumentAccessor SaveAction { get; }

    public void Save(string path)
    {
        byte[] doc = ConvertToBytes();
        Save(path, doc);
    }

    protected virtual void Save(string path, byte[] bytes)
    {
        using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write);
        fileStream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Converts <typeparamref name="T"/> to <see langword="byte"/>[].
    /// </summary>
    /// <returns>This <see cref="Document"/>, as <see langword="byte"/>[].</returns>
    protected abstract byte[] ConvertToBytes();

    /// <summary>
    /// Clears the contents of the <see cref="Document"/> in preparation for disposal.
    /// </summary>
    /// <returns>A <typeparamref name="T"/> that has been cleared.</returns>
    protected abstract T ClearDocument();

    public virtual void Dispose()
    {
        Document = ClearDocument();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Provides instructions for serializing <typeparamref name="T"/> data to <see langword="byte"/>[] and transforming and/or saving it in the target format.
/// </summary>
/// <typeparam name="T">The source format type.</typeparam>
public abstract partial class SaveableAndTransformableBase<T> : SaveableBase<T>, ITransformableOutput
{
    protected SaveableAndTransformableBase(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

    /// <summary>
    /// The byte encoding of the exported document.
    /// Default value: <see cref="Encoding.UTF8"/>, override to change.
    /// </summary>
    protected virtual Encoding OutputEncoding => Encoding.UTF8;

    /// <summary>
    /// The <see cref="ReaderOptions"/> when reading the converted XML before transformation.
    /// Default value: <see cref="ReaderOptions.None"/>, override to change.
    /// </summary>
    protected virtual ReaderOptions XmlReaderOptions => ReaderOptions.None;

    /// <summary>
    /// If true, and if an XSLT stylesheet doesn't exist, include a stylesheet reference in the exported XML conversion.
    /// Default value: <see langword="true"/>, override to change.
    /// </summary>
    protected virtual bool IncludeStylesheetHref => true;

    /// <summary>
    /// Force the using of defined <see cref="XmlWriterSettings"/> <see cref="XmlSettings"/> for this transformation,
    /// instead of using <see cref="XmlWriterSettings"/> parsed from the XSLT stylesheet output tag.
    /// Default value: <see langword="false"/>, override to change.
    /// </summary>
    protected virtual bool ForceUseOfSpecifiedSettings => false;

    /// <summary>
    /// The reference to the XSLT stylesheet, if <see cref="IncludeStylesheetHref"/> is <see langword="true"/>.
    /// </summary>
    protected virtual XProcessingInstruction? ProcessingInstruction => IncludeStylesheetHref ? new("xml-stylesheet", $"href=\"{StylesheetPath}\" type=\"text/xsl\"") : null;

    /// <summary>
    /// The path to the target XSLT stylesheet for this XML (if <typeparamref name="T"/> is <see cref="XDocument"/>)
    /// or for this file in its XML transformed (<see cref="TransformToXml"/>) form.
    /// Default value: <see cref="SaveableBase{T}.FormatName"/>.xslt, override to change.
    /// </summary>
    protected virtual string StylesheetPath => $"{FormatName}.xslt";

    /// <summary>
    /// The settings for the writing of this XML document or XML transformation.
    /// Must be overidden to be used for XML or XML transformed documents.
    /// This <see cref="XmlSettings"/> encoding will be used as the override for <see cref="OutputEncoding"/>.
    /// </summary>
    protected abstract XmlWriterSettings XmlSettings { get; }

    /// <summary>
    /// Transforms the document, in format <typeparamref name="T"/>, to XML and saves it to the disk.
    /// </summary>
    /// <param name="name">The file name of the target transformed document.</param>
    public virtual void TransformAndSave(string name)
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

    protected virtual string TransformToXmlToStringWithXslt(XslCompiledTransform transformer)
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

    protected virtual string GetFormat(XmlOutputMethod? method) => method switch
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
    protected virtual string TransformToXmlToString()
    {
        using MemoryStream ms = new();
        using (XmlWriter xmlWriter = XmlWriter.Create(ms, XmlSettings))
        {
            new XDocument(ProcessingInstruction, TransformToXml().Root).Save(xmlWriter);
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
    protected JsonSaveable(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

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

    protected override List<JsonDocument> ClearDocument()
    {
        return new();
    }
}

/// <summary>
/// Provides instructions for serializing and transforming XML data.
/// </summary>
public abstract class XmlSaveable : SaveableAndTransformableBase<XDocument>
{
    protected XmlSaveable(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

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

    protected override XDocument ClearDocument()
    {
        return new XDocument();
    }
}

/// <summary>
/// Provides instructions for serializing and transforming TXT data.
/// </summary>
public abstract class TxtSaveable : SaveableAndTransformableBase<string?[]>
{
    protected TxtSaveable(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

    /// <summary>
    /// Default line ending for the TXT document.
    /// Default value: <see cref="Environment.NewLine"/>, override to change.
    /// </summary>
    protected virtual string LineEnding => Environment.NewLine;

    protected override byte[] ConvertToBytes()
    {
        string doc = string.Join(LineEnding, Document);
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

    protected override string[] ClearDocument()
    {
        return Array.Empty<string>();
    }
}

/// <summary>
/// Provides instructions for serializing an array of bytes.
/// </summary>
public abstract class ByteSaveable : SaveableBase<byte[]>
{
    protected ByteSaveable(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

    protected override byte[] ConvertToBytes()
    {
        return Document;
    }

    protected override byte[] ClearDocument()
    {
        return Array.Empty<byte>();
    }
}
