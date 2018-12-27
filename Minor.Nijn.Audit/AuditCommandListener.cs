using Microsoft.Extensions.Logging;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Entities;
using Minor.Nijn.Audit.Models;
using Minor.Nijn.WebScale.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit
{
    [CommandListener]
    public class AuditCommandListener
    {
        private readonly ILogger _logger;
        private readonly IAuditMessageDataMapper _dataMapper;
        private readonly IEventReplayer _replayer;

        public AuditCommandListener(IAuditMessageDataMapper dataMapper, IEventReplayer replayer, ILoggerFactory logger)
        {
            _dataMapper = dataMapper;
            _replayer = replayer;
            _logger = logger.CreateLogger<AuditCommandListener>();
        }

        [Command(Constants.AuditCommandQueueName)]
        public async Task<ReplayEventsCommandResult> HandleReplayEventCommands(ReplayEventsCommand request)
        {
            _logger.LogInformation(
                "Received replay command with correlationId: {0} and criteria(fromTimestamp={1}, ToTimestamp={2}, EventType={4}, RoutingKeyExpression={5})", 
                request.CorrelationId,
                request.FromTimestamp,
                request.ToTimestamp,
                request.EventType,
                request.RoutingKeyExpression
            );

            var criteria = new AuditMessageCriteria
            {
                FromTimestamp = request.FromTimestamp,
                ToTimestamp = request.ToTimestamp,
                EventType = request.EventType,
                RoutingKeyExpression = request.RoutingKeyExpression
            };

            var auditMessages = await _dataMapper.FindAuditMessagesByCriteriaAsync(criteria);

            var task = new Task(() => ReplayMessages(request, auditMessages));
            task.Start();

            var numberOfMessages = auditMessages.Count();
            _logger.LogInformation("Sending {0} event messages to {1} exchange", numberOfMessages, request.ExchangeName);

            return new ReplayEventsCommandResult
            {
                ExchangeName = request.ExchangeName,
                StartTimestamp = DateTime.Now.Ticks,
                NumberOfEvents = numberOfMessages
            };
        }

        public void ReplayMessages(ReplayEventsCommand request, IEnumerable<AuditMessage> auditMessages)
        {
            _replayer.DeclareExchange(request.ExchangeName);

            foreach (var message in auditMessages)
            {
                _replayer.ReplayAuditMessage(message);
            }
        }
    }
}