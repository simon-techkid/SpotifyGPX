// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with formats sharing the ability to transform XML using an XSLT stylesheet.
/// </summary>
public interface ITransformableOutput
{
    /// <summary>
    /// Transform this file to an XML document, and then save it to the disk.
    /// </summary>
    /// <param name="name">The name of this file, without the extension.</param>
    void TransformAndSave(string name);
}
