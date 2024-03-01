// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with formats supporting transformation (with or without an XSLT stylesheet) to XML.
/// </summary>
public interface ITransformable
{
    void Transform(string name, string xsltPath); // Allows the transformation of the file to another format
}

/// <summary>
/// Provides instructions for transforming data to XML using XSLT stylesheets.
/// </summary>
/// <typeparam name="T">The type of data being serialized to XML.</typeparam>
public abstract class TransformableBase<T>
{
    protected abstract T Document { get; }

    public TransformationResult Transform(string xsltPath)
    {
        XDocument serializedData = SerializeData();

        if (!File.Exists(xsltPath))
        {
            return new TransformationResult(serializedData, null);
        }

        XDocument transformedDocument = new();

        XslCompiledTransform xslt = new();
        xslt.Load(xsltPath);

        // Create an XmlWriter to write the transformed XML directly to the XDocument
        using (XmlWriter xw = transformedDocument.CreateWriter())
        {
            // Perform the transformation
            xslt.Transform(serializedData.CreateReader(), xw);
        }

        return new TransformationResult(transformedDocument, xslt.OutputSettings);
    }

    protected abstract XDocument SerializeData();
}

/// <summary>
/// Provides access to the resulting document and settings of the transformation.
/// </summary>
public class TransformationResult
{
    public XDocument TransformedDocument { get; }
    public XmlWriterSettings? TransformationSettings { get; }
    public Encoding? TargetEncoding => TransformationSettings?.Encoding;
    public XmlOutputMethod? TargetOutputMethod => TransformationSettings?.OutputMethod;

    public TransformationResult(XDocument document, XmlWriterSettings? settings)
    {
        TransformedDocument = document;
        TransformationSettings = settings;
    }

    public string Format
    {
        get
        {
            return TargetOutputMethod switch
            {
                XmlOutputMethod.Xml => "xml",
                XmlOutputMethod.Text => "txt",
                XmlOutputMethod.Html => "html",
                XmlOutputMethod.AutoDetect => "xml",
                _ => "xml"
            };
        }
    }
}

public class XmlTransformer : TransformableBase<XDocument>
{
    protected override XDocument Document { get; }

    public XmlTransformer(XDocument document)
    {
        Document = document;
    }

    protected override XDocument SerializeData()
    {
        return Document;
    }
}

public class JsonTransformer : TransformableBase<List<JObject>>
{
    protected override List<JObject> Document { get; }

    public JsonTransformer(List<JObject> document)
    {
        Document = document;
    }

    protected override XDocument SerializeData()
    {
        XElement root = new("Root");

        Document.Select(obj => JsonConvert.DeserializeXNode(obj.ToString(), "Root")?.Root)
            .Where(element => element != null)
            .ToList()
            .ForEach(root.Add);

        return new XDocument(root);
    }
}

public class TxtTransformer : TransformableBase<string?[]>
{
    protected override string?[] Document { get; }

    public TxtTransformer(string?[] document)
    {
        Document = document;
    }

    protected override XDocument SerializeData()
    {
        XElement root = new("Root");

        Document.Select(line => new XElement("Line", line))
            .ToList()
            .ForEach(root.Add);

        return new XDocument(root);
    }
}
