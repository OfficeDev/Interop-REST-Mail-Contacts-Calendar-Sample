//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Events;
using System;

namespace MeetingManager
{
    public class Logger
    {
        private readonly IEventAggregator _eventAggregator;

        public Logger(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void Initialize(string fileName)
        {
        }

        public void LogHttp(string method, string uri, string requestBody, string requestHeaders,
                    string statusCode, string responseBody, string responseHeaders)
        {
            _eventAggregator.GetEvent<HttpEvent>().Publish(new HttpEventData
            {
                TimeStamp = DateTimeOffset.Now,
                Method = method,
                Uri = uri,
                RequestBody = requestBody,
                RequestHeaders = requestHeaders,
                StatusCode = statusCode,
                ResponseBody = responseBody,
                ResponseHeaders = responseHeaders
            });
        }
    }
}
