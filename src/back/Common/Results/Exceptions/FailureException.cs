namespace AdPlatforms.Back.Common.Results.Exceptions;

public class FailureException(string? msg = null) : ResultException(msg);