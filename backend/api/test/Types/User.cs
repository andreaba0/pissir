namespace Types;

public class UserTest
{

    [Test]
    public void HasTest()
    {

        {
            User user = new User();
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.ID }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.EMAIL }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.NAME }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.SURNAME }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.ROLE }));
        }

        {
            User user = new User();
            user.Id = "test";
            Assert.IsTrue(user.Has(new User.Fields[] { User.Fields.ID }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.EMAIL }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.NAME }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.SURNAME }));
            Assert.IsFalse(user.Has(new User.Fields[] { User.Fields.ROLE }));
        }
    }

    [Test]
    public void ParseJsonEncodedTest() {
        {
            string json = @"{
                ""Role"": ""WSP"",
                ""Email"": """"
            }";
            User user = User.ParseJsonEncoded(json);
            Assert.That(user.Role, Is.EqualTo("WSP"));
        }
    }

}