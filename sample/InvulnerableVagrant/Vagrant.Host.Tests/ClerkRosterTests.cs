namespace Vagrant.Host.Tests;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Vagrant.Host.Infrastructure;
using Vagrant.Staffing;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Xunit;

// These tests go under the HTTP surface because the behaviour they pin is a database
// one. The lost race below is a real DbUpdateConcurrencyException raised by SQLite
// refusing an UPDATE whose concurrency token no longer matches — not a fake, not a
// mocked repository, and not two threads whose interleaving the test would be guessing
// at. An interceptor on the losing context runs the winning claim to completion at the
// moment the loser is about to write, which is the same order two racing requests reach
// in and is the same order every run.
public sealed class ClerkRosterTests : IAsyncDisposable
{
    private static readonly DateTimeOffset Noon =
        new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);

    private static readonly FakeTimeProvider Clock = new(Noon);

    private readonly string _file = Path.Combine(
        Path.GetTempPath(),
        $"vagrant-roster-{Guid.CreateVersion7():N}.db");

    [Fact]
    public async Task A_lost_race_hands_the_errand_to_the_next_free_clerk()
    {
        await StaffTheCounterAsync();

        await using StaffingDbContext winning = new(Options());
        ClerkRoster ahead = new(winning, Clock);

        Errand theirs = Errand();
        Interleaved race = new(() => ahead.ClaimAsync(theirs, Ct));

        await using StaffingDbContext losing = new(Options(race));
        ClerkRoster behind = new(losing, Clock);

        Assignment mine = (await behind.ClaimAsync(Errand(), Ct)).ShouldBeOk();

        race.Ran.ShouldBe(1);
        race.Won.ShouldBeOk().Clerk.ShouldNotBe(mine.Clerk);
    }

    [Fact]
    public async Task Both_sides_of_a_lost_race_end_up_holding_their_own_errand()
    {
        await StaffTheCounterAsync();

        await using StaffingDbContext winning = new(Options());
        ClerkRoster ahead = new(winning, Clock);

        Errand theirs = Errand();
        Errand mine = Errand();
        Interleaved race = new(() => ahead.ClaimAsync(theirs, Ct));

        await using StaffingDbContext losing = new(Options(race));
        ClerkRoster behind = new(losing, Clock);

        (await behind.ClaimAsync(mine, Ct)).ShouldBeOk();

        await using StaffingDbContext reading = new(Options());
        IReadOnlyList<Clerk> onDuty =
            await new ClerkRoster(reading, Clock).OnDutyAsync(Ct);

        // An empty array for a free clerk rather than an unwrap with a fallback. A
        // default Errand would be indistinguishable from a real one, which is what WM2015
        // says and what would make this assertion lie.
        onDuty.SelectMany(clerk => clerk.Engagement.Match(
                   static assignment => new[] { assignment.Errand },
                   static () => []))
              .ShouldBe([theirs, mine], ignoreOrder: true);
    }

    [Fact]
    public async Task A_shop_whose_clerks_are_all_busy_refuses_by_name()
    {
        await StaffTheCounterAsync();

        await using StaffingDbContext db = new(Options());
        ClerkRoster roster = new(db, Clock);

        for (int taken = 0; taken < 4; taken++)
        {
            (await roster.ClaimAsync(Errand(), Ct)).ShouldBeOk();
        }

        Error refused = (await roster.ClaimAsync(Errand(), Ct)).ShouldBeErr();

        refused.Code.Value.ShouldBe("vagrant.staffing.no_clerk_free");
    }

    [Fact]
    public async Task A_claim_writes_the_hour_the_clock_reads()
    {
        await StaffTheCounterAsync();

        await using StaffingDbContext db = new(Options());

        Assignment taken =
            (await new ClerkRoster(db, Clock).ClaimAsync(Errand(), Ct)).ShouldBeOk();

        taken.Since.ShouldBe(Noon);
    }

    [Fact]
    public async Task Releasing_work_no_clerk_is_holding_is_none()
    {
        await StaffTheCounterAsync();

        await using StaffingDbContext db = new(Options());

        Option<Assignment> released = await new ClerkRoster(db, Clock)
           .ReleaseAsync(new ErrandSubject(Guid.CreateVersion7()), Ct);

        released.ShouldBeNone();
    }

    [Fact]
    public async Task A_released_clerk_takes_the_next_errand()
    {
        await StaffTheCounterAsync();

        await using StaffingDbContext db = new(Options());
        ClerkRoster roster = new(db, Clock);

        Errand first = Errand();

        for (int taken = 0; taken < 4; taken++)
        {
            (await roster.ClaimAsync(taken is 0 ? first : Errand(), Ct)).ShouldBeOk();
        }

        (await roster.ReleaseAsync(first.Subject, Ct)).ShouldBeSome();

        (await roster.ClaimAsync(Errand(), Ct)).ShouldBeOk();
    }

    [Fact]
    public async Task Claiming_an_errand_a_clerk_already_holds_names_that_clerk()
    {
        await StaffTheCounterAsync();

        await using StaffingDbContext db = new(Options());
        ClerkRoster roster = new(db, Clock);

        Errand cloak = Errand();

        Assignment first = (await roster.ClaimAsync(cloak, Ct)).ShouldBeOk();
        Assignment again = (await roster.ClaimAsync(cloak, Ct)).ShouldBeOk();

        again.Clerk.ShouldBe(first.Clerk);
        (await roster.OnDutyAsync(Ct)).Count(clerk => clerk.Engagement.IsSome)
                                      .ShouldBe(1);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();

        string directory = Path.GetDirectoryName(_file)!;
        string stem = Path.GetFileNameWithoutExtension(_file);

        foreach (string leftover in Directory.EnumerateFiles(directory, $"{stem}*"))
        {
            File.Delete(leftover);
        }

        await ValueTask.CompletedTask;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Errand Errand() => new(new ErrandSubject(Guid.CreateVersion7()));

    private DbContextOptions<StaffingDbContext> Options(
        params IInterceptor[] interceptors) =>
        new DbContextOptionsBuilder<StaffingDbContext>()
           .UseSqlite($"Data Source={_file}")
           .AddInterceptors(interceptors)
           .Options;

    private async Task StaffTheCounterAsync()
    {
        await using StaffingDbContext db = new(Options());

        await db.Database.EnsureCreatedAsync(Ct);
        (await ClerkSeed.OpenTheCounterAsync(db, Ct)).ShouldBe(4);
    }

    /// <remarks>
    /// Runs another claim to completion at the moment this context is about to write, so
    /// the write it then issues carries a concurrency token the database has already
    /// moved past. Once only: the retry that follows has to be allowed to land.
    /// </remarks>
    private sealed class Interleaved(Func<Task<Result<Assignment, Error>>> other)
        : SaveChangesInterceptor
    {
        public int Ran { get; private set; }

        public Result<Assignment, Error> Won { get; private set; } =
            Result.Err<Assignment, Error>(
                new Error(
                    new ErrorCode("test.not_run"),
                    "the interleaved claim never ran"));

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (Ran is 0)
            {
                Ran++;
                Won = await other().ConfigureAwait(false);
            }

            return result;
        }
    }
}
