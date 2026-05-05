using BlazorApp.Lib;
using BlazorApp.Lib.Functions.Singularity;
using Fluxor;

namespace BlazorApp.Store.BeamState;

// Reducers are pure static functions: (currentState, action) => newState.
// They must never mutate state — use C# `with` expressions to produce a new record.
// Synchronous computation is fine here because reducers are just functions.
public static class BeamReducers
{
    [ReducerMethod]
    public static BeamState OnSetLength(BeamState state, SetBeamLengthAction action) =>
        Recompute(state with { Length = action.Length });

    [ReducerMethod]
    public static BeamState OnSetSupport(BeamState state, SetSupportTypeAction action) =>
        Recompute(state with { Support = action.Support });

    [ReducerMethod]
    public static BeamState OnAddLoad(BeamState state, AddLoadAction action)
    {
        var load = new BeamLoad(state.NextLoadId, action.Type, action.Magnitude, action.Location, action.EndLocation);
        var newLoads = state.Loads.Append(load).ToList();
        return Recompute(state with { Loads = newLoads, NextLoadId = state.NextLoadId + 1 });
    }

    [ReducerMethod]
    public static BeamState OnRemoveLoad(BeamState state, RemoveLoadAction action)
    {
        var newLoads = state.Loads.Where(l => l.Id != action.Id).ToList();
        return Recompute(state with { Loads = newLoads });
    }

    // ── Computation helper ────────────────────────────────────────────────────
    // Called at the end of every reducer that changes beam configuration.
    // Returns a new state with Results (or ComputeError) filled in.

    static BeamState Recompute(BeamState state)
    {
        if (!state.Loads.Any())
            return state with { Results = null, ComputeError = null };

        var (tStart, rStart, tEnd, rEnd) = GetRestraints(state.Support);
        var beam = new Beam
        {
            Length           = state.Length,
            TranslationStart = tStart,
            RotationStart    = rStart,
            TranslationEnd   = tEnd,
            RotationEnd      = rEnd,
        };

        foreach (var load in state.Loads)
        {
            switch (load.Type)
            {
                case LoadType.ConcentratedForce:
                    beam.AddConcentratedForce(load.Magnitude, load.Location);
                    break;
                case LoadType.ConcentratedMoment:
                    beam.AddConcentratedMoment(load.Magnitude, load.Location);
                    break;
                case LoadType.DistributedForce:
                    beam.AddConstantlyDistributedForce(load.Magnitude, load.Location, load.EndLocation);
                    break;
            }
        }

        InternalForces forces;
        try
        {
            forces = beam.ComputeInternalForce();
        }
        catch (Exception ex)
        {
            return state with { Results = null, ComputeError = $"Analysis failed: {ex.Message}" };
        }

        // Discontinuity locations need extra sample points on each side so
        // the diagram shows sharp jumps instead of a sloped approximation.
        var jumpXs = state.Loads
            .Where(l => l.Type is LoadType.ConcentratedForce or LoadType.ConcentratedMoment)
            .Select(l => l.Location)
            .Concat(state.Loads
                .Where(l => l.Type == LoadType.DistributedForce)
                .SelectMany(l => new[] { l.Location, l.EndLocation }))
            .Where(x => x > 0 && x < state.Length)
            .Distinct()
            .ToList();

        var shearPts  = Sample(forces.ShearFunction,  state.Length, jumpXs);
        var momentPts = Sample(forces.MomentFunction, state.Length, jumpXs);

        var results = new BeamResults(
            ShearX:            shearPts.Select(p => p.x).ToArray(),
            ShearY:            shearPts.Select(p => p.y).ToArray(),
            MomentX:           momentPts.Select(p => p.x).ToArray(),
            MomentY:           momentPts.Select(p => p.y).ToArray(),
            LoadingDiagramSvg: BuildLoadingDiagramSvg(state));

        return state with { Results = results, ComputeError = null };
    }

