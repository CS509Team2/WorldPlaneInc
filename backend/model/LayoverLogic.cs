namespace model;

public static class LayoverLogic
{
    public const int MinimumLayoverMinutes = 90;
    public const int MaximumLayoverMinutes = 360;

    public static double GetLayoverMinutes(FlightSegment currentLeg, FlightSegment nextLeg)
    {
        return (nextLeg.DepartDateTime - currentLeg.ArriveDateTime).TotalMinutes;
    }

    public static bool IsValidConnection(FlightSegment currentLeg, FlightSegment nextLeg)
    {
        var layoverMinutes = GetLayoverMinutes(currentLeg, nextLeg);

        return layoverMinutes >= MinimumLayoverMinutes &&
               layoverMinutes <= MaximumLayoverMinutes;
    }
}