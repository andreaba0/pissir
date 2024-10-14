using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Utility;

public class Utility {
    public static string HmacSha256(string key, string data) {
        using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key))) {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public static string Base64URLDecode(string input) {
        input = input.Replace('-', '+').Replace('_', '/');
        switch (input.Length % 4) {
            case 2: input += "=="; break;
            case 3: input += "="; break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(input));
    }


}