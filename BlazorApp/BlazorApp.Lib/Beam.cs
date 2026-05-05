using BlazorApp.Lib.Functions.Singularity;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace BlazorApp.Lib;

public class Beam
{
    public double Length { get; set; }

    public Restraint TranslationStart { get; set; } = Restraint.Released;
    public Restraint RotationStart { get; set; } = Restraint.Released;
    public Restraint TranslationEnd { get; set; } = Restraint.Released;
    public Restraint RotationEnd { get; set; } = Restraint.Released;

    private SingularityFunction _loadFunction = new SingularityFunction();

    public void AddConcentratedMoment(double magnitude, double location)
    {
        var term = new SingularityTerm(magnitude, location, -2);
        _loadFunction.AddTerm(term);
    }

    public void AddConcentratedForce(double magnitude, double location)
    {
        var term = new SingularityTerm(-magnitude, location, -1);
        _loadFunction.AddTerm(term);
    }

    public void AddConstantlyDistributedForce(double magnitude, double startLocation, double endLocation)
    {
        _loadFunction.AddTerm(new SingularityTerm(-magnitude, startLocation, 0));
        _loadFunction.AddTerm(new SingularityTerm(magnitude, endLocation, 0));
    }

    public InternalForces ComputeInternalForce()
    {
        var shearFunction = _loadFunction.Clone();
        shearFunction.Integrate();

        var momentFunction = shearFunction.Clone();
        momentFunction.Integrate();

        var rotationFunction = momentFunction.Clone();
        rotationFunction.Integrate();

        var deflectionFunction = rotationFunction.Clone();
        deflectionFunction.Integrate();

        double[] constantsArray = new double[4];

        if (TranslationStart == Restraint.Fixed) // if translation dof is fixed - delta i is zero
        {
            constantsArray[0] = 0.0;
        }
        else // if translation dof is free - shear i is zero
        {
            constantsArray[0] = 0.0;
        }

        if (RotationStart == Restraint.Fixed) // if start rotation is fixed - theta i is zero
        {
            constantsArray[1] = 0.0;
        }
        else // if start rotation is free - moment i is zero
        {
            constantsArray[1] = 0.0;
        }

        if (TranslationEnd == Restraint.Fixed) // if end translation is fixed - delta j is zero
        {
            constantsArray[2] = deflectionFunction.Evaluate(Length);
        }
        else // if end translation is free - shear j is zero
        {
            constantsArray[2] = shearFunction.Evaluate(Length);
        }

        if (RotationEnd == Restraint.Fixed) // if end rotation is fixed, theta j is zero
        {
            constantsArray[3] = rotationFunction.Evaluate(Length);
        }
        else // if end rotation is free, moment j is zero
        {
            constantsArray[3] = momentFunction.Evaluate(Length);
        }

        var constantsVector = DenseVector.OfArray(constantsArray);

        Vector<double> row1;
        Vector<double> row2;
        Vector<double> row3;
        Vector<double> row4;

        if (TranslationStart == Restraint.Fixed) // if translation dof is fixed - delta i is zero
        {
            row1 = DenseVector.OfArray(new double[] { 0, 0, 0, 1 });
        }
        else // if translation dof is free - shear i is zero
        {
            row1 = DenseVector.OfArray(new double[] { 1, 0, 0, 0 });
        }

        if (RotationStart == Restraint.Fixed) // if start rotation is fixed - theta i is zero
        {
            row2 = DenseVector.OfArray(new double[] { 0, 0, 1, 0 });
        }
        else // if start rotation is free - moment i is zero
        {
            row2 = DenseVector.OfArray(new double[] { 0, 1, 0, 0 });
        }

        if (TranslationEnd == Restraint.Fixed) // if end translation is fixed - delta j is zero
        {
            row3 = DenseVector.OfArray(new double[] { (1.0 / 6.0) * Math.Pow(Length, 3), (1.0 / 2.0) * Math.Pow(Length, 2), Length, 1 });
        }
        else // if end translation is free - shear j is zero
        {
            row3 = DenseVector.OfArray(new double[] { 1, 0, 0, 0 });
        }

        if (RotationEnd == Restraint.Fixed) // if end rotation is fixed, theta j is zero
        {
            row4 = DenseVector.OfArray(new double[] { (1.0 / 2.0) * Math.Pow(Length, 2), Length, 1, 0 });
        }
        else // if end rotation is free, moment j is zero
        {
            row4 = DenseVector.OfArray(new double[] { Length, 1, 0, 0 });
        }

        var coefficientsMatrix = DenseMatrix.OfRowVectors(row1, row2, row3, row4);

        var initialConditions = coefficientsMatrix.Inverse().Multiply(constantsVector.Negate());

        var revisedShearFunction = _loadFunction.Clone() as SingularityFunction;

        if (initialConditions[0] != 0)
        {
            revisedShearFunction!.AddTerm(new SingularityTerm(initialConditions[0], 0, -1));
        }

        revisedShearFunction.Integrate();

        var revisedMomentFunction = revisedShearFunction.Clone() as SingularityFunction;

        if (initialConditions[1] != 0)
        {
            revisedMomentFunction!.AddTerm(new SingularityTerm(initialConditions[1], 0, -2));
        }

        revisedMomentFunction.Integrate();

        return new InternalForces()
        {
            ShearFunction = revisedShearFunction,
            MomentFunction = revisedMomentFunction
        };
    }
}

public class InternalForces
{
    public ISingularityFunction ShearFunction { get; set; }

    public ISingularityFunction MomentFunction { get; set; }
}

public enum Restraint
{
    Released,
    Fixed
}