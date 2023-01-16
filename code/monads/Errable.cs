using System;
namespace Game.Monads;

// TODO(srp): Add line and column error attribute to go with each errable
// TODO(srp): Bind appends errors

public struct Errable<T>
{
    private readonly T value;
    private readonly bool isFine;
    private readonly string errorMessage;

    private Errable(T value, string errorMessage = "")
    {
        this.value = value;
        this.errorMessage = errorMessage;

        bool noError = errorMessage is null || errorMessage == "";
        bool hasValue = value is not null;
        isFine = noError && hasValue;
    }

    /// Implicit conversion of T to Errable<T>
    public static implicit operator Errable<T>(T value)
    {
        if (value is null)
            return new Errable<T>(value, "[ERROR]: Value is null");

        return new Errable<T>(value);
    }

    /// "Throws" an error
    public static Errable<T> Err(string errorMessage)
    {
        if (errorMessage is null || errorMessage == "")
        {
            return new Errable<T>(default, "[ERROR]: Unknown error");
        }

        return new Errable<T>(default, errorMessage);
    }

    public string GetErrLog()
	{
        return errorMessage;
	}

    public TResult Match<TResult>(TResult good, TResult error)
    {
        if (isFine)
            return good;

        return error;
    }

    public TResult Match<TResult>(Func<T, TResult> good, Func<TResult> error)
    {
        if (isFine)
            return good(value);

        return error();
    }

    public void Match(Action<T> good, Action error)
    {
        if (isFine)
            good(value);
        else
            error();
    }

    public Errable<ToType> NonErrableMap<ToType>(Func<T, ToType> mapping, string prepend = "")
    {
        if (isFine)
            return new Errable<ToType>(mapping(value));

        return Errable<ToType>.Err(prepend + errorMessage);
    }

    public Errable<TResult> ErrableMap<TResult>(Func<T, Errable<TResult>> mapping, string prepend = "")
    {
        if (!isFine)
            return Errable<TResult>.Err(prepend + errorMessage);

        return mapping(value);
    }

    public static Errable<TResult> 
    ErrableBiMap<U, V, TResult>(Errable<U> first, Errable<V> second, 
            Func<U, V, Errable<TResult>> mapping, string prepend = "")
    {
        if (!first.isFine || !second.isFine)
            return Errable<TResult>.Err(prepend + (first.errorMessage ?? "") + (second.errorMessage ?? ""));

        return mapping(first.value, second.value);
    }
}
