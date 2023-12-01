namespace Data;

class KeyString
{
    public string KeyPrivate { get; set; }
    public string KeyPublic { get; set; }
    public string KeyId { get; set; }
}

public class KeyManager
{
    private readonly Dictionary<string, RsaSecurityKey> privateKeys = new Dictionary<string, RsaSecurityKey>();
    private readonly Dictionary<string, RsaSecurityKey> publicKeys = new Dictionary<string, RsaSecurityKey>();

    public KeyManager()
    {
        KeyString key1 = new KeyString();
        key1.KeyPrivate = @"-----BEGIN PRIVATE KEY-----
MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQC/ZuOufYCLowS2
EzQOwOFkHH72TNQ6CbDwIaGFnZUHmDhPCN3GE5Z4L0B8ndCdnuIVuUydoPkd375T
9MWp1i2TV4KG6f/aytq+CU0yP6r6EViEUGWG5uV8PIEAYaQ5acQZ+CKLaCk83iRy
Yq35HzL0vWXJS0mB74d/0veq1QYJ3IMvSqaTEC8YHxWyxPvaJeK1WEggu/ALmobY
QjRAW7A94+jy2ME6P7O3urYZSvOmzJNUbTvTmFUAVeKMZu2YCCtNMVmbgBvYvZqa
8ehyGwmMDg/4a8q0w+lgWEH7Y3lGMUdB28ZWUByizgXz+g8iHv9KxHaR8btOccSg
+pZR6vhRAgMBAAECggEAGI0jnnHFx+OYypwq/Mt7/eHz3Tccr/cnRZ4iwGdmwush
Ke4quGoxzmfNP07lABrtyDiVHdEUs2LMYKuQoUjyXqiGezTP12pW5bJ+vvfQwDPy
ZUKEy2mZvTZrm7dqn9yzxVsBX110kLSz+yIyS7bXS/JxeIEaRDsL5/cRQs4BcMkp
9DRmQ2OUwBgdcZ70Hskt/TyjIyoRC0a5Wa7LRqcllPmd1tL/4jhxjCMZCQPAPkxh
25ktRrnLrcY3rZhz8Lpo+pb3HnHxAorYE63nMwLAD79MqUoX4j3uqQVq2i7FjSJg
QvJwzuRG4ghfwnQ1MIdIMylAqAqNJeS938v8oXSuawKBgQDtLpL8UoSHTQfGk9+7
oh2nfHHBJNmT1vbxpjC/0LM6M1QdRocyDfWctYeQBSH9imp0+AdzSwzFNSlIeo6y
5ciAt2HG9MxxJRvSYe2fjr7Vb53wEBoYnJuafbMcQQd9E+1MPt2K4mLEbycqS9aA
ljiy+iHrYo/Kw6TECJhkNEt8XwKBgQDOlnkCfrwKWRC3BJKE2g+6wqoRQpbJH80d
x0gac0TftUnOTx+MuueBroL1t3AyiuhB9exnKgC3QhRkuO8otV/ozMeOtdW+oigW
/Nux7+SZQnJ1ATDwSJz5ona9zio70JNjj5oYoqdyiKruBmQJL+78QbrV0MZxV99E
CArYOKfJTwKBgGYwBswHkWnp2kvIMkDZHAWpLCmXAtcbatG6VI6QEGgm7TqfxGfg
c83MuFdonrhqBmvW4P7feuUMfnjntMuHDNfIKqxZlCu7XT1LS+HO0pGKwXVIurWX
48KwHZcyvVM6XqbW/wRzfTI84ZbrjcNg9ZTHCA4uLN2jZT3Apkel8E/pAoGAa3tM
MCkhyjx2ftHpLSeKwX8gHmTHsJJUFGcnM2O/dKiMYsM1xfFEG2thBVhQDmvI1PCO
80meH7pIg/LXsxVSdft849nlAA67zuH1p4kJJVe11vwsqwcMbLDDGJNX09D7rfP7
l0+g0O0cCUTX7RO8QhAslavOYw/1wB5zDfXEDe8CgYBqg58afUL/z3Fp2Ha0U+8z
pnfL+H6wrAbqoRtEdeLDcK1znddrfye1JYaGyK08v2xG9gkESlqyTbMNbK+bUPmR
BhTE7o8IyLplAuQFrTaaMLU7yF77ay4TBxm6yTlTGku5614VZZFFFgBujSrd7pBg
xYqWB6xWlcPGJGMpH8NBEA==
-----END PRIVATE KEY-----
";
        key1.KeyPrivate = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv2bjrn2Ai6MEthM0DsDh
ZBx+9kzUOgmw8CGhhZ2VB5g4TwjdxhOWeC9AfJ3QnZ7iFblMnaD5Hd++U/TFqdYt
k1eChun/2sravglNMj+q+hFYhFBlhublfDyBAGGkOWnEGfgii2gpPN4kcmKt+R8y
9L1lyUtJge+Hf9L3qtUGCdyDL0qmkxAvGB8VssT72iXitVhIILvwC5qG2EI0QFuw
PePo8tjBOj+zt7q2GUrzpsyTVG0705hVAFXijGbtmAgrTTFZm4Ab2L2amvHochsJ
jA4P+GvKtMPpYFhB+2N5RjFHQdvGVlAcos4F8/oPIh7/SsR2kfG7TnHEoPqWUer4
UQIDAQAB
-----END PUBLIC KEY-----
";
        key1.KeyId = "key1"
KeyString key2 = new KeyString();
        key2.KeyPrivate = @"-----BEGIN PRIVATE KEY-----
MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQDW518Aq/IsSFbF
rFiR6c5TY/uf+mg2xg4baZiCHf7FBhshEPBBleqc5NTavWDLBv/g+7NEKmZrGSsM
7Egfxa4Is51g5GHXKl4NVSAH7SQ76QsFgrn96yGcJFSKSr9vtUulD/euLg9j+eXD
8vsYZ4DIbWexO594gXJvA/1oxzYNCZ2kbz6v6TlPCxq6zYsFgLn56HS0or35tUIk
GBoJ4T5zUv4zpPLANrXCGqBnMakouajGu1VNJsnlU4UccsoavXaH3Ertv87RNEo1
QB0koVjsKvcoxETHpdKHkaLY7PfW+f9tSZUKXY5LoPdJatqN5G/GVrxxcxGC658h
ANodLNupAgMBAAECggEAAKTwcInrRoBOj/3IMfYfR2LXnRgMNUwofYuHiliqjsgV
8a1vSKoqR9ntsEo/4kab8jMmaJikO+aRGkC5edJBkvA3O3afvqqCSGwovghtHSYL
bXyG/ekOwJg9OVOOwzcygaU9ePnLcqvcQLXJvfAPg/IN1A3fp6w4cBe48Yfuyo8A
KBNdLBsF28vjhJHuzfcYvdRSYnUlxWzogboCGnxyPkbNxW7jACkxvObu/s5tippM
6vBPCD32g/26gPElrw64wF/7a+s5E+nL00OJXCHhIUOrppEczts2BJ+em32ixt4G
hRS6wueRRPjpWfddyhOhLELoBVSTAJf293XM3eXQ+QKBgQDZ+OTknO1FHaj0yjdF
Md23d5AUaREAmDy8I1rweSNdMvAH4xg8DUE6vxt8jSfU0mDUa45UWGL4ZngCvxMn
U07dmO+JA/KWYkF9/H0ur1nq2KvAwy0vLC7ZeQtqdiZhZMJGjq8PwQ0Jz9whJwjl
SuoRO7N1IShMMrNOXMb1paq6swKBgQD8ZW75bjS3I/w4/xBrdnvwQQ7G4dE91b66
+mnciQIt9Mg8tBwmSWqZ/FBlAyeJ/2LN+Qshyl8T8awP5CfXyWhFqxYi9+pfy2Fv
KQV2FdaEzgEpozh0XCJLWNPum0aa7yMYVkYdHy0R21FvajkPbqvg0jP3yrjwjOum
lolPW6yuMwKBgFoR4DuboOwDgpKMkOQ14+FxNYdJly4AunAfx+9Aj11utAL+U0BV
VdP5rlj8Vy45NKvyOeEemN7+MK8p17dTVG57SNf8WpzLwzZVREErUmOZ3vcLb7tj
bYSN4Sq/4cIzCZhwU2SWKFYan2LPp3xgRXKTkedfqTA7bx59dfWB3JfDAoGAYKxB
W0x46ITiALrBgUe6pQVmMYsg5/xMt7Vegtf/e7a8U3PxLEEGkcBw0p/orxSgtZ/k
QacPz8XLT7LmB+LfC4BCVFZ88QAna7pcmyQ2LFvzWg1ofDkl8ZIuasASxRk/afzx
kgn061gD0xbukBilwNECkrCVMZ3oTMBwC3Njqr0CgYBNSufKfToXczaU2uT3bzZo
s2yloB43Pwg4SCtAwbyM5YsOofr+dXkTh4icM3Uy/zJZme5lxX+faIfU4XwkZ1Yk
dBO/kwfbuU8DllPrVOI5lQ8lE/LMv25uLI+mYFzrhTMVhbu9MWuzd9n+6cNOcvEA
nEkf/15KJizKLEsWFlokqQ==
-----END PRIVATE KEY-----
";
        key2.KeyPublic = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1udfAKvyLEhWxaxYkenO
U2P7n/poNsYOG2mYgh3+xQYbIRDwQZXqnOTU2r1gywb/4PuzRCpmaxkrDOxIH8Wu
CLOdYORh1ypeDVUgB+0kO+kLBYK5/eshnCRUikq/b7VLpQ/3ri4PY/nlw/L7GGeA
yG1nsTufeIFybwP9aMc2DQmdpG8+r+k5Twsaus2LBYC5+eh0tKK9+bVCJBgaCeE+
c1L+M6TywDa1whqgZzGpKLmoxrtVTSbJ5VOFHHLKGr12h9xK7b/O0TRKNUAdJKFY
7Cr3KMREx6XSh5Gi2Oz31vn/bUmVCl2OS6D3SWrajeRvxla8cXMRguufIQDaHSzb
qQIDAQAB
-----END PUBLIC KEY-----
";
        key2.KeyId = "key2";
        KeyString key3 = new KeyString();
        key3.KeyPrivate = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCUoDISo+vnIVdz
y517rqlQKmiT+mhg/UdTQfRW10NJPByegbExuYYVBQZlUD9OQKmstUb6AjZEpeo5
2KnbJTzx5ZPllKHuxfd6rRNuC2rbWZnzG8WTXTkePByNCPgXVciT9tZPxvfJkMCW
psyApS/9LG5UJhGr3KgfO3KvSuF+GnpaDWbT0Sexz3xfwrtVPBErmO6bFFGb/tJh
OxYc8qAYJhtLATm50do6rUu/nKgyf9bnv0cXN91Kd10usIf8Uciln3Vr/vIi9Y22
RAK+WIs4buURZEz1fgDMSKU6g9vff1BgPMGoEhJ7X9RmgbaiLrfG4HYKFnCBIPVb
LJikhMbDAgMBAAECggEAALyuouMHN3U13tAirstV8ZzUOe+i9uAqq2IHWSrmHYXb
ynlwLIP0A0SsHHKCbdvqO34GpRS/uiVsfO7zILAVlkmCK0BKcUmwCX60OvZ5iC7w
kYEQwMNUli7MIXLq8Aqv3PP7ahfdgLVkDxsypmyrSUEgJvCtY8HLX6RG1FG1vo9X
HgcsX+tZhHnujM14Uz/P/rjiTDw0UTsbo1eidDYNVHZpuBSZj64FLItk3LIYztTB
iKz/3CRsEQtRx/HooD5ZNCxApBSrfm5fJBgCKFT9/03WFJnnzNsLNZbjvgblNpsG
pSmJOU7Rkd1y+lOlWHwWZy1Gkqs5kSjwvIGwDSpoMQKBgQC5AsJU+TqSAGpC9tWu
Mb5ksHYf40HPfpLAx702MZB7mYn1o9/5isvSWqdvshAdXza0zcN1BgD5WArfuG2P
gJJdFBj4yjm4qVi1e1Vr/Drm88a/qFBPEJYRBE1mCzLdh6pUsLL2kfVsohu65wIA
eCCXsLjEVKfPyDnkthOPV9TOmQKBgQDNp2ivUb8zXlVzHt38B9AwZGNh7DO/NksW
wjyoHWWnokI1BJ0zXEBvZ9ikDl51nUdFNSH+2qQ3EzPp9IlpyomBNpTP3WPiv9K+
qE2zon17EynP/sAuIa5iSRXEXt0Ddl0OcSr3UfFrzwLYS869JpAu2C4xW8tjxFF8
NOlV0jvluwKBgEjqSna85xWFrluH9r05g6UhWtzEbzp80w/BaNQEAsKDx7iVBn/N
8PiNm/HMhsdcsC3f7omffSDITzjO8fcdoBGpCxJ6ePdLXtffrNHrTbeaGhmNF2Gh
0tMQFAUEot5mSn4oSdAdxFE1LqKhyssujQHbt2ZBSwX8Dr0R+XGEFy35AoGAGIX5
BscBvNdn6yVoJTCtU8f0ze2Dsi6AP8ODxixTPe0sZfQZ5tD+YgqJG+8WtoG9yPPw
DNr3sBWbIC/n3vSm9wCSOENXMOfc8p1RPrWGrxF27/WZ5yZfDBtY/CSvyETqDdnS
3NEpr8hst2w6x/V8RgnDYGFo3InBicUpefFq8RcCgYEAjpp8UAPe0qkDfE7/n0z1
DAYRldsIKp+QivUpGUu+kJeh7WELptNWdA94kNk/0mkv+iGJckEKtwwZ15jwuUT+
ual3E41saRLC8XHmKc+F9+wtuv03m0OeICpCXbUqDjsbfXFE6LTvF/kpRI+odCql
oZSW7HLwxTIjVY25fo58TAY=
-----END PRIVATE KEY-----
";
        key3.KeyPublic = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAlKAyEqPr5yFXc8ude66p
UCpok/poYP1HU0H0VtdDSTwcnoGxMbmGFQUGZVA/TkCprLVG+gI2RKXqOdip2yU8
8eWT5ZSh7sX3eq0Tbgtq21mZ8xvFk105HjwcjQj4F1XIk/bWT8b3yZDAlqbMgKUv
/SxuVCYRq9yoHztyr0rhfhp6Wg1m09Ensc98X8K7VTwRK5jumxRRm/7SYTsWHPKg
GCYbSwE5udHaOq1Lv5yoMn/W579HFzfdSnddLrCH/FHIpZ91a/7yIvWNtkQCvliL
OG7lEWRM9X4AzEilOoPb339QYDzBqBISe1/UZoG2oi63xuB2ChZwgSD1WyyYpITG
wwIDAQAB
-----END PUBLIC KEY-----
";
        key3.KeyId = "key3";
        
        RSA rsa1 = RSA.Create();
        rsa1.ImportFromPem(key1.KeyPrivate.ToCharArray());
        publicKeys.Add(key1.KeyId, new RsaSecurityKey(rsa1.ExportParameters(false))
        {
            KeyId = key1.KeyId
        });
        privateKeys.Add(key1.KeyId, new RsaSecurityKey(rsa1.ExportParameters(true))
        {
            KeyId = key1.KeyId
        });

        RSA rsa2 = RSA.Create();
        rsa2.ImportFromPem(key2.KeyPrivate.ToCharArray());
        publicKeys.Add(key2.KeyId, new RsaSecurityKey(rsa2.ExportParameters(false))
        {
            KeyId = key2.KeyId
        });
        privateKeys.Add(key2.KeyId, new RsaSecurityKey(rsa2.ExportParameters(true))
        {
            KeyId = key2.KeyId
        });

        RSA rsa3 = RSA.Create();
        rsa3.ImportFromPem(key3.KeyPrivate.ToCharArray());
        publicKeys.Add(key3.KeyId, new RsaSecurityKey(rsa3.ExportParameters(false))
        {
            KeyId = key3.KeyId
        });
        privateKeys.Add(key3.KeyId, new RsaSecurityKey(rsa3.ExportParameters(true))
        {
            KeyId = key3.KeyId
        });
    }

    public RsaSecurityKey GetPublicKey(string keyId)
    {
        if (publicKeys.ContainsKey(keyId))
        {
            return publicKeys[keyId];
        }
        else
        {
            return null;
        }
    }

    public RsaSecurityKey GetPrivateKey(string keyId)
    {
        if (privateKeys.ContainsKey(keyId))
        {
            return privateKeys[keyId];
        }
        else
        {
            return null;
        }
    }
}