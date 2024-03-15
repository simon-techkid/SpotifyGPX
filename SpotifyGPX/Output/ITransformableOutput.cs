// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with formats sharing the ability to save and transform data.
/// </summary>
public interface ITransformableOutput
{
    void TransformAndSave(string name, string xsltPath); // Allows the transformation of the file to another format
}
