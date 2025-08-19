namespace MobileAggregator.Application.UseCases.UploadReceiptUseCase;

public interface IUploadReceiptUseCase
{
    Task<UploadReceiptUseCaseOut> ExecuteAsync(UploadReceiptUseCaseIn request);
}
