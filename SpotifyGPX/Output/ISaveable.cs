// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with formats sharing the ability to save data.
/// </summary>
public interface ISaveable
{
    void Save(string path);
}

/// <summary>
/// Provides instructions for serializing data in the source format to a string and transforming and/or saving data in the target format.
/// </summary>
/// <typeparam name="T">The source format type.</typeparam>
public abstract class SaveableAndTransformableBase<T> : ISaveable, ITransformable
{
    protected abstract T Document { get; }
    protected abstract SaveOptions OutputOptions { get; }
    protected abstract Encoding OutputEncoding { get; }

    public void Save(string path)
    {
        string doc = SerializeDocument(Document);
        File.WriteAllText(path, doc, OutputEncoding);
    }

    public void Transform(string name, string xsltPath)
    {
        TransformationResult transformation = TransformDocument(Document, xsltPath);
        string doc = transformation.TransformedDocument.ToString(OutputOptions);
        string outputPath = $"{name}.{transformation.Format}";
        File.WriteAllText(outputPath, doc, transformation.TargetEncoding ?? OutputEncoding);
    }

    protected abstract string SerializeDocument(T document);

    protected abstract TransformationResult TransformDocument(T document, string xsltPath);
}

/// <summary>
/// Provides instructions for serializing and transforming JSON data.
/// </summary>
public abstract class JsonSaveable : SaveableAndTransformableBase<List<JObject>>
{
    protected abstract JsonSerializerSettings JsonSettings { get; }

    protected override string SerializeDocument(List<JObject> document)
    {
        return JsonConvert.SerializeObject(document, JsonSettings.Formatting, JsonSettings);
    }

    protected override TransformationResult TransformDocument(List<JObject> document, string xsltPath)
    {
        return new JsonTransformer(document).Transform(xsltPath);
    }
}

/// <summary>
/// Provides instructions for serializing and transforming XML data.
/// </summary>
public abstract class XmlSaveable : SaveableAndTransformableBase<XDocument>
{
    protected override string SerializeDocument(XDocument document)
    {
        return document.ToString(OutputOptions);
    }

    protected override TransformationResult TransformDocument(XDocument document, string xsltPath)
    {
        return new XmlTransformer(document).Transform(xsltPath);
    }
}

/// <summary>
/// Provides instructions for serializing and transforming TXT data.
/// </summary>
public abstract class TxtSaveable : SaveableAndTransformableBase<string?[]>
{
    protected override string SerializeDocument(string?[] document)
    {
        return string.Join(Environment.NewLine, document);
    }

    protected override TransformationResult TransformDocument(string?[] document, string xsltPath)
    {
        return new TxtTransformer(document).Transform(xsltPath);
    }
}