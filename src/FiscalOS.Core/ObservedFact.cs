namespace FiscalOS.Core;

public sealed record ObservedFact(string ConceptCode, object Value)
{
    public static ObservedFact Revenue(decimal value) => new("REVENUE", value);

    public static ObservedFact EmployeeCount(int value) => new("EMPLOYEE_COUNT", value);
}
