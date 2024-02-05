using Middleware;
using Moq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Extension;

public static class AuthenticationTest
{

    [Test]
    public static void IsAuthenticatedTest()
    {
        {
            string jwt = string.Empty;
            string message = string.Empty;
            bool isAuthenticated = Authentication.IsAuthenticated(string.Empty, out jwt, out message);
            Assert.AreEqual(isAuthenticated, false);
            Assert.AreEqual(jwt, string.Empty);
            Assert.AreEqual(message, "Missing Authorization header");
        }

        {
            string jwt = string.Empty;
            string message = string.Empty;
            bool isAuthenticated = Authentication.IsAuthenticated("Bearer ", out jwt, out message);
            Assert.AreEqual(isAuthenticated, false);
            Assert.AreEqual(jwt, string.Empty);
            Assert.AreEqual(message, "Invalid Authorization header");
        }

        {
            string jwt = string.Empty;
            string message = string.Empty;
            bool isAuthenticated = Authentication.IsAuthenticated("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", out jwt, out message);
            Assert.AreEqual(isAuthenticated, false);
            Assert.AreEqual(jwt, string.Empty);
            Assert.AreEqual(message, "Invalid token format, expected Json Web Token");
        }

        {
            string jwt = string.Empty;
            string message = string.Empty;
            bool isAuthenticated = Authentication.IsAuthenticated("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", out jwt, out message);
            Assert.AreEqual(isAuthenticated, true);
            Assert.AreEqual(jwt, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
            Assert.AreEqual(message, string.Empty);
        }

        {
            string jwt = string.Empty;
            string message = string.Empty;
            bool isAuthenticated = Authentication.IsAuthenticated(" Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", out jwt, out message);
            Assert.AreEqual(isAuthenticated, false);
            Assert.AreEqual(jwt, string.Empty);
            Assert.AreEqual(message, "Invalid Authorization header");
        }
    }

    /*[Test]
    public static async Task JwtCheckTest()
    {
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var httpResponseExtension = new Mock<IStaticWrapperHRE>();
        int statusCode = 0;
        string messageResponse = string.Empty;
        context.Setup(x => x.Request).Returns(request.Object);
        context.Setup(x => x.Response).Returns(response.Object);
        context.Setup(x => x.Request.Headers["Authorization"]).Returns(string.Empty);
        context.SetupSet(x => x.Response.StatusCode).Callback((int code) =>
        {
            statusCode = code;
        });
        httpResponseExtension.Setup(x => x.WriteAsync(It.IsAny<HttpResponse>(), It.IsAny<string>())).Callback((HttpResponse response, string message) =>
        {
            messageResponse = message;
        });

        await Authentication.JwtCheck(context.Object, async () =>
        {
            await Task.CompletedTask;
        }, httpResponseExtension.Object);

        Assert.AreEqual(statusCode, 401);
        Assert.AreEqual(messageResponse, "Missing Authorization header");
    }*/

}