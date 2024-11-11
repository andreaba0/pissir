namespace Types;

public class CustomDbType {
    public enum IndustrySector {
        WSP,
        FAR
    }
}

public static class CustomDbTypeExtensions {
    public static string ToString(this CustomDbType.IndustrySector sector) {
        return sector switch {
            CustomDbType.IndustrySector.WSP => "WSP",
            CustomDbType.IndustrySector.FAR => "FAR",
            _ => throw new NotImplementedException()
        };
    }
}