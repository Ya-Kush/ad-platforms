namespace AdPlatforms.Back.Models.Exceptions;

public class ModelException(string? msg = null, Exception? inner = null) : ApplicationException(msg, inner);