    static List<(double x, double y)> Sample(ISingularityFunction fn, double length, List<double> jumpXs, int n = 250)
    {
        const double eps = 1e-7;
        var xs = new SortedSet<double>();

        for (int i = 0; i <= n; i++)
            xs.Add(length * i / n);

        foreach (double d in jumpXs)
        {
            if (d - eps >= 0)        xs.Add(d - eps);
            xs.Add(d);
            if (d + eps <= length)   xs.Add(d + eps);
        }

        return xs.Select(x => (x, y: fn.Evaluate(x)))
                 .Where(p => double.IsFinite(p.y))
                 .ToList();
    }

    // ── SVG loading diagram ───────────────────────────────────────────────────
    // ViewBox: 0 0 600 110   content area: x=[25,575]  y=[12,98]

    const double SvgW = 600, SvgH = 110, MX = 25, MY = 12;
    static double DataW => SvgW - 2 * MX;

    static double MapX(double x, double length) => MX + x / length * DataW;

    static string BuildLoadingDiagramSvg(BeamState state)
    {
        double beamY = SvgH * 0.55;
        double x0    = MapX(0, state.Length);
        double xEnd  = MapX(state.Length, state.Length);
        double maxMag = state.Loads.Max(l => Math.Abs(l.Magnitude));
        double scale  = maxMag > 0 ? 38.0 / maxMag : 1.0;

        var sb = new System.Text.StringBuilder();
        sb.Append($"<line x1=\"{x0:F1}\" y1=\"{beamY:F1}\" x2=\"{xEnd:F1}\" y2=\"{beamY:F1}\" stroke=\"#333\" stroke-width=\"2.5\"/>");

        var (tStart, rStart, tEnd, rEnd) = GetRestraints(state.Support);
        sb.Append(SupportSvg(x0,   beamY, tStart, rStart, isStart: true));
        sb.Append(SupportSvg(xEnd, beamY, tEnd,   rEnd,   isStart: false));

        foreach (var load in state.Loads)
        {
            double lx = MapX(load.Location, state.Length);

            if (load.Type == LoadType.ConcentratedForce)
            {
                double arrowH = load.Magnitude * scale;
                double yTip   = beamY;
                double yBase  = beamY - arrowH;
                double labelY = Math.Min(yBase - 2, MY + 9);
                sb.Append($"<line x1=\"{lx:F1}\" y1=\"{yBase:F1}\" x2=\"{lx:F1}\" y2=\"{yTip - 6:F1}\" stroke=\"#dc3545\" stroke-width=\"1.8\"/>");
                sb.Append($"<polygon points=\"{lx:F1},{yTip:F1} {lx - 4:F1},{yTip - 8:F1} {lx + 4:F1},{yTip - 8:F1}\" fill=\"#dc3545\"/>");
                sb.Append($"<text x=\"{lx:F1}\" y=\"{labelY:F1}\" font-size=\"9\" fill=\"#dc3545\" text-anchor=\"middle\">{load.Magnitude} kN</text>");
            }
            else if (load.Type == LoadType.ConcentratedMoment)
            {
                double r  = 12;
                double cy = beamY - 18;
                sb.Append("<defs><marker id=\"arrowM\" markerWidth=\"6\" markerHeight=\"6\" refX=\"3\" refY=\"3\" orient=\"auto\">");
                sb.Append("<path d=\"M0,0 L6,3 L0,6 Z\" fill=\"#fd7e14\"/></marker></defs>");
                sb.Append($"<path d=\"M {lx - r:F1} {cy:F1} A {r} {r} 0 0 1 {lx + r:F1} {cy:F1}\" fill=\"none\" stroke=\"#fd7e14\" stroke-width=\"1.8\" marker-end=\"url(#arrowM)\"/>");
                sb.Append($"<text x=\"{lx:F1}\" y=\"{cy - r - 2:F1}\" font-size=\"9\" fill=\"#fd7e14\" text-anchor=\"middle\">{load.Magnitude} kN·m</text>");
            }
            else if (load.Type == LoadType.DistributedForce)
            {
                double arrowH  = load.Magnitude * scale;
                double lx2     = MapX(load.EndLocation, state.Length);
                double rectTop = beamY - Math.Abs(arrowH);
                double midX    = (lx + lx2) / 2;
                double labelY  = Math.Min(rectTop - 2, MY + 9);

                sb.Append($"<rect x=\"{lx:F1}\" y=\"{rectTop:F1}\" width=\"{lx2 - lx:F1}\" height=\"{Math.Abs(arrowH):F1}\" fill=\"#0d6efd\" fill-opacity=\"0.15\" stroke=\"#0d6efd\" stroke-width=\"1\"/>");
                sb.Append($"<line x1=\"{lx:F1}\" y1=\"{rectTop:F1}\" x2=\"{lx2:F1}\" y2=\"{rectTop:F1}\" stroke=\"#0d6efd\" stroke-width=\"1.2\"/>");
                sb.Append($"<text x=\"{midX:F1}\" y=\"{labelY:F1}\" font-size=\"9\" fill=\"#0d6efd\" text-anchor=\"middle\">{load.Magnitude} kN/m</text>");

                int ticks = Math.Max(2, (int)((lx2 - lx) / 30));
                for (int i = 0; i <= ticks; i++)
                {
                    double tx = lx + i * (lx2 - lx) / ticks;
                    sb.Append($"<line x1=\"{tx:F1}\" y1=\"{rectTop:F1}\" x2=\"{tx:F1}\" y2=\"{beamY - 6:F1}\" stroke=\"#0d6efd\" stroke-width=\"1\"/>");
                    sb.Append($"<polygon points=\"{tx:F1},{beamY:F1} {tx - 3:F1},{beamY - 7:F1} {tx + 3:F1},{beamY - 7:F1}\" fill=\"#0d6efd\"/>");
                }
            }
        }

        sb.Append($"<text x=\"{x0:F1}\" y=\"{SvgH - 1}\" font-size=\"9\" fill=\"#888\" text-anchor=\"middle\">0</text>");
        sb.Append($"<text x=\"{xEnd:F1}\" y=\"{SvgH - 1}\" font-size=\"9\" fill=\"#888\" text-anchor=\"middle\">{state.Length} m</text>");

        return sb.ToString();
    }

