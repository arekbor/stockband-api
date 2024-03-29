using MediatR;
using Stockband.Application.Interfaces.ExternalServices;
using Stockband.Application.Interfaces.FeatureServices;
using Stockband.Application.Interfaces.Repositories;
using Stockband.Domain.Common;
using Stockband.Domain.Entities;
using Stockband.Domain.Enums;
using Stockband.Domain.Exceptions;

namespace Stockband.Application.Features.UserFeatures.Commands.RevokeToken;

public class RevokeTokenCommandHandler:IRequestHandler<RevokeTokenCommand, BaseResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthenticationUserService _authenticationUserService;
    private readonly IConfigurationHelperService _configurationHelperService;
    private readonly IUserFeaturesService _userFeaturesService;

    public RevokeTokenCommandHandler(
        IRefreshTokenService refreshTokenService, 
        IRefreshTokenRepository refreshTokenRepository, 
        IAuthenticationUserService authenticationUserService,
        IUserFeaturesService userFeaturesService, 
        IConfigurationHelperService configurationHelperService)
    {
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _authenticationUserService = authenticationUserService;
        _userFeaturesService = userFeaturesService;
        _configurationHelperService = configurationHelperService;
    }

    public async Task<BaseResponse> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        BaseResponse<string> tokenResponse = _userFeaturesService
            .GetRefreshTokenFromSource(request.RefreshToken, request.Cookie);
        if (!tokenResponse.Success)
        {
            return new BaseResponse(tokenResponse.Errors);
        }
        
        RefreshToken? refreshToken = await _refreshTokenRepository
            .GetRefreshTokenByToken(tokenResponse.Result);
        if (refreshToken == null)
        {
            return new BaseResponse(new ObjectNotFound(typeof(RefreshToken), request.RefreshToken),
                BaseErrorCode.RefreshTokenNotFound);
        }

        if (!refreshToken.IsActive)
        {
            return new BaseResponse(new PerformRestrictedOperationException(), 
                BaseErrorCode.InvalidRefreshToken);
        }

        string ipAddress = _authenticationUserService.GetUserIp();
        await _refreshTokenService
            .RevokeRefreshToken(refreshToken, ipAddress, RefreshTokenReasonRevoke.Manual);
        
        string cookieName = _configurationHelperService.GetRefreshTokenCookieName();
        _authenticationUserService.InvalidateCookie(cookieName);

        return new BaseResponse();
    }
}