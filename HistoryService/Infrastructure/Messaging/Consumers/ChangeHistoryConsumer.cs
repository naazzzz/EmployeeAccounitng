using AutoMapper;
using General.Event.ChangeHistoryService;
using HistoryService.Core.Domain.Entity;
using HistoryService.Core.Interfaces;
using MassTransit;

namespace HistoryService.Infrastructure.Messaging.Consumers;

public class ChangeHistoryConsumer: IConsumer<SaveChangeTrackerHistoryEvent>
{
    private readonly IChangeHistoryRepository _changeHistoryRepository;
    private readonly ApplicationContext _applicationContext; 
    private readonly ILogger<ChangeHistoryConsumer> _logger;
    private readonly IMapper _mapper;
    
    public ChangeHistoryConsumer(
        IChangeHistoryRepository changeHistoryRepository,
        ILogger<ChangeHistoryConsumer> logger,
        IMapper mapper, ApplicationContext applicationContext)
    {
        _changeHistoryRepository = changeHistoryRepository;
        _logger = logger;
        _mapper = mapper;
        _applicationContext = applicationContext;
    }
    
    public async Task Consume(ConsumeContext<SaveChangeTrackerHistoryEvent> context)
    {
        try
        {
            var changeHistory = _mapper.Map<ChangeHistory>(context.Message.ChangeHistoryDto);
            
            await _applicationContext.ChangeHistory.AddAsync(changeHistory);
            await _applicationContext.SaveChangesAsync();
            
            _logger.LogInformation(
                "Change history added for model {EntityName} with id {EntityId}",
                context.Message.ChangeHistoryDto.EntityName,
                context.Message.ChangeHistoryDto.EntityId);     
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}