using AutoMapper;
using General.Dto;
using General.Event.AuthService;
using General.Event.ChangeHistoryService;
using HistoryService.Core.Domain.Entity;
using HistoryService.Core.Interfaces;
using MassTransit;

namespace HistoryService.Infrastructure.Messaging.Consumers;

public class UserChangeHistoryConsumer : IConsumer<ChangeHistoryCreateByUserIdRequest>, IConsumer<ChangeHistoryRequest>, IConsumer<ChangeHistoryRequestLatestByUserId>
{
    private readonly IUserChangeHistoryRepository _userChangeHistoryRepository;
    private readonly ILogger<UserChangeHistoryConsumer> _logger;
    private readonly IMapper _mapper;

    public UserChangeHistoryConsumer( 
        ILogger<UserChangeHistoryConsumer> logger,
        IMapper mapper,
        IUserChangeHistoryRepository userChangeHistoryRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _userChangeHistoryRepository = userChangeHistoryRepository;
    }

    public async Task Consume(ConsumeContext<ChangeHistoryCreateByUserIdRequest> context)
    {
        try
        {
            _logger.LogInformation("Received request for user {UserId}", context.Message.UserChangeHistoryDto.UserId);

            var changeHistory = _mapper.Map<UserChangeHistory>(context.Message.UserChangeHistoryDto);
            changeHistory.Id = Guid.NewGuid().ToString();
            changeHistory.CreatedAt = DateTimeOffset.UtcNow;

            changeHistory = await _userChangeHistoryRepository.AddWithSaveAsync(changeHistory);
            if (changeHistory == null)
            {
                _logger.LogError("Change history is null");
                return;
            }

            if (context.Message.UserChangeHistoryDto.NeedCode)
            {
                _logger.LogInformation("Publishing CreateConfirmationCode for user {UserId}, Type: {Type}", 
                    changeHistory.UserId, typeof(CreateConfirmationCodeEvent).FullName);
                await context.Publish(new CreateConfirmationCodeEvent
                {
                    UserId = changeHistory.UserId,
                    UserChangeHistoryId = changeHistory.Id
                });
                _logger.LogInformation("CreateConfirmationCode published for user {UserId}", changeHistory.UserId);
                Console.WriteLine("CreateConfirmationCode message was sent");
            }
            _logger.LogInformation("ChangeHistoryConsumer processing completed for user {UserId}", 
                context.Message.UserChangeHistoryDto.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ChangeHistoryCreateByUserIdRequest for user {UserId}",
                context.Message.UserChangeHistoryDto.UserId);
            throw;
        }
    }
    
    
    public async Task Consume(ConsumeContext<ChangeHistoryRequest> context)
    {
        _logger.LogInformation("Get ChangeHistory by id - {ChangeHistoryId}", context.Message.ChangeHistoryId);

        var changeHistory = await _userChangeHistoryRepository.GetByIdAsync(context.Message.ChangeHistoryId);
        if (changeHistory == null)
        {
            _logger.LogError("Change history with id - {ChangeHistoryId} not found", context.Message.ChangeHistoryId);
            return;
        }
        
        await context.RespondAsync(new ChangeHistoryResponse(_mapper.Map<UserChangeHistoryDto>(changeHistory)));
    }

    public async Task Consume(ConsumeContext<ChangeHistoryRequestLatestByUserId> context)
    {
        _logger.LogInformation("Get ChangeHistory by user_id - {ChangeHistoryId}", context.Message.UserId);

        var changeHistory = await _userChangeHistoryRepository.GetLastChanges(context.Message.UserId);
        if (changeHistory == null)
        {
            _logger.LogError("Change history by user_id - {User_id} not found", context.Message.UserId);
            return;
        }
        
        await context.RespondAsync(new ChangeHistoryResponse(_mapper.Map<UserChangeHistoryDto>(changeHistory)));
    }
}