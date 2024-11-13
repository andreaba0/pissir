using System;
using System.Text.RegularExpressions;

namespace Routes;
public static class CompanySecretTest {
    [Test]
    public static void PasswordGeneratorTest() {
        string password = CompanySecret.PasswordGenerator(16);
        Console.WriteLine(password);
        Assert.AreEqual(16, password.Length);
        Assert.IsTrue(Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"));
    }
}