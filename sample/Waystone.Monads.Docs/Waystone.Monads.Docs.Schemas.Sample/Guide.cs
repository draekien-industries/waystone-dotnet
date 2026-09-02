namespace Waystone.Monads.Docs.Schemas.Sample;

using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

public sealed record RegistrationDto(
    string? Email,
    string? DisplayName,
    int? Age,
    bool? AcceptedTerms);

#region schemas-guide-the-type
// The constructor is not public, so the only way to hold a Registration is to
// have passed the schema that builds one.
public sealed class Registration
{
    internal Registration(string email, string displayName, Option<int> age)
    {
        Email = email;
        DisplayName = displayName;
        Age = age;
    }

    public string Email { get; }

    public string DisplayName { get; }

    public Option<int> Age { get; }
}
#endregion

#region schemas-guide-the-checks
// Each one is a value. Name it, and reuse it wherever that shape turns up.
public static class Registrations
{
    public static readonly Schema<string, string> Email =
        Schema.Text.Trim().Email();

    public static readonly Schema<string, string> DisplayName =
        Schema.Text.Trim().LengthBetween(2, 40);

    public static readonly Schema<int, int> Age =
        Schema.Number.Int32.Between(13, 130);

    // A rule over the whole subject, because accepting the terms gates the
    // registration without contributing anything to it.
    public static readonly Schema<RegistrationDto, RegistrationDto> Terms =
        Schema.For<RegistrationDto>()
              .Check(
                   subject => subject.AcceptedTerms == true,
                   ViolationCode.NotAllowed,
                   "You have to accept the terms.");
}
#endregion

#region schemas-guide-the-schema
public partial class RegistrationSchema
    : SchemaConfig<RegistrationDto, Registration>
{
    protected override Result<Registration, SchemaViolation> Configure(
        RegistrationDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Email, Registrations.Email),
                   Schema.Required(subject.DisplayName, Registrations.DisplayName),
                   Schema.Optional(subject.Age, Registrations.Age))
              .Refine(Schema.Extend(subject, Registrations.Terms))
              .Into(
                   (email, name, age) => new Registration(email, name, age));
}
#endregion

/// <summary>guides/schemas.md</summary>
internal static class GuidePage
{
    #region schemas-guide-the-usual-way
    // Nothing here is wrong on its own, and this is how most of us write it.
    public static Registration? Register(
        RegistrationDto dto,
        List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            problems.Add("Email is required.");
        }

        if (dto.DisplayName is null)
        {
            problems.Add("Display name is required.");
        }

        if (dto.AcceptedTerms != true)
        {
            problems.Add("You have to accept the terms.");
        }

        if (problems.Count > 0)
        {
            return null;
        }

        // Look at the null-forgiving operators. The compiler has no idea those
        // checks ran, so nothing stops this line moving above them, or being
        // written against a field nobody checked.
        return new Registration(
            dto.Email!,
            dto.DisplayName!,
            dto.Age is null ? Option.None<int>() : Option.Some(dto.Age.Value));
    }
    #endregion

    internal static IResponse Handle(RegistrationDto body)
    {
        #region schemas-guide-at-the-edge
        // One call. Either you are holding a Registration, or you are holding
        // every reason you are not.
        return RegistrationSchema.Instance
                                 .Parse(body)
                                 .Match<IResponse>(
                                      registration => new Created(registration),
                                      violation => new BadRequest(
                                          violation.ToDictionary()));
        #endregion
    }
}

public interface IResponse;

public sealed record Created(Registration Registration) : IResponse;

public sealed record BadRequest(IDictionary<string, string[]> Problems)
    : IResponse;
