using Waystone.Monads.Results.Errors;

namespace Questing;

/// <summary>source-generation/error-code-catalogs.md</summary>
[ErrorCodeCatalog]
public enum QuestErrorCode
{
    NotFound,
    AlreadyCompleted,
}

internal static class ErrorCodeCatalogsPage
{
    internal static void WhatYouGet()
    {
        // The code as a compile-time constant.
        _ = QuestErrorCodeCatalog.Names.NotFound; // "QuestErrorCode.NotFound"

        // The code as an ErrorCode.
        _ = QuestErrorCodeCatalog.Codes.NotFound; // ErrorCode { Value = "QuestErrorCode.NotFound" }

        // An Error carrying that code.
        _ = QuestErrorCodeCatalog.Errors.NotFound("no quest on the board with that name");
    }

    internal static void FromAValueYouAlreadyHave(QuestErrorCode errorCode)
    {
        string asName = errorCode.ToErrorCodeName();
        ErrorCode asErrorCode = errorCode.ToErrorCode();
        Error asError = errorCode.ToError("no quest on the board with that name");

        _ = (asName, asErrorCode, asError);
    }

    internal static string AValueThatIsNotADeclaredMember() =>
        ((QuestErrorCode)99).ToErrorCodeName(); // "QuestErrorCode.99"
}
