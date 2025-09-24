namespace AdPlatforms.Common.Results.Exceptions;

public class NullValueException(string? message = null) : ResultException(message);