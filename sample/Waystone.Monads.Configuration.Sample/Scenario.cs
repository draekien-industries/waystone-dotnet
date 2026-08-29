namespace Waystone.Monads.Configuration.Sample;

using Results.Errors;

internal static class Scenario
{
    internal static void Heading(string title)
    {
        Console.WriteLine($"-- {title} --");
    }

    internal static void ReportOptionsInEffect(string label = "in effect")
    {
        var blank = new Error(new ErrorCode(" "), " ");

        Console.WriteLine($"  {label}");
        Console.WriteLine($"    fallback code     : {blank.Code}");
        Console.WriteLine($"    fallback message  : {blank.Message}");
        Console.WriteLine(
            $"    code for a timeout: {ErrorCode.FromException(new TimeoutException())}");
    }
}
