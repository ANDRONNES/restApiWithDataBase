﻿namespace Tutorial8.Exceptions;

public class InternalServerErrorException : Exception
{
    public InternalServerErrorException(string message):base(message){}
}