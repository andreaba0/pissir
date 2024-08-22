namespace Utility;

public static class EnvManager {
    public enum Variable {
        PISSIR_ISS,
        PISSIR_AUD, 
    }
    private static readonly string _base = "DOTNET_ENV";

    public static string Get(Variable variable) {
        switch (variable) {
            case Variable.PISSIR_ISS:
                return Environment.GetEnvironmentVariable($"{_base}_PISSIR_ISS");
            case Variable.PISSIR_AUD:
                return Environment.GetEnvironmentVariable($"{_base}_PISSIR_AUD");
            default:
                throw new Exception("Invalid variable");
        }
    }
}