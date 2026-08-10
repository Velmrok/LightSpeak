namespace Common.Dto;
public record ErrorResponse(string Code, string Details, IEnumerable<ErrorItem>? Errors = null);

public record ErrorItem(string Section, string Code, string Details);