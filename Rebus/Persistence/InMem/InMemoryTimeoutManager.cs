﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Extensions;
using Rebus.Messages;
using Rebus.Time;
using Rebus.Timeouts;

namespace Rebus.Persistence.InMem
{
    public class InMemoryTimeoutManager : ITimeoutManager
    {
        readonly ConcurrentDictionary<string, DeferredMessage> _deferredMessages = new ConcurrentDictionary<string, DeferredMessage>();

        public async Task Defer(DateTimeOffset approximateDueTime, Dictionary<string, string> headers, byte[] body)
        {
            _deferredMessages
                .AddOrUpdate(headers.GetValue(Headers.MessageId),
                    id => new DeferredMessage(approximateDueTime, headers, body),
                    (id, existing) => existing);
        }

        public async Task<DueMessagesResult> GetDueMessages()
        {
            lock (_deferredMessages)
            {
                var keyValuePairsToRemove = _deferredMessages
                    .Where(v => RebusTime.Now >= v.Value.DueTime)
                    .ToList();

                var result = new DueMessagesResult(keyValuePairsToRemove
                    .Select(kvp => new DueMessage(kvp.Value.Headers, kvp.Value.Body)));

                foreach (var kvp in keyValuePairsToRemove)
                {
                    DeferredMessage _;
                    _deferredMessages.TryRemove(kvp.Key, out _);
                }

                return result;
            }
        }

        class DeferredMessage
        {
            public DateTimeOffset DueTime { get; private set; }
            public Dictionary<string, string> Headers { get; private set; }
            public byte[] Body { get; private set; }

            public DeferredMessage(DateTimeOffset dueTime, Dictionary<string, string> headers, byte[] body)
            {
                DueTime = dueTime;
                Headers = headers;
                Body = body;
            }
        }
    }
}