    static string SupportSvg(double x, double beamY, Restraint translation, Restraint rotation, bool isStart)
    {
        if (translation == Restraint.Released) return "";
        return rotation == Restraint.Released ? PinSvg(x, beamY) : FixedSvg(x, beamY, isStart);
    }

    static string PinSvg(double x, double beamY) =>
        $"<polygon points=\"{x:F1},{beamY:F1} {x - 7:F1},{beamY + 12:F1} {x + 7:F1},{beamY + 12:F1}\" fill=\"#555\"/>";

    static string FixedSvg(double x, double beamY, bool isStart)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"<line x1=\"{x:F1}\" y1=\"{beamY - 16:F1}\" x2=\"{x:F1}\" y2=\"{beamY + 16:F1}\" stroke=\"#333\" stroke-width=\"3\"/>");
        double dx = isStart ? -8.0 : 8.0;
        for (int i = 0; i < 5; i++)
        {
            double y1 = beamY - 12 + i * 6;
            sb.Append($"<line x1=\"{x:F1}\" y1=\"{y1:F1}\" x2=\"{x + dx:F1}\" y2=\"{y1 + 6:F1}\" stroke=\"#666\" stroke-width=\"1\"/>");
        }
        return sb.ToString();
    }

    static (Restraint tStart, Restraint rStart, Restraint tEnd, Restraint rEnd) GetRestraints(SupportType s) => s switch
    {
        SupportType.SimplySupported   => (Restraint.Fixed,    Restraint.Released, Restraint.Fixed,    Restraint.Released),
        SupportType.Cantilever        => (Restraint.Fixed,    Restraint.Fixed,    Restraint.Released, Restraint.Released),
        SupportType.ProppedCantilever => (Restraint.Fixed,    Restraint.Fixed,    Restraint.Fixed,    Restraint.Released),
        SupportType.FixedFixed        => (Restraint.Fixed,    Restraint.Fixed,    Restraint.Fixed,    Restraint.Fixed),
        _                             => (Restraint.Fixed,    Restraint.Released, Restraint.Fixed,    Restraint.Released),
    };
}
