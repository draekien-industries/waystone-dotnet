using System.Text.RegularExpressions;
using Waystone.Monads.Configs;
using Waystone.Monads.Options;
using Waystone.Monads.Results.Errors;

// one time configuration of the library
MonadOptions.Configure(x =>
{
    x.UseExceptionLogger((ex, callerInfo) =>
    {
        Console.Error.WriteLine(
            $"[{callerInfo.MemberName}:{callerInfo.LineNumber}]: Exception Handled");

        Console.Error.WriteLine(callerInfo.ArgumentExpression);

        Console.Error.WriteLine(ex.Message);
        Console.Error.WriteLine(ex.StackTrace);
    });

    x.UseErrorCodeFactory(new MyErrorCodeFactory());
    x.UseFallbackErrorCode("S:Unknown");
    x.UseFallbackErrorMessage("Something went wrong.");
});

// a simple example of a pipe that validates an email address and extracts the
// domain
Option<string> emailInput = Option.Some("hello@example.com");

EmailDomain emailDomain = emailInput
   .Email()
   .EmailDomain()
   .UnwrapOr(EmailDomain.Unknown);

Console.WriteLine(emailDomain);

Option<string> notEmailInput = Option.Some("hello world!");

emailDomain = notEmailInput
   .Email()
   .EmailDomain()
   .UnwrapOr(EmailDomain.Unknown);

Console.WriteLine(emailDomain);

// some pipes for parsing emails
internal static partial class EmailPipes
{
    [GeneratedRegex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$")]
    private static partial Regex CreateEmailRegex();

    extension(Option<string> input)
    {
        public Option<Email> Email() =>
            input
               .Filter(x => !string.IsNullOrWhiteSpace(x))
               .Filter(x => CreateEmailRegex().IsMatch(x))
               .Map(x => new Email(x));
    }

    extension(Option<Email> email)
    {
        public Option<EmailDomain> EmailDomain() =>
            email.Map(x => new EmailDomain(
                x.Value[(x.Value.IndexOf('@') + 1)..]));
    }
}

internal record struct Email(string Value);

internal record struct EmailDomain(string Value)
{
    public static readonly EmailDomain Unknown = new("[unknown]");
}

internal class MyErrorCodeFactory : ErrorCodeFactory
{
    /// <inheritdoc />
    public override ErrorCode FromEnum(Enum @enum) =>
        new($"S:{base.FromEnum(@enum)}");

    /// <inheritdoc />
    public override ErrorCode FromException(Exception exception) =>
        new($"S:{base.FromException(exception)}");
}
