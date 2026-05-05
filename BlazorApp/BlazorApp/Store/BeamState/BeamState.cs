using Fluxor;

namespace BlazorApp.Store.BeamState;

public enum LoadType { ConcentratedForce, ConcentratedMoment, DistributedForce }
public enum SupportType { SimplySupported, Cantilever, ProppedCantilever, FixedFixed }

public record BeamLoad(int Id, LoadType Type, double Magnitude, double Location, double EndLocation)
{
    public string Label => Type switch
    {
        LoadType.ConcentratedForce  => $"{Magnitude} kN @ {Location} m",
        LoadType.ConcentratedMoment => $"{Magnitude} kN·m @ {Location} m",
        LoadType.DistributedForce   => $"{Magnitude} kN/m  [{Location}–{EndLocation} m]",
        _                           => ""
    };
}

// Holds computed shear/moment sample arrays and the loading diagram SVG.
public record BeamResults(
    double[] ShearX,
    double[] ShearY,
    double[] MomentX,
    double[] MomentY,
    string LoadingDiagramSvg);

// [FeatureState] tells Fluxor to auto-register this record as a feature.
// The parameterless constructor supplies default values for the initial state.
[FeatureState]
public record BeamState
{
    public double Length { get; init; } = 10.0;
    public SupportType Support { get; init; } = SupportType.SimplySupported;
    public IReadOnlyList<BeamLoad> Loads { get; init; } = Array.Empty<BeamLoad>();
    public int NextLoadId { get; init; } = 1;
    public BeamResults? Results { get; init; }
    public string? ComputeError { get; init; }
}
