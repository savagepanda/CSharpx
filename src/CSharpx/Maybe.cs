﻿//Use project level define(s) when referencing with Paket.
//#define CSX_MAYBE_INTERNAL // Uncomment this to set visibility to internal.
//#define CSX_REM_EITHER_FUNC // Uncomment this to remove dependency to Either.cs.

using System;

namespace CSharpx
{
    /// <summary>
    /// Discriminator for <see cref="CSharpx.Maybe"/>.
    /// </summary>
#if !CSX_MAYBE_INTERNAL
    public
#endif
    enum MaybeType { Just, Nothing }

    /// <summary>
    /// The Maybe type models an optional value. A value of type Maybe a either contains a value of type a (represented as Just a),
    /// or it is empty (represented as Nothing). 
    /// </summary>
#if !CSX_MAYBE_INTERNAL
    public
#endif
    abstract class Maybe<T>
    {
        private readonly MaybeType tag;

        protected Maybe(MaybeType tag)
        {
            this.tag = tag;
        }

        /// <summary>
        /// Type discriminator.
        /// </summary>
        public MaybeType Tag
        {
            get { return tag; }
        }

        /// <summary>
        /// Matches an empty value returning <c>true</c>.
        /// </summary>
        public bool MatchNothing()
        {
            return Tag == MaybeType.Nothing;
        }

        /// <summary>
        /// Matches a value returning <c>true</c> and value itself via output parameter.
        /// </summary>
        public bool MatchJust(out T value)
        {
            value = Tag == MaybeType.Just
                ? ((Just<T>)this).Value
                : default(T);
            return Tag == MaybeType.Just;
        }
    }

    /// <summary>
    /// Models a <see cref="CSharpx.Maybe"/> when in empty state.
    /// </summary>
#if !CSX_MAYBE_INTERNAL
    public
#endif
    sealed class Nothing<T> : Maybe<T>
    {
        internal Nothing() : base(MaybeType.Nothing) { }
    }

    /// <summary>
    /// Models a <see cref="CSharpx.Maybe"/> when contains a value.
    /// </summary>
#if !CSX_MAYBE_INTERNAL
    public
#endif
    sealed class Just<T> : Maybe<T>
    {
        private readonly T value;

        internal Just(T value)
            : base(MaybeType.Just)
        {
            this.value = value;
        }

        /// <summary>
        /// The wrapped value.
        /// </summary>
        public T Value
        {
            get { return value; }
        }
    }

    /// <summary>
    /// Provides static methods for manipulating <see cref="CSharpx.Maybe"/>.
    /// </summary>
#if !CSX_MAYBE_INTERNAL
    public
#endif
    static class Maybe
    {
        /// <summary>
        /// Builds the empty case of <see cref="CSharpx.Maybe"/>.
        /// </summary>
        public static Maybe<T> Nothing<T>()
        {
            return new Nothing<T>();
        }

        /// <summary>
        /// Builds the case when <see cref="CSharpx.Maybe"/> contains a value.
        /// </summary>
        public static Just<T> Just<T>(T value)
        {
            return new Just<T>(value);
        }

        /// <summary>
        /// If both maybes contain a value, it merges them into a maybe with a tupled value.
        /// </summary>
        public static Maybe<Tuple<T1, T2>> Merge<T1, T2>(Maybe<T1> first, Maybe<T2> second)
        {
            T1 value1;
            T2 value2;
            if (first.MatchJust(out value1) && second.MatchJust(out value2))
            {
                return Maybe.Just(Tuple.Create(value1, value2));
            }
            return Maybe.Nothing<Tuple<T1, T2>>();
        }

#if !CSX_REM_EITHER_FUNC
        /// <summary>
        /// Maps Choice 1Of2 to Some value, otherwise None.
        /// </summary>
        public static Maybe<T1> OfEither<T1, T2>(Either<T1, T2> either)
        {
            if (either.Tag == Either2Type.Either1Of2)
            {
                return Maybe.Just(((Either1Of2<T1, T2>)either).Value);
            }
            return Maybe.Nothing<T1>();
        }
#endif
    }

    /// <summary>
    /// Provides convenience extension methods for <see cref="CSharpx.Maybe"/>.
    /// </summary>
#if !CSX_MAYBE_INTERNAL
    public
#endif
    static class MaybeExtensions
    {
        /// <summary>
        /// Equivalent to Return or Unit operation.
        /// Builds a <see cref="CSharpx.Just{T}"/> value in case <paramref name="value"/> is different from its default.
        /// </summary>
        public static Maybe<T> ToMaybe<T>(this T value)
        {
            return Equals(value, default(T)) ? Maybe.Nothing<T>() : Maybe.Just(value);
        }

        /// <summary>
        /// Invokes a function on an optional value that itself yields a maybe.
        /// </summary>
        public static Maybe<T2> Bind<T1, T2>(this Maybe<T1> maybe, Func<T1, Maybe<T2>> func)
        {
            T1 value1;
            return maybe.MatchJust(out value1)
                ? func(value1)
                : Maybe.Nothing<T2>();
        }

