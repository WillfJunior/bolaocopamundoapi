namespace BolaoCopaMundo.Application.Services;

public static class ScoringService
{
    public static int CalculatePoints(int predHome, int predAway, int actualHome, int actualAway)
    {
        if (predHome == actualHome && predAway == actualAway)
            return 3;

        if (GetOutcome(predHome, predAway) == GetOutcome(actualHome, actualAway))
            return 1;

        return 0;
    }

    private static int GetOutcome(int home, int away) =>
        home > away ? 1 : home < away ? -1 : 0;
}
