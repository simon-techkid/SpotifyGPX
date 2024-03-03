// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace SpotifyGPX;

public interface IHashProvider<T>
{
    string ComputeHash(T data);
}

public abstract class FormatHashProviderBase<T> : IHashProvider<T>
{
    protected abstract string SerializeData(T data);

    public string ComputeHash(T data)
    {
        string serializedData = SerializeData(data);

        byte[] bytes = Encoding.UTF8.GetBytes(serializedData);
        byte[] hashBytes = SHA256.HashData(bytes);

        System.Text.StringBuilder builder = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            builder.Append(hashBytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}

public class XmlHashProvider : FormatHashProviderBase<IEnumerable<XElement>>
{
    protected override string SerializeData(IEnumerable<XElement> data)
    {
        return string.Join("", data.Select(element => element.ToString()));
    }
}

public class JsonHashProvider<T> : FormatHashProviderBase<T>
{
    protected override string SerializeData(T data)
    {
        return JsonConvert.SerializeObject(data);
    }
}
