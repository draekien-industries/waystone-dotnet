namespace Waystone.Monads.Hosting.Sample;

using Results.Errors;

internal static class Report
{
    internal static void Heading(string title)
    {
        Console.WriteLine($"-- {title} --");
    }

    internal static void OptionsInEffect(string label)
    {
        var blank = new Error(new ErrorCode(" "), " ");

        Console.WriteLine($"  {label}");
        Console.WriteLine($"    fallback code   : {blank.Code}");
        Console.WriteLine($"    fallback message: {blank.Message}");
    }
}
