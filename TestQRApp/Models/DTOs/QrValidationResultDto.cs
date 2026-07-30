namespace TestQRApp;

public record QrValidationResultDto(bool IsValid, string ErrorMessage, Guid? ClientId);
