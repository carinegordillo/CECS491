﻿using System;

public class JwtHeader
{
    public string Alg { get; set; } = "HS256";
    public string Typ { get; set; } = "JWT";
}
