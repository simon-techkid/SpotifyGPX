// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Observation;

public abstract class Observer<T> : IObserver<T>
{
    /// <summary>
    /// Whether or not this observer is silenced (do not disturb mode).
    /// </summary>
    public bool Silent { get; set; }

    /// <summary>
    /// Whether or not this observer's do not disturb <see cref="Silent"/> state should be ignored.
    /// </summary>
    protected virtual bool IgnoreSilent => false;

    public Observer() { }

    /// <summary>
    /// Called when the observable sequence is completed.
    /// </summary>
    public void OnCompleted()
    {
        bool silentState = Silent;
        Silent = false;
        HandleCompletion();
        Silent = silentState;
    }

    /// <summary>
    /// Handles the completion of the observable sequence.
    /// </summary>
    protected abstract void HandleCompletion();

    /// <summary>
    /// Called when an error occurs in the observable sequence.
    /// </summary>
    /// <param name="error"></param>
    public void OnError(Exception error)
    {
        bool silentState = Silent;
        Silent = false;
        HandleException(error);
        Silent = silentState;
    }

    /// <summary>
    /// Handles an exception thrown by the observable sequence.
    /// </summary>
    /// <param name="exception"></param>
    protected abstract void HandleException(Exception exception);

    /// <summary>
    /// Called when a new value is produced by the observable sequence.
    /// </summary>
    /// <param name="value"></param>
    public void OnNext(T value)
    {
        WriteLine(value);
    }

    /// <summary>
    /// Handles a new message produced by the observable sequence.
    /// </summary>
    /// <param name="message">The message of type <typeparamref name="T"/> to take in.</param>
    protected abstract void HandleMessage(T message);

    /// <summary>
    /// The log level of this observer.
    /// </summary>
    public abstract LogLevel Level { get; }

    /// <summary>
    /// Determines whether or not a message should be handled by this observer.
    /// </summary>
    /// <param name="lvl">The <see cref="LogLevel"/> of a new incoming message.</param>
    /// <returns>True, if the message can be sent to this observer. Otherwise, false.</returns>
    public bool MessageMatcher(LogLevel lvl) => MessageMatch(lvl);

    /// <summary>
    /// Determines whether or not a message should be handled by this observer based on the log level of the message.
    /// </summary>
    protected virtual Func<LogLevel, bool> MessageMatch => lvl => lvl >= Level;

    /// <summary>
    /// Writes a new message to the observer if <see cref="Silent"/> is <see langword="false"/> or <see cref="IgnoreSilent"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="value">A message of type <typeparamref name="T"/> to add to the observer if <see cref="Silent"/> is <see langword="false"/> or <see cref="IgnoreSilent"/> is <see langword="true"/>.</param>
    private void WriteLine(T value)
    {
        if (!Silent || IgnoreSilent)
        {
            HandleMessage(value);
        }
    }
}
