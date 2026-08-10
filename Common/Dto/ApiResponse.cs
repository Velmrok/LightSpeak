
namespace Common.Dto;

public record ApiResponse<T>(T? Data, IEnumerable<ErrorItem>? Errors = null);