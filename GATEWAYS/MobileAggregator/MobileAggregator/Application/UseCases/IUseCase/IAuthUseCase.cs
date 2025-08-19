using MobileAggregator.Application.UseCases.UseCaseIn;
using MobileAggregator.Application.UseCases.UseCaseOut;

namespace MobileAggregator.Application.UseCases.IUseCase;

public interface IAuthUseCase
{
    Task<LoginUseCaseOut> LoginAsync(LoginUseCaseIn request);
    Task<RefreshTokenUseCaseOut> RefreshTokenAsync(RefreshTokenUseCaseIn request);
    Task<ChangePasswordUseCaseOut> ChangePasswordAsync(ChangePasswordUseCaseIn request);
    Task<ForgotPasswordUseCaseOut> ForgotPasswordAsync(ForgotPasswordUseCaseIn request);
}
