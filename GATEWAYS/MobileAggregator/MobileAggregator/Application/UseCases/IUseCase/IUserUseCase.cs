using MobileAggregator.Application.UseCases.UseCaseIn;
using MobileAggregator.Application.UseCases.UseCaseOut;

namespace MobileAggregator.Application.UseCases.IUseCase;

public interface IUserUseCase
{
    Task<CreateUserUseCaseOut> CreateUserAsync(CreateUserUseCaseIn request);
    Task<GetUserUseCaseOut> GetUserAsync(GetUserUseCaseIn request);
    Task<UpdateUserUseCaseOut> UpdateUserAsync(UpdateUserUseCaseIn request);
    Task<DeleteUserUseCaseOut> DeleteUserAsync(DeleteUserUseCaseIn request);
    Task<ListUsersUseCaseOut> ListUsersAsync(ListUsersUseCaseIn request);
}
