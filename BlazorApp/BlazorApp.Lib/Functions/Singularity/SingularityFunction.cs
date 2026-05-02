namespace BlazorApp.Lib.Functions.Singularity;

public interface ISingularityFunction
{
    ISingularityFunction Clone();

    double Evaluate(double x);

    void Integrate();
}

public class SingularityFunction : ISingularityFunction
{
    private List<ISingularityFunction> _terms = new List<ISingularityFunction>();

    public void AddTerm(ISingularityFunction term)
    {
        _terms.Add(term);
    }

    public ISingularityFunction Clone()
    {
        var clone = new SingularityFunction();

        foreach (var term in _terms)
        {
            clone.AddTerm(term.Clone());
        }

        return clone;
    }

    public double Evaluate(double x)
    {
        double result = 0;

        foreach (var term in _terms)
        {
            result += term.Evaluate(x);
        }

        return result;
    }

    public void Integrate()
    {
        foreach (var term in _terms)
        {
            term.Integrate();
        }
    }
}
