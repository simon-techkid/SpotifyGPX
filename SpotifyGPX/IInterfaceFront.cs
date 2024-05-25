// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

/// <summary>
/// Provides access to underlying properties of an object implementing <typeparamref name="TInterface"/>.
/// </summary>
/// <typeparam name="TInterface">The required interface.</typeparam>
public interface IInterfaceFront<TInterface>
{
    /// <summary>
    /// Get a property value from this <typeparamref name="TInterface"/>, assuming it is of (and implemented by) the specified type <typeparamref name="TImplementer"/>.
    /// </summary>
    /// <typeparam name="TImplementer">The object of type <typeparamref name="TImplementer"/> (which must implement <typeparamref name="TInterface"/>) to get the property from.</typeparam>
    /// <param name="propertySelector">The property of object type <typeparamref name="TImplementer"/> to return.</param>
    /// <returns>If this <typeparamref name="TInterface"/> is <typeparamref name="TImplementer"/>, the selected object <see cref="object"/>. Otherwise, <see langword="null"/>.</returns>
    public object? GetPropertyValue<TImplementer>(Func<TImplementer, object?> propertySelector)
    {
        if (this is TImplementer implementer)
        {
            return propertySelector(implementer);
        }

        return null;
    }

    /// <summary>
    /// Get the object of type <typeparamref name="TImplementer"/> implementing <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TImplementer">The object of type <typeparamref name="TImplementer"/> to return.</typeparam>
    /// <returns>If <typeparamref name="TInterface"/> is <typeparamref name="TImplementer"/>, the object of type <typeparamref name="TImplementer"/> that implements <typeparamref name="TInterface"/>.</returns>
    public TImplementer GetObject<TImplementer>()
    {
        if (this is TImplementer implementer)
        {
            return implementer;
        }

        throw new Exception($"Cannot cast {GetType().FullName} to {typeof(TImplementer).FullName}.");
        //return default;
    }

}
