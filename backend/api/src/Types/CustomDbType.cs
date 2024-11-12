namespace Types;

public class CustomDbType {
    public enum IndustrySector {
        WSP,
        FAR
    }

    public enum ObjectType {
        UMDTY,
        TMP,
        ACTUATOR
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

    public static string ToString(this CustomDbType.ObjectType type) {
        return type switch {
            CustomDbType.ObjectType.UMDTY => "UMDTY",
            CustomDbType.ObjectType.TMP => "TMP",
            CustomDbType.ObjectType.ACTUATOR => "ACTUATOR",
            _ => throw new NotImplementedException()
        };
    }
}