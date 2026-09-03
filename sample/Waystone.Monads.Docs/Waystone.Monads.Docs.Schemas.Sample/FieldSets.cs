namespace Waystone.Monads.Docs.Schemas.Sample;

using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>packages/schemas/field-sets.md</summary>
internal static class FieldSetsPage
{
    #region schema-field-sets-cross-field
    // A rule about two fields at once, so it takes the whole subject.
    public static readonly Schema<PartyDto, PartyDto> Chronology =
        Schema.For<PartyDto>()
              .Check(
                   party => party.Disbanded is null
                         || party.Formed is null
                         || party.Disbanded > party.Formed,
                   ViolationCode.Conflicting,
                   "A party cannot disband before it forms.");
    #endregion

    internal static Field[] EveryKindOfField(PartyDto subject)
    {
        #region schema-field-sets-required
        Field<string> name =
            Schema.Required(subject.Name, Schema.Text.Trim().NotEmpty());
        #endregion

        #region schema-field-sets-required-message
        Field<string> title =
            Schema.Required(subject.Title, Guild.Title, "Every party needs {Path}.");
        #endregion

        #region schema-field-sets-optional
        Field<Option<int>> size =
            Schema.Optional(subject.Size, Schema.Number.Int32.AtLeast(1).AtMost(6));
        #endregion

        #region schema-field-sets-forbidden
        Field<Checked> legacy =
            Schema.Forbidden(subject.LegacyId, "Do not send {Path}.");
        #endregion

        #region schema-field-sets-extend
        Field<Checked> chronology = Schema.Extend(subject, Chronology);
        #endregion

        #region schema-field-sets-as-checked
        // A field the caller must send correctly but that the parsed type has no
        // place for. AsChecked runs its rules, drops its value, and keeps its own
        // path, so it goes to Refine rather than spending a slot in Into.
        Field<Checked> confirmation =
            Schema.Required(subject.ConfirmEmail, Schema.Text.Email())
                  .AsChecked();
        #endregion

        return [name, title, size, legacy, chronology, confirmation];
    }
}

public sealed record LeaderDto(string? Name, string? Email);

public sealed class Leader
{
    internal Leader(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; }

    public string Email { get; }
}

public partial class LeaderSchema : SchemaConfig<LeaderDto, Leader>
{
    protected override Result<Leader, SchemaViolation> Configure(
        LeaderDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Name, Schema.Text.Trim().NotEmpty()),
                   Schema.Required(subject.Email, Guild.Email))
              .Into((name, email) => new Leader(name, email));
}

public sealed record PartyDto(
    string? Name,
    string? Title,
    LeaderDto? Leader,
    int? Size,
    string? LegacyId,
    DateTimeOffset? Formed,
    DateTimeOffset? Disbanded,
    string? ConfirmEmail);

public sealed class Party
{
    internal Party(string name, Leader leader, Option<int> size)
    {
        Name = name;
        Leader = leader;
        Size = size;
    }

    public string Name { get; }

    public Leader Leader { get; }

    public Option<int> Size { get; }
}

#region schema-field-sets-the-whole-thing
public partial class PartySchema : SchemaConfig<PartyDto, Party>
{
    protected override Result<Party, SchemaViolation> Configure(PartyDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Name, Schema.Text.Trim().NotEmpty()),

                   // A nested schema is just a schema. Its violations arrive under
                   // "leader", so a reader is told which one failed.
                   Schema.Required(subject.Leader, LeaderSchema.Instance),
                   Schema.Optional(subject.Size, Schema.Number.Int32.AtLeast(1)))

              // Refine takes fields that gate the parse without producing a value,
              // so the Into lambda keeps one parameter per field above and no
              // discards.
              .Refine(
                   Schema.Forbidden(subject.LegacyId, "Do not send {Path}."),
                   Schema.Extend(subject, FieldSetsPage.Chronology))
              .Into((name, leader, size) => new Party(name, leader, size));
}
#endregion

public sealed record ConsentDto(string? Terms, string? Privacy);

#region schema-field-sets-checked
// A schema that only gates finishes with Checked. There is nothing to construct,
// so there is no lambda and nothing to name.
public partial class ConsentSchema : SchemaConfig<ConsentDto, Checked>
{
    protected override Result<Checked, SchemaViolation> Configure(
        ConsentDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Terms, Schema.Text.NotEmpty()),
                   Schema.Required(subject.Privacy, Schema.Text.NotEmpty()))
              .Checked();
}
#endregion
