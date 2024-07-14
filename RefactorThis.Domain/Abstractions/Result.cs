﻿using System;

namespace RefactorThis.Domain.Abstractions
{
    public class Result
    {
        public Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException();
            }

            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException();
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        public static Result Success() => new Result(true, Error.None);

        public static Result Failure(Error error) => new Result(false, error);

        public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value, true, Error.None);

        public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(default(TValue), false, error);

        public static Result<TValue> Create<TValue>(TValue value) =>
            value != null ? Success(value) : Failure<TValue>(Error.NullValue);
    }

    public sealed class Result<TValue> : Result
    {
        private readonly TValue _value;

        public Result(TValue value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public TValue Value => IsSuccess
            ? _value
            : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

        public static implicit operator Result<TValue>(TValue value) => Create(value);
    }
}
