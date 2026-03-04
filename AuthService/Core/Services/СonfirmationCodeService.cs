using System.Text.Json;
using AuthService.Core.Entities;
using AuthService.Core.Interfaces;
using AuthService.Core.Interfaces.Repositories;
using AuthService.Core.Interfaces.Services;
using AutoMapper;
using General.Event.ChangeHistoryService;
using General.Event.NotificationService;
using General.Exceptions;
using MassTransit;

namespace AuthService.Core.Services;

public class СonfirmationCodeService : IСonfirmationCodeService
{
    private readonly ILogger<СonfirmationCodeService> _logger;
    private readonly IСonfirmationCodeRepository _сonfirmationCodeRepository;
    private readonly IPublishEndpoint _publishedEndpoint;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRequestClient<ChangeHistoryRequest> _requestClient;
    private readonly IRequestClient<ChangeHistoryRequestLatestByUserId> _requestClientForLatest;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;

    public СonfirmationCodeService(
        IСonfirmationCodeRepository сonfirmationCodeRepository,
        ILogger<СonfirmationCodeService> logger,
        IPublishEndpoint publishedEndpoint,
        IServiceScopeFactory scopeFactory,
        IRequestClient<ChangeHistoryRequest> requestClient,
        IMapper mapper,
        IUserService userService,
        IUserRepository userRepository,
        IRequestClient<ChangeHistoryRequestLatestByUserId> requestClientForLatest)
    {
        _сonfirmationCodeRepository = сonfirmationCodeRepository;
        _logger = logger;
        _publishedEndpoint = publishedEndpoint;
        _scopeFactory = scopeFactory;
        _requestClient = requestClient;
        _mapper = mapper;
        _userService = userService;
        _userRepository = userRepository;
        _requestClientForLatest = requestClientForLatest;
    }

    public async Task FindAndAcceptCode(string code, string userId)
    {
        var mailCode = _сonfirmationCodeRepository.GetMailCodeByCodeAndUserId(code, userId);
        if (mailCode == null)
        {
            throw new RecordNotFoundException(code, "MailCode");
        }

        var user = await _userService.GetById(userId);
        if (user == null)
        {
            throw new RecordNotFoundException(userId, "User");
        }

        var response =
            await _requestClient.GetResponse<ChangeHistoryResponse>(new ChangeHistoryRequest(mailCode.ChangeHistoryId));
        if (response.Message == null)
        {
            _logger.LogWarning($"ChangeHistory response null on request by id - {mailCode.ChangeHistoryId}");
            throw new Exception($"ChangeHistory response null on request by id - {mailCode.ChangeHistoryId}");
        }

        var updatedUserData = JsonSerializer.Deserialize<Entities.User>(response.Message.UserChangeHistoryDto.ValueAction);
        if (updatedUserData == null)
        {
            throw new Exception("Can't deserialize to updated User");
        }

        _mapper.Map(updatedUserData, user);
        user.IsChangeConfirmed = true;
        await _userRepository.SaveChanges();
    }

    public async Task CreateNewCodeForUserByRefreshCode(string userId, string? refreshCode = null)
    {
        var user = await _userService.GetById(userId);
        if (user == null)
        {
            throw new RecordNotFoundException(userId, "User");
        }

        string changeHistoryId = null!;

        if (refreshCode != null)
        {
            var mailCode = _сonfirmationCodeRepository.GetMailCodeByRefreshCodeAndUserId(refreshCode, userId);
            if (mailCode == null)
            {
                throw new RecordNotFoundException(refreshCode, "MailCode");
            }

            changeHistoryId = mailCode.ChangeHistoryId;
        }
        else
        {
            var response =
                await _requestClientForLatest.GetResponse<ChangeHistoryResponse>(
                    new ChangeHistoryRequestLatestByUserId(userId));
            if (response.Message == null)
            {
                _logger.LogWarning($"ChangeHistory latest response null on request by user_id - {user!.Id}");
                throw new Exception($"ChangeHistory latest response null on request by user_id - {user.Id}");
            }

            changeHistoryId = response.Message.UserChangeHistoryDto.Id;
        }

        var code = new ConfirmationCode
        {
            UserId = userId,
            ChangeHistoryId = changeHistoryId ?? throw new Exception("Change history id must be not null")
        };

        await _сonfirmationCodeRepository.AddWithSaveAsync(code);

        await _publishedEndpoint.Publish(new CodeSentEvent()
        {
            Code = code.Code,
            UserName = user!.UserName!,
            Email = user.Email!,
        });
    }

    public void RemoveExpiredCodes()
    {
        var changeCount = _сonfirmationCodeRepository.RemoveExpiredCodes();
        _logger.LogInformation($"The number of successfully remote expired codes: {changeCount}");
    }

    public async Task<object> CreateNewCode(string userId, string userChangeHistoryId)
    {
        var user = await _userService.GetById(userId);
        if (user == null)
        {
            throw new RecordNotFoundException(userId, "User");
        }

        var code = new ConfirmationCode
        {
            UserId = userId,
            ChangeHistoryId = userChangeHistoryId
        };

        await _сonfirmationCodeRepository.AddWithSaveAsync(code);

        return new
        {
            Code = code.Code,
            UserName = user.UserName,
            Email = user.Email,
        };
    }
}