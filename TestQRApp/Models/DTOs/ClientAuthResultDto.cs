namespace TestQRApp;

public record ClientAuthResultDto(bool IsSuccess, string ErrorMessage, Guid? ClientId);