        /// <summary>
        /// Transforms an option value by using a specified mapping function.
        /// </summary>
        public static Maybe<T2> Map<T1, T2>(this Maybe<T1> maybe, Func<T1, T2> func)
        {
            T1 value1;
            return maybe.MatchJust(out value1)
                ? Maybe.Just(func(value1))
                : Maybe.Nothing<T2>();
        }

        /// <summary>
        /// If contains a values executes a mapping function over it, othervalue returns <paramref name="noneValue"/>.
        /// </summary>
        public static T2 MapMaybeOrDefault<T1, T2>(this Maybe<T1> maybe, Func<T1, T2> func, T2 noneValue)
        {
            T1 value1;
            return maybe.MatchJust(out value1)
                ? func(value1)
                : noneValue;
        }

        /// <summary>
        /// If contans a value executes an <see cref="System.Action{T}"/> delegate over it.
        /// </summary>
        public static void Do<T>(this Maybe<T> maybe, Action<T> action)
        {
            T value;
            if (maybe.MatchJust(out value))
            {
                action(value);
            }
        }

        /// <summary>
        /// Map operation compatible with Linq.
        /// </summary>
        public static Maybe<TResult> Select<TSource, TResult>(
            this Maybe<TSource> maybe, Func<TSource, TResult> selector)
        {
            return maybe.Map(selector);
        }

        /// <summary>
        /// Bind operation compatible with Linq.
        /// </summary>
        public static Maybe<TResult> SelectMany<TSource, TValue, TResult>(
            this Maybe<TSource> maybe,
            Func<TSource, Maybe<TValue>> valueSelector,
            Func<TSource, TValue, TResult> resultSelector)
        {
            return maybe.Bind(
                sourceValue => valueSelector(sourceValue)
                    .Map(
                        resultValue => resultSelector(sourceValue, resultValue)));
        }

        /// <summary>
        /// Extracts the element out of a <see cref="CSharpx.Just{T}"/> and returns a default value if its argument is <see cref="CSharpx.Nothing{T}"/>.
        /// </summary>
        public static T FromJust<T>(this Maybe<T> maybe)
        {
            T value;
            if (maybe.MatchJust(out value))
            {
                return value;
            }
            return default(T);
        }

        /// <summary>
        /// Extracts the element out of a <see cref="CSharpx.Just{T}"/> and throws an error if its argument is <see cref="CSharpx.Nothing{T}"/>.
        /// </summary>
        public static T FromJustOrFail<T>(this Maybe<T> maybe, Exception exceptionToThrow = null)
        {
            T value;
            if (maybe.MatchJust(out value))
            {
                return value;
            }
            throw exceptionToThrow ?? new ArgumentException("Value empty.");
        }

        /// <summary>
        /// Returns <c>true</c> iffits argument is of the form <see cref="CSharpx.Nothing{T}"/>.
        /// </summary>
        public static bool IsNothing<T>(this Maybe<T> maybe)
        {
            return maybe.Tag == MaybeType.Nothing;
        }

        /// <summary>
        /// Returns <c>true</c> iffits argument is of the form <see cref="CSharpx.Just{T}"/>.
        /// </summary>
        public static bool IsJust<T>(this Maybe<T> maybe)
        {
            return maybe.Tag == MaybeType.Just;
        }

        /// <summary>
        /// Provides pattern matching using <see cref="System.Action"/> delegates.
        /// </summary>
        public static void Match<T>(this Maybe<T> maybe, Action<T> ifJust, Action ifNothing)
        {
            T value;
            if (maybe.MatchJust(out value))
            {
                ifJust(value);
                return;
            }
            ifNothing();
        }

        /// <summary>
        /// Provides pattern matching using <see cref="System.Action"/> delegates over maybe with tupled wrapped value.
        /// </summary>
        public static void Match<T1, T2>(this Maybe<Tuple<T1, T2>> maybe, Action<T1, T2> ifJust, Action ifNothing)
        {
            T1 value1;
            T2 value2;
            if (maybe.MatchJust(out value1, out value2))
            {
                ifJust(value1, value2);
                return;
            }
            ifNothing();
        }

        /// <summary>
        /// Matches a value returning <c>true</c> and tupled value itself via two output parameters.
        /// </summary>
        public static bool MatchJust<T1, T2>(this Maybe<Tuple<T1, T2>> maybe, out T1 value1, out T2 value2)
        {
            Tuple<T1, T2> value;
            if (maybe.MatchJust(out value))
            {
                value1 = value.Item1;
                value2 = value.Item2;
                return true;
            }
            value1 = default(T1);
            value2 = default(T2);
            return false;
        }

        /// <summary>
        /// If contans a value executes an <see cref="System.Action{T1, T2}"/> delegate over it.
        /// </summary>
        public static void Do<T1, T2>(this Maybe<Tuple<T1, T2>> maybe, Action<T1, T2> action)
        {
            T1 value1;
            T2 value2;
            if (maybe.MatchJust(out value1, out value2))
            {
                action(value1, value2);
            }
        }
    }
}