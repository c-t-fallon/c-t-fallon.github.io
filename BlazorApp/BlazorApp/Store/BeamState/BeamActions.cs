namespace BlazorApp.Store.BeamState;

// Actions are plain records — they describe *what happened*, not how to handle it.
// Reducers react to them; components dispatch them.

public record SetBeamLengthAction(double Length);
public record SetSupportTypeAction(SupportType Support);
public record AddLoadAction(LoadType Type, double Magnitude, double Location, double EndLocation);
public record RemoveLoadAction(int Id);
