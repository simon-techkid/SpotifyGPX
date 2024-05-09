// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;

namespace SpotifyGPX;

public partial class ArgumentParser
{
    public static (Dictionary<string, string> options, HashSet<string> flags) Parse(string[] args)
    {
        var options = new Dictionary<string, string>();
        var flags = new HashSet<string>();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.StartsWith("--"))
            {
                if (i + 1 < args.Length)
                {
                    string key = arg[2..];
                    string value = args[i + 1];
                    options[key] = value;
                    i++;
                }
                else
                {
                    throw new ArgumentException($"Expected value after {arg}");
                }
            }
            else if (arg.StartsWith('-'))
            {
                string flag = arg[1..];
                flags.Add(flag);
            }
        }

        return (options, flags);
    }

    public static void PrintHelp(Broadcaster bcast)
    {
        bcast.Type = "HELP";
        bcast.Broadcast(Help);
    }
}
