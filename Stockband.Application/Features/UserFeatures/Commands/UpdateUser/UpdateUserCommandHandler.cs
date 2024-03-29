using MediatR;
using Stockband.Application.Interfaces.FeatureServices;
using Stockband.Application.Interfaces.Repositories;
using Stockband.Application.Interfaces.ExternalServices;
using Stockband.Domain.Enums;
using Stockband.Domain.Common;
using Stockband.Domain.Entities;
using Stockband.Domain.Exceptions;

namespace Stockband.Application.Features.UserFeatures.Commands.UpdateUser;

public class UpdateUserCommandHandler:IRequestHandler<UpdateUserCommand, BaseResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserFeaturesService _userFeaturesService;
    private readonly IAuthenticationUserService _authenticationUserService;
    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUserFeaturesService userFeaturesService, 
        IAuthenticationUserService authenticationUserService)
    {
        _userRepository = userRepository;
        _userFeaturesService = userFeaturesService;
        _authenticationUserService = authenticationUserService;
    }
    public async Task<BaseResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (!_authenticationUserService.IsAuthorized(request.UserId))
        {
            return new BaseResponse(new UnauthorizedOperationException(),
                BaseErrorCode.UserUnauthorizedOperation);
        }
        
        User? user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return new BaseResponse(new ObjectNotFound(typeof(User), request.UserId), 
                BaseErrorCode.UserNotFound);
        }

        int currentUserId = _authenticationUserService.GetUserId();

        if (await _userFeaturesService.IsEmailAlreadyUsed(request.Email) && user.Id != currentUserId)
        {
            return new BaseResponse(new ObjectAlreadyCreatedException(typeof(User), request.Email), 
                BaseErrorCode.UserEmailAlreadyExists);
        }

        user.Email = request.Email;
        user.Username = request.Username;

        await _userRepository.UpdateAsync(user);

        return new BaseResponse();
    }
}