namespace Utility;

public class DateTimeProviderTest {
    [Test]
    public void NowTest() {
        DateTimeProvider dateTimeProvider = new DateTimeProvider();
        DateTime now = dateTimeProvider.Now;
        //check that the time is within 1 second of the expected time
        Assert.IsTrue(now >= DateTime.Now.AddSeconds(-1) && now <= DateTime.Now.AddSeconds(1));
    }

    /*[Test]
    public void FutureTest() {
        DateTimeProvider dateTimeProvider = new DateTimeProvider(DateTime.Now.AddSeconds(10));
        DateTime fakeNow = dateTimeProvider.Now;
        DateTime realNow = DateTime.Now;
        //check that the time is within 1 second of the expected time
        Assert.IsTrue(fakeNow >= realNow.AddSeconds(9) && fakeNow <= realNow.AddSeconds(11));
    }

    [Test]
    public void PastTest() {
        DateTimeProvider dateTimeProvider = new DateTimeProvider(DateTime.Now.AddSeconds(-10));
        DateTime fakeNow = dateTimeProvider.Now;
        DateTime realNow = DateTime.Now;
        //check that the time is within 1 second of the expected time
        Assert.IsTrue(fakeNow >= realNow.AddSeconds(-11) && fakeNow <= realNow.AddSeconds(-9));
    }*/
}