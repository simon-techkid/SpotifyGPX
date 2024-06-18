// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Text;

namespace SpotifyGPX.Observation;

public partial class FileObserver : Observer<string>, IDisposable
{
    private string FilePath { get; }
    private Encoding Encoding { get; }
    public override LogLevel Level { get; }
    private System.Text.StringBuilder Document { get; set; }

    public FileObserver(string filePath, Encoding encoding, LogLevel level) : base()
    {
        FilePath = filePath;
        Encoding = encoding;
        Level = level;
        Document = new();
    }

    protected override void HandleMessage(string message)
    {
        Document.AppendLine(message);
    }

    protected override void HandleException(Exception exception)
    {
        Document.AppendLine(exception.ToString());
    }

    protected override void HandleCompletion()
    {
        Console.WriteLine("All files have been saved.");
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
