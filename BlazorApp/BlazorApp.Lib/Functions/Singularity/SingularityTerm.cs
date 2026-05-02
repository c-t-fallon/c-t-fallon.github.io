namespace BlazorApp.Lib.Functions.Singularity;

public class SingularityTerm : ISingularityFunction
{
    private double _k;
    private double _a;
    private int _n;

    public SingularityTerm(double k, double a, int n)
    {
        _k = k;
        _a = a;
        _n = n;
    }

    public SingularityTerm(SingularityTerm other)
    {
        _k = other._k;
        _a = other._a;
        _n = other._n;
    }

    public ISingularityFunction Clone()
    {
        return new SingularityTerm(_k, _a, _n);
    }

    public double Evaluate(double x)
    {
        if (x < _a || _n < 0)
        {
            return 0;
        }
        else
        {
            return _k * Math.Pow(x - _a, _n);
        }
    }

    public void Integrate()
    {
        if (_n < 0)
        {
            _n = _n + 1;
        }
        else
        {
            _k = _k / (_n + 1);
            _n = _n + 1;
        }
    }

    public override string ToString()
    {
        return $"{_k} <x - {_a}> ^ {_n}";
    }
}