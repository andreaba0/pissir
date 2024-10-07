/**
 * This file does not contain any sensitive information used in production.
 * It is used to simulate a cache for the OAuth server.
 * The real OAuth server used in production is available at <oauth_redirect/*>
 */


const key1 = {
    private: `-----BEGIN PRIVATE KEY-----
MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQDKxFpnRoq3nZbL
ElEZl8FYbjNNZ4MRsKgqvwrkKcPUMjZZxStP8tV8CUo4u/CjbdJ7wNkfSiuv0+IQ
/a7Pj4UAkooy/s2GuPRL2m2Nb4Zy5Qy5qerI2uHB91GDZVJrebVR+DHzQub1UIoX
4oD1e8F30RUmqbnJR1zY1vCgQFamWPbuAi2zCQh2+5LLffSHR2WTafzJUwPLQIBz
FoWs3QdkOvIe1js6sD5xj5q0q87jIvD2GqDtmqGu233BO9iLWbPOChMcNIAPx3LV
LPJJnkgjQ80qiCH2vc/Yz+vcStZKVtoivF7Ee8IGVI55cUo1d/ZsHBWU4VWz+sf4
Z5np4LXRAgMBAAECggEBALW+ZaGoEvIdqGmAk3yzDzQqIoXAwDR8+V7HOgXkgYiT
MTApkHbqm/u/f+12V2zFJSKTaomnO50v97NvDt8wLvCiaIjo5mNZKG9M62EYk5Qs
Rcmsyasatbvb6A40JTn8L0+3fV4jm0xK0lwyQ5DFSlYQ7DppawNEma4qiie5Q3F5
N7Jro6sdFF7VxXq6KaMuTMARFAwlOj9jgvB8T+Ij2Ob2zKYBO+kvrbg53yCkkK7k
xDJ0KQl5nVus8SYW4ThNq1De0WRZEEfcXEGJNhEPBTYukCvqRbOAQvhivmU0IbtO
FBwwb/CDmJu9N+RYhIDZD1fY020RDR/Q1KQY7YSJZZkCgYEA7KHNi/4z1n+7pxej
D+PdETvy+tt5uGmt3YfhKBpQhoRmsM+jUI3SBP1ixz3HBnA3OV7Y9mGViSB+J4Vs
+Ulg2nO3tPOBBaDVIv778uxYs6JwdmGWmuf4PvKAaYhKc8ZNFdUg+W6j5vF0jpJx
W35va204kV01rVxXMYXc6ArWOB8CgYEA21z4Mahq90EyQKW13Zk+c/iBAR8z0WWc
ATfTeT55hiKjoTnw3doh6NkQxZPVGNS571MqON+YdrpNEa1aM5/L+vWtNKXe6GH5
ZcRMsu6OLxGnJZ2NidJaHu8fsHQwDIr1pNh8lXzOW+zwTOA+NIZbIIHhyP8cHJOC
jT93qRrvFA8CgYEAk/JqACAUKwU7vzHRGqs92Xug5GT6imlQclR1WsxxElmPlvaL
Rl8VM1d1PdsLJ6RbcrwxbIIAq3asSWtrgcT2ED8PWcxxG6K9/55qyiwpHU7vf6Ru
NDXaiQi3bsPuwMn4AjF551nckhTe6hD3XNwFL6ZfGsurNtOoFGd8gXvdCcMCgYAe
MdzkEPewUyQUq/qtpxNmqTUwr1fVofPoYwPT85qMsSXu4mfaUdVLFN3vJFs30MhL
PhpYu8ZjyV8kaMU6x+su/zQ5Q/+74wiOTof0NUwTBI4KanZ6Gx9WlLm68zNN9q67
YyU6+nP5SVMGcfN+DWRrxBUNMrhu4osc4DibaV6xVwKBgQCJWI439f/zaKX45l7a
FPw4fxai2wj7p/D4twG6cAjAcFGO2fuyWoB5pDVKn1t9c47ShP4YCs9JSnq+ORwq
NL7fFjBoXJwEdR4D+U4Z4SS/gs40Npv39DcaiyC6mW8gTcBkDWvdbAdCeOW75Lbt
lBQVRcOXZlMKNgVn0HcNRBczkw==
-----END PRIVATE KEY-----
`
}

