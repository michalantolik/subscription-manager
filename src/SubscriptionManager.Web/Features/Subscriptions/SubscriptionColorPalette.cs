namespace SubscriptionManager.Web.Features.Subscriptions;

/// <summary>
/// Provides colors for visually distinguishing subscriptions.
/// </summary>
public static class SubscriptionColorPalette
{
    private static readonly string[] Colors =
    [
        "#55ca79",
        "#63a0ff",
        "#ffad62",
        "#a980ff",
        "#ff7279",
        "#45c4b0",
        "#f2c94c",
        "#5c8df6",
        "#d77bf3",
        "#ff8f5c",
        "#7cc7e8",
        "#9fba6f"
    ];

    public static string GetColor(
        int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position));
        }

        return Colors[
            position % Colors.Length];
    }

    public static IReadOnlyList<string> GetAll()
    {
        return Colors;
    }
}
