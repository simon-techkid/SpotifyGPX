// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SpotifyGPX.Options;

namespace SpotifyGPX.PointPredict;

partial class PointPredict
{
    private static double CalculateDistance((double, double) coord1, (double, double) coord2)
    {
        double lat1 = coord1.Item1;
        double lon1 = coord1.Item2;
        double lat2 = coord2.Item1;
        double lon2 = coord2.Item2;

        double latDiff = lat2 - lat1;
        double lonDiff = lon2 - lon1;

        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);

        return distance;
    }
}
