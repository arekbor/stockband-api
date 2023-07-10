using FluentValidation.Results;
using MediatR;
using Stockband.Application.Interfaces.FeatureServices;
using Stockband.Application.Interfaces.Repositories;
using Stockband.Application.Interfaces.Services;
using Stockband.Domain;
using Stockband.Domain.Common;
using Stockband.Domain.Entities;
using Stockband.Domain.Exceptions;

namespace Stockband.Application.Features.UserFeatures.Commands.UpdatePassword;

public class UpdatePasswordCommandHandler:IRequestHandler<UpdatePasswordCommand, BaseResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserFeaturesService _userFeaturesService;
    private readonly IAuthenticationUserService _authenticationUserService;
    public UpdatePasswordCommandHandler(
        IUserRepository userRepository,
        IUserFeaturesService userFeaturesService, 
        IAuthenticationUserService authenticationUserService)
    {
        _userRepository = userRepository;
        _userFeaturesService = userFeaturesService;
        _authenticationUserService = authenticationUserService;
    }
    public async Task<BaseResponse> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        UpdatePasswordCommandValidator validator = new UpdatePasswordCommandValidator();
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new BaseResponse(validationResult);
        }

        int currentUserId = _authenticationUserService.GetCurrentUserId();
        
        User? user = await _userRepository.GetByIdAsync(currentUserId);
        if (user == null)
        {
            return new BaseResponse(new ObjectNotFound(typeof(User), currentUserId), 
                BaseErrorCode.RequestedUserNotExists);
        }
        
        if (!_userFeaturesService.VerifyHashedPassword(request.CurrentPassword, user.Password))
        {
            return new BaseResponse(new UnauthorizedOperationException(), 
                BaseErrorCode.UserUnauthorizedOperation);
        }
        
        user.Password = _userFeaturesService.HashPassword(request.NewPassword);

        await _userRepository.UpdateAsync(user);
        return new BaseResponse();
    }
}