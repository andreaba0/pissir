using System;

namespace Types;
public static class CustomDbTypeTest {

    [Test]
    public static void Uppercase() {
        Assert.That(CustomDbTypeExtensions.ToString(CustomDbType.IndustrySector.WSP), Is.EqualTo("WSP"));
        Assert.That(CustomDbTypeExtensions.ToString(CustomDbType.IndustrySector.FAR), Is.EqualTo("FAR"));
    }
}