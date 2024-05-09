// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Text;

namespace SpotifyGPX.Observation;

public class FileObserver : Observer, IDisposable
{
    private string FilePath { get; }
    private Encoding Encoding { get; }
    private System.Text.StringBuilder Document { get; set; }
    public FileObserver(string filePath, Encoding encoding) : base()
    {
        FilePath = filePath;
        Encoding = encoding;
        Document = new();
    }
    protected override string CompletionMessage => "All files have been saved.";
    protected override void HandleMessage(string message)
    {
        Document.AppendLine(message);
    }

    public void Dispose()
    {
        using (FileStream stream = new(FilePath, FileMode.Create, FileAccess.Write))
        {
            byte[] bytes = Encoding.GetBytes(Document.ToString());
            stream.Write(bytes, 0, bytes.Length);
        }

        GC.SuppressFinalize(this);
    }
}
