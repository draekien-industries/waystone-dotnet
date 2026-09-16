# Measuring a major break

Compile code written against the outgoing major against the working tree, and read the
compiler's diagnostics. A claim about what breaks a consumer is settled by compiling,
not by reading a diff.

This describes the harness that measured v6 against v7. It was deleted once v7 shipped —
an inventory of a migration nobody is running decays silently, and its rows stay
plausible while they go wrong. Rebuild it at the start of the next major and delete it
again at the end.

## Two projects, not one

**A declaration-phase error masks every body-phase error in the same compilation.**
Measured: a bad `using static` reports its own `CS0234`, and a `CS0029` in a method body
two files away is never reported at all. The compiler does not get as far as binding
bodies.

So one project holds the call sites that break *before* bodies are bound, and another
holds everything else. Neither references the other, so a break in one cannot suppress
the other. Left in one project, a single declaration-phase change reduces the whole
inventory to one row and nothing says so.

Put a call site in the declarations project when it breaks at a `using` directive, a
base type, a member signature, or an `override`. Everything else goes in the other.

## Keep them out of the build

List both in `Waystone.Net.slnx` with `<Build Project="false" />`, so `dotnet build` at
the root skips them while an IDE still loads them and builds them on demand. CI builds
the same solution, so that covers CI too.

`dotnet sln add` does not write that element back. Re-adding a project with it and
forgetting the element fails every workflow in the repository on the first breaking
layer.

Set `TreatWarningsAsErrors=false` in both project files. It is belt and braces rather
than a workaround: `TreatWarningsAsErrors` lives in `src/Directory.Build.props` and
there is no `Directory.Build.props` at the repository root, so nothing outside `src/**`
inherits it.

## Commit the inventory on the layer that changed the surface

The diff is the attribution: this break arrived with this change. Rebuilt only at the
end of a stack, the file proves the final state and attributes nothing.

**Re-run the capture whenever a type or member leaves the public surface**, and read the
count table rather than trusting that the row you came for is current. A missed
re-capture is worse than no file. The layer that collapsed the per-family extension
classes did not re-run it, and the inventory went on naming `AndThenExtensions` and
`IsSomeAndExtensions` for a further nineteen layers — five rows attributed to `CS0411`
and `CS1739` that had become `CS0103` and `CS0234`, every one still reading like a
measurement.

## The capture script

Builds both projects, normalises the diagnostics to repository-relative paths, and
writes a count-by-code table and a row per diagnostic. It exits `0` even when the build
fails, because a failing build is the measurement.

```powershell
#!/usr/bin/env pwsh
# Builds both previous-major sample projects against the working tree and writes
# their diagnostics to breaks.txt. That file is the artefact: commit it on the
# layer that changed the surface, so the diff shows which break arrived with
# which change.

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$repo = (Resolve-Path (Join-Path $here '..' '..')).Path
$output = Join-Path $here 'breaks.txt'
$separator = [IO.Path]::DirectorySeparatorChar

$projects = @(
    Join-Path $here 'Waystone.Monads.PreviousMajor.Sample.csproj'
    Join-Path $repo 'sample' 'Waystone.Monads.PreviousMajor.Declarations.Sample' 'Waystone.Monads.PreviousMajor.Declarations.Sample.csproj'
)

function Get-RepoRelativePath([string] $path) {
    $path.Replace("$repo$separator", '').Replace('\', '/')
}

$pattern = '^(?<file>.+?)\((?<line>\d+),(?<col>\d+)\): (?<kind>error|warning) (?<code>[A-Z]+\d+): (?<text>.+?)( \[.+\])?$'

$rows = foreach ($project in $projects) {
    & dotnet build $project -c Release --nologo -v:m 2>&1 |
        ForEach-Object { [string]$_ } |
        ForEach-Object {
            $match = [regex]::Match($_.Trim(), $pattern)
            if (-not $match.Success) { return }
            [pscustomobject]@{
                Project = Get-RepoRelativePath $project
                File    = Get-RepoRelativePath $match.Groups['file'].Value
                Line    = [int]$match.Groups['line'].Value
                Kind    = $match.Groups['kind'].Value
                Code    = $match.Groups['code'].Value
                Text    = $match.Groups['text'].Value
            }
        }
}

# MSBuild prints each diagnostic once per target that surfaced it, so the same
# code at the same position is one diagnostic, not two.
$rows = @($rows | Sort-Object Code, File, Line, Text -Unique)

$report = [Collections.Generic.List[string]]::new()
$report.Add('# v7 break inventory')
$report.Add('')
$report.Add('Written by `capture-breaks.ps1`. Every row is a diagnostic a consumer')
$report.Add('on the previous major gets from the surface in the working tree.')
$report.Add('')

if ($rows.Count -eq 0) {
    $report.Add('No diagnostics. The previous major still compiles against this tree.')
} else {
    $report.Add('| Code | Kind | Count |')
    $report.Add('| --- | --- | --- |')
    foreach ($group in ($rows | Group-Object Code, Kind | Sort-Object Name)) {
        $first = $group.Group[0]
        $report.Add("| $($first.Code) | $($first.Kind) | $($group.Count) |")
    }
    $report.Add('')
    $report.Add('| Code | File | Line | Message |')
    $report.Add('| --- | --- | --- | --- |')
    foreach ($row in $rows) {
        $report.Add("| $($row.Code) | $($row.File) | $($row.Line) | $($row.Text) |")
    }
}

Set-Content -Path $output -Value $report
Write-Output "$($rows.Count) diagnostics written to $(Get-RepoRelativePath $output)"

# A failed build is the measurement, not a failure of this script.
exit 0
```

Change the `# v7 break inventory` heading and the two project paths to the new major's.

## What it cannot measure

A behaviour change that emits no diagnostic. v7 changed what `MonadOptionsScope` does
when disposed out of order; nothing counts that, and the migration guide had to state it
in prose.

Adding an extension method is the case worth writing a call site for even when the
issue claims no break. It is non-breaking only while nobody already has that name in
scope, so declare a consumer's own `Select` and `Where` and call them — the collision
count then stops being zero by assumption.

## Quote nothing from these projects

They are excluded from the root build, so a break in them is not caught, and
`tools/Waystone.DocSnippets` reads all of `sample/`. A page quoting a project that does
not build publishes code nothing compiles.
