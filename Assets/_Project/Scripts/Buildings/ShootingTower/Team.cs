public static class TeamExtensions
{
    public static Team GetEnemyTeam(this Team team)
    {
        return team switch
        {
            Team.Blue => Team.Red,
            Team.Red => Team.Blue,
            _ => Team.Neutral
        };
    }
}

public enum Team
{

    Blue,
    Red,
    Neutral


}