const key2 = {
    private: `-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC+qMVxVQeQ5g5+
rM4FPn1sZNBNnWXLG2O7/nZZbgV9WvB5S8EWZgwBkkWP70uRHniaui9759jeUPeQ
Ow92+C2yXW+V2nnDwwPBlxu1itLtV9X7ZP9Vdq6CzP6Gz1c4rg5WiUQnkjG/tniK
zb0ctAZp7NTkKr+8AxHE1Zf/r+BwW+vlPAaqPXRb4goKDwdz1iEqL38RXwEy64pN
VbCgEZW6WQebtYolo6Xw/J0bZua82zz3NjLorMty+9x+euPrpbpH6p9qyPLxRZse
PIelmL43AFvpxHxkdus9RV10tND4PEVqxClFk2TfSv47qFwNeybUomxLWx0whOQs
ydx8PW/DAgMBAAECggEBAJ7b5NePb6Rt67nkpix8auj7/wwMj5pyEeG8HkRZhb1z
HlcSpanSEULl64wIaMBb0UWqyJHHFk32zK+iZjpMg1bQv37u8PIJ9YFCNNZI+4LC
4wp6lr1RDAr0X0dYT2ZssImuCdmm2EjS8xFEZ8QYxBdSO/h5HhS7wfxttB5ZHvq+
TB5OtIsS1sMF3jV4xYFFbyUWMCODo56FejCdZ+jHzdrHw5WH5YD/hNq3AV9HyO1F
3D4O7ZGtaC6BnRS5p75ISG7iaJxmmbu7kMDlP1jKzODtgaVMQ0LKF69tTCq4qaZw
XloYVQIgrgDtMr/twSYWg8s3FQBdiVkTCHigvs871jECgYEA5VCMYvHe2J3S8/Iu
fnjAusmY85orNXB5ppsy9XxJ9GfQ5UeJklvQufl87IhjVYs53daCv7di3JDj+9od
8MXD3Nw+Zs2HVbsUXDw0LQBwY/hCfu3+PAfa3+XDDYeP77Ndxt2m62DVn2RDIjO7
N3252nA1ybjKOmnz7p1XdiqHCE8CgYEA1NimlmNgbZo9GWWVOtHyDxApYXFQqP3b
anhCBslbRAkGEJ7PKCgDVQIHa4kZCa7nIhuAREe/7xvOG2bGv9KXxTcKM6aDrbjC
k0uUx/tEvGpxTanh8j5C+hxLGOpuMPF8Z9STrXDS9g0sPauHwvvKNz2fDNh7cZUk
ZyyPI/t1EE0CgYEAh7Zt/zjqDAULjfgscAkC9wjMjo8VN2FDfcioFgTit9ShGrCF
yKqyEv6GYm4ta7kkHBgR9WiRB7xwUOM7iP1h91oZTDoi+Cfp2tR+hIuX+HI62Gf9
aIW1kYCnOWCbXN8ohsMzQGZkxxpNaPZ4vFSaI575FlNYy81c4kEYPATbOZ0CgYBZ
kNl5C1R2iqMPXtnPKK2NfSWMpZxloH2GbhiKtoy7Mnjk4Y8WvM+6I+Jvc/+fPYRx
q9Tr5q1ztuWiTjtwWPQYvzKBzZ43qjnmtbVj+e5QwxHutoAn6d9RVDqdv3ijC9tn
7eI+5+0+MQje4qF8gWv0lu+o/ifkAfiRmgSaBaSbOQKBgCsylvb/Z43H9fMVqYPh
xtjTG2TzHJdAy0WK/2M/CTz+cI1O/8vu9LbJeIkJ510FIbBpOxpF645ZqIobBLuP
UlJj5ww25z6FDH+3CX0zuAR+8ofS2ac5d18di27TU/EAdLcJXvp7pwhQVo0RjCAa
UKG0lDSQbmonEkaWcGVujcfI
-----END PRIVATE KEY-----
`
}

