// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;

namespace SpotifyGPX.Pairings;

public abstract class PairingsFactory : StringBroadcasterBase
{
    private bool PointPredict { get; }
    private bool AutoPredictPoints { get; }

    protected PairingsFactory(StringBroadcaster BCast, bool pointPredict = false, bool autoPredict = false) : base(BCast)
    {
        PointPredict = pointPredict;
        AutoPredictPoints = autoPredict;
    }

    /// <summary>
    /// The name of the PairingsHandler to be created.
    /// </summary>
    protected virtual string PairingsSetName => $"Pairings-on_{DateTimeOffset.Now.UtcDateTime.ToString(Options.ISO8601UTC)}";

    protected override string BroadcasterPrefix => "PAIRINGSHANDLER";

    /// <summary>
    /// Get the appropriate PairingsHandler instance.
    /// </summary>
    /// <returns></returns>
    public abstract PairingsHandler GetHandler();

    /// <summary>
    /// Instantiate an empty <see cref="PairingsHandler"/> class instance, containing only the <see langword="string"/> <see cref="PairingsHandler.Name"/> and <see cref="BroadcasterBase{T}.BCaster"/> broadcaster.
    /// This <see cref="PairingsHandler"/> does not yet contain calculated pairings.
    /// You must run CalculatePairings() to calculate the pairings on this <see cref="PairingsHandler"/> instance.
    /// </summary>
    /// <returns></returns>
    protected PairingsHandler GetDupeOrRegHandler()
    {
        BCaster.Broadcast($"Welcome to the PairingsHandler creation tool.", Observation.LogLevel.Debug);
        BCaster.Broadcast($"Creating a {(PointPredict == true ? "dupes" : "normal")} PairingsHandler.", Observation.LogLevel.Debug);
        BCaster.Broadcast($"PairingsHandler name: '{PairingsSetName}'", Observation.LogLevel.Debug);

        PairingsHandler handler;

        if (PointPredict)
        {
            if (AutoPredictPoints)
            {
                BCaster.Broadcast("Auto-predicting points.", Observation.LogLevel.Debug);
                handler = new DupeHandlerAuto(PairingsSetName, BCaster.Clone());
            }
            else
            {
                BCaster.Broadcast("Manual-predicting points.", Observation.LogLevel.Debug);
                handler = new DupeHandlerManual(PairingsSetName, BCaster.Clone());
            }
        }
        else
        {
            BCaster.Broadcast("Running traditional pair calculation.", Observation.LogLevel.Debug);
            handler = new PairerVanilla(PairingsSetName, BCaster.Clone());
        }

        return handler;
    }
}
