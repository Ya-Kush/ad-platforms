namespace AdPlatforms.Back.Common.Results.Exceptions;

public class ResultException(string? msg = null, Exception? inner = null) : ApplicationException(msg, inner);