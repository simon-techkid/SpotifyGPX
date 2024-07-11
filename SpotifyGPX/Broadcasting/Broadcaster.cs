// SpotifyGPX by Simon Field

using SpotifyGPX.Observation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Broadcasting;

public abstract partial class Broadcaster<T> : ICloneable
{
    /// <summary>
    /// The observers of this broadcaster of type <typeparamref name="T"/>.
    /// </summary>
    public List<Observation.IObserver<T>> Observers { get; set; }

    /// <summary>
    /// The hash code of this broadcaster of type <typeparamref name="T"/>.
    /// </summary>
    public int HashCode { get; }

    protected Broadcaster()
    {
        Observers = new();
        HashCode = GetHashCode();
    }

    /// <summary>
    /// Broadcast a message of type <typeparamref name="T"/> to all observers of type <typeparamref name="T"/>.
    /// The default log level for a message is <see cref="LogLevel.Info"/>.
    /// </summary>
    /// <param name="message">The contents of the message to broadcast.</param>
    /// <param name="level">The <see cref="LogLevel"/> of the message.</param>
    public void Broadcast(T message, LogLevel level = DefaultForMessages)
    {
        // Allow special handling of the message before it's sent to observers,
        // based on the log level of the message.
        T broadcastMessage = HandleMessageBasedOnMessageLogLevel(message, level);

        // Send the message to all observers that target this message's log level.
        foreach (var observer in Observers.Where(observer => observer.MessageMatcher(level)))
        {
            // Allow special handling of the message before it's sent to the observer,
            // based on the log level of the observer.
            T levelSpecificMessage = HandleMessageBasedOnObserverLogLevel(broadcastMessage, observer.Level);

            // Allow special handling of the message before it's sent to the observer,
            // based on the log level of the message and the log level of the observer.
            T finalMessage = HandleMessageBasedOnObserverLogLevelAndMessageLogLevel(levelSpecificMessage, level, observer.Level);

            // Send the message to the observer.
            observer.OnNext(finalMessage);
        }
    }

    /// <summary>
    /// Allow special handling by the <see cref="Broadcaster{T}"/> class of the message of type <typeparamref name="T"/> before it is sent based on the <see cref="LogLevel"/> of the message and the <see cref="LogLevel"/> of the observer.
    /// </summary>
    /// <param name="message">The message to be sent, of type <typeparamref name="T"/>.</param>
    /// <param name="messageLevel">The log level of this message.</param>
    /// <param name="observerLevel">The log level of the observer to which the message is being sent.</param>
    /// <returns>The message to be broadcast of type <typeparamref name="T"/>.</returns>
    protected virtual T HandleMessageBasedOnObserverLogLevelAndMessageLogLevel(T message, LogLevel messageLevel, LogLevel observerLevel) => message;

    /// <summary>
    /// Allow special handling by the <see cref="Broadcaster{T}"/> class of the message of type <typeparamref name="T"/> before it is sent based on the <see cref="LogLevel"/> of the message.
    /// </summary>
    /// <param name="message">A message of type <typeparamref name="T"/> to modify based on broadcaster-specific parameters.</param>
    /// <param name="l">The log level of this message.</param>
    /// <returns>The message to be broadcast of type <typeparamref name="T"/>.</returns>
    protected virtual T HandleMessageBasedOnMessageLogLevel(T message, LogLevel l) => message;

    /// <summary>
    /// Allow special handling by the <see cref="Broadcaster{T}"/> class of the message of type <typeparamref name="T"/> before it is sent based on the <see cref="LogLevel"/> of the observer the message is being sent to.
    /// </summary>
    /// <param name="message">A message of type <typeparamref name="T"/> to modify based on broadcaster-specific parameters.</param>
    /// <param name="l">The log level of the observer the message is being sent to.</param>
    /// <returns>The message to be broadcast of type <typeparamref name="T"/>.</returns>
    protected virtual T HandleMessageBasedOnObserverLogLevel(T message, LogLevel l) => message;

    /// <summary>
    /// Broadcast an error (of type <see cref="Exception"/>) to all observers of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="error">An error of type <see cref="Exception"/>.</param>
    public void BroadcastError(Exception error)
    {
        foreach (var observer in Observers.Where(observer => observer.MessageMatcher(ErrorLevel)))
        {
            observer.OnError(error);
        }
    }

    /// <summary>
    /// Allow special handling by the <see cref="Broadcaster{T}"/> class of the error (of type <see cref="Exception"/>) before it is sent.
    /// </summary>
    /// <param name="error">An <see cref="Exception"/> to modify based on broadcaster-specific parameters.</param>
    /// <param name="l">The log level of this message.</param>
    /// <returns>The message to be broadcast of type <see cref="Exception"/>.</returns>
    protected virtual Exception ErrorHandler(Exception error, LogLevel l) => error;

    /// <summary>
    /// The default <see cref="LogLevel"/> to use for error messages is <see cref="LogLevel.Error"/>.
    /// </summary>
    protected virtual LogLevel ErrorLevel => LogLevel.Error;

    /// <summary>
    /// Broadcast a completion message to all observers of type <typeparamref name="T"/>.
    /// </summary>
    public void BroadcastCompletion()
    {
        foreach (var observer in Observers.Where(observer => observer.MessageMatcher(CompletionLevel)))
        {
            observer.OnCompleted();
        }
    }

    /// <summary>
    /// The default <see cref="LogLevel"/> to use for completion messages is <see cref="LogLevel.Info"/>.
    /// </summary>
    protected virtual LogLevel CompletionLevel => LogLevel.Info;

    /// <summary>
    /// Subscribe an observer of type <typeparamref name="T"/> to the broadcaster of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="observer">An observer of type <typeparamref name="T"/> which must match the broadcaster of type <typeparamref name="T"/>.</param>
    /// <returns>An <see cref="IDisposable"/> disposer that, when called, unsubscribes the observer from this broadcaster.</returns>
    public IDisposable Subscribe(Observation.IObserver<T> observer)
    {
        Observers.Add(observer);

        AdditionalSubscriptionInstructions();

        IDisposable? additionalUnsubscriptionInstructions = observer as IDisposable ?? null;

        return new Unsubscriber(observer, Observers, additionalUnsubscriptionInstructions);
    }

    protected virtual void AdditionalSubscriptionInstructions() { }

    private class Unsubscriber : IDisposable
    {
        private Observation.IObserver<T> _observer;
        private List<Observation.IObserver<T>> _observers;
        private IDisposable? _additional;

        public Unsubscriber(Observation.IObserver<T> observer, List<Observation.IObserver<T>> observers, IDisposable? additional)
        {
            _observer = observer;
            _observers = observers;
            _additional = additional;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _additional?.Dispose();
                _observers.Remove(_observer);
            }
        }
    }

    public abstract object Clone();
}
