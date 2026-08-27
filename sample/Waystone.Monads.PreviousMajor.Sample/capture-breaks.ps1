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
