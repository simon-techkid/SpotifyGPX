// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with formats sharing the ability to transform XML using an XSLT stylesheet.
/// </summary>
public interface ITransformableOutput
{
    void TransformAndSave(string name, string xsltPath); // Allows the transformation of the file to another format
}
