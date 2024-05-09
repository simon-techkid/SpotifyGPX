// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for serializing <typeparamref name="TDocument"/> data to <see langword="byte"/>[] and transforming and/or saving it in the target format.
/// </summary>
/// <typeparam name="TDocument">The source format type.</typeparam>
/// <typeparam name="THashed">The format type of the hashable portion of this document.</typeparam>
public abstract partial class SaveableAndTransformableBase<TDocument, THashed> : SaveableBase<TDocument, THashed>, ITransformableOutput
{
    protected SaveableAndTransformableBase(Func<IEnumerable<SongPoint>> pairs, Broadcaster bcast, string? trackName = null) : base(pairs, bcast, trackName)
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
    /// The path to the target XSLT stylesheet for this XML (if <typeparamref name="TDocument"/> is <see cref="XDocument"/>)
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
    /// Transforms the document, in format <typeparamref name="TDocument"/>, to XML and saves it to the disk.
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
    /// Converts <typeparamref name="TDocument"/> to <see cref="XDocument"/>.
    /// </summary>
    /// <returns><typeparamref name="TDocument"/> as an XML document, <see cref="XDocument"/>.</returns>
    protected abstract XDocument TransformToXml();

    /// <summary>
    /// Converts <typeparamref name="TDocument"/> to <see cref="XDocument"/> and then converts it to <see langword="string"/>.
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
