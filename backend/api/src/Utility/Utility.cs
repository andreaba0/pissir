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

    public struct CountEntity {
        public DateTimeOffset date;
        public bool status;
    }

    /// <summary>
    /// This methods return the number of seconds in which the status is true
    /// true means actuator is on
    /// false means actuator is off
    /// </summary>
    /// <param name="refer"></param>
    /// <param name="entities"></param>
    /// <returns></returns>
    public static long CountSeconds(DateTimeOffset refer, List<CountEntity> entities) {
        long count = 0;
        if (entities.Count == 0) {
            return 0;
        }
        long referEpoch = refer.ToUnixTimeSeconds();
        long start = entities[0].date.ToUnixTimeSeconds();
        if(start < referEpoch && entities[0].status == true && entities.Count == 1) {
            return (referEpoch-start) + (60*60*24);
        }

        bool prevStatus = entities[0].status;
        for(int i = 1; i < entities.Count; i++) {
            CountEntity current = entities[i];
            if (prevStatus==false && current.status==true) {
                start = current.date.ToUnixTimeSeconds();
                prevStatus = current.status;
                continue;
            }
            if ((prevStatus==true && current.status==false)||(prevStatus==true&&current.status==true)) {
                count += (current.date.ToUnixTimeSeconds() - start);
                start = current.date.ToUnixTimeSeconds();
                prevStatus = current.status;
                continue;
            }
            start = current.date.ToUnixTimeSeconds();
            prevStatus = current.status;
        }
        if (prevStatus == true) {
            DateTimeOffset nextDay = refer.AddDays(1);
            count += nextDay.ToUnixTimeSeconds() - start;
        }
        return count;
    }


}