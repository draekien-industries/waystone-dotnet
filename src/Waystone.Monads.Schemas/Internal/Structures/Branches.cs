namespace Waystone.Monads.Schemas.Internal.Structures;

using System;

internal static class Branches
{
    internal static T[] Require<T>(T[] branches)
    {
        if (branches is null)
        {
            throw new ArgumentNullException(nameof(branches));
        }

        if (branches.Length == 0)
        {
            throw new ArgumentException(
                "A combinator needs at least one branch to run.",
                nameof(branches));
        }

        for (var index = 0; index < branches.Length; index++)
        {
            if (branches[index] is null)
            {
                throw new ArgumentException(
                    "A combinator branch cannot be null.",
                    nameof(branches));
            }
        }

        return branches;
    }
}