const key3 = {
    private: `-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDONNz/dl0dGRRm
XyZ75hPguzA1YBKrmunyUz631tJknFm58fvtu0Is9ohfARRmNaVt2RYjxNNB38H4
1GyTYwl0za85IFQ4erlHDtCOlfRG7IEkYldg5PRKpves7dKTbWa4Oc5sgSq63QMo
XtlTY8p3qYrcFaZr75Ll29NZ3k9PJLcN/h70OwXLtKNRav4R5aUyrvmB8WtwOou4
MeLFBxYOSgDID4Dg5G0rBgKZB5Tul2gCU3OHPOjjBpeb1vJl5haDP6py1+kTgQcG
s+tdiNmLZ0zBTnFW/cyBRGPFC6rNRUqQfsu9I40FORWr5WXhz0MVRNpoVg1KrrA8
5I+m7gjzAgMBAAECggEBAJpHMOWtICNqJTomjHt8e3jTdFEq7S0ZPVnBcJZJEqaV
2AfoBY4rYWqUlRlHjISgNIJIPmlKlKifidfqzsVC/ss2LJg4GOsc3sxMSDu188qw
njqjnS5wflnwLDRkxvzoiEbjcQ77YQXn37nvpPfgHJaE8dCSHi55U1XIr4t4aawS
uzeheISXuY5U4a0aXPUJdDOiL2fLqhNR19OZlOmKKoNPnHa4JZcZH8LJeiEZZIwe
2hTlmkkOptX5U7qofd2jgmPMi6T/WuZOyPr4stD0ZEfvoe2R57wJMHQmcl7HGkiH
N01O7RR8d5X1Q3y0WLxg5kB6wI1/mrKzblw5tidyABkCgYEA7C6woldwNGbHyOpv
0VC/CQoJcw6fjD7IuV0d2z+QNQw1QSQSfLwzoEgplPzqgOcLeHfgHpDCcJjCKJRH
TEuPPl4I1diWWoZWCMgOPGbpz079wMrf1qjcj725vCztjP3sxGoXpR7hh7QlExna
l6N3Dz+FbaInpSaAkqTInGlLVYUCgYEA34JG6axNcNJUE7PZbm1SGsxaeTckezuC
1goaoZjMFxWgsq4uhRRHAiwBcf9l6vrAzrUaMCLGAeq7q/6zMhpE97Gwpi4X2ix8
0z78/VusD1MhEzn/XqpYU5aRlIaqq4JUH6YstIsBW89z3rkzTBYKYBdWvS5ZbTWU
unHIGW5uEhcCgYBmzGerMV2wITBB/fBPbdLIb2rvfmTpWQZqz+HH5SGyRS8EvFuj
mRLrZ51ZsL+s/WNbuul8xEstUP/pQ6jIx3aAkdNMqKnAmwnlZpXzqmOIDPybpnHI
79SYZ97ozbFTlc4xK3BBOXC2bS1Z4/3l1ItkwqQjtNmGo3yA12JN2fKlNQKBgQCU
8KOAo6kXPZF5b6e8xmyoHvVLQUu2MvoE18bXWJLn7yme6CNn2xNWhyyPYrnv1BkX
pedFnGaGcH2PJ7MR+B9tT6N/mcebQ3oK6zZ7PtNxgtvPjB54XtttGDD64KJql/an
AH1Z8id9nsh/6rocQbO4RNUwS+qbs9DgTWIiUExZEQKBgGWdMEPdNfZYYgNghy19
ZEDvjWh8KMbV18Y6qPkl4yRc8FKvqfSZvr0fDcYiwsnk+o7/ZhaBrd1P+wf3E4D8
O9fqjSQMm/LQJZlS3le0kAACKNmxKzU5y6R1YybGRgPpeqUXd9uDWXIF0+FA9e4X
GjoQK6ICGmkcwHL1dLWSbRNk
-----END PRIVATE KEY-----
`
}

function Cache() {
    this.get = function(id) {
        switch (id) {
            case 'key1':
                return key1.private;
            case 'key2':
                return key2.private;
            case 'key3':
                return key3.private;
            default:
                return null;
        }
    }

    this.list = function() {
        return ['key1', 'key2', 'key3'];
    }
}

module.exports = {
    Cache: Cache
}