// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace GameKit.Mediator.Abstractions
{
    public interface ICommand : ICommand<Unit>
    {
    }

    public interface ICommand<out TResult> : VitalRouter.ICommand
    {
    }
}
