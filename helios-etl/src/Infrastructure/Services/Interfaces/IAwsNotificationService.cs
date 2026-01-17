using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IAwsNotificationService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="snsMessage"></param>
        /// <returns></returns>
        Task<(bool, string)> PushNotificationToAwsTopic(AwsSnsMessage snsMessage, string topicName, bool isFifo = false, string messageGroupId = "", string deDuplicationId = "");
        Task<(bool, string)> PushNotificationBatchToAwsTopic(List<AwsSnsMessage> snsMessages, string topicName, bool isFifo = false, string messageGroupId = "", string deDuplicationPrefix = "");
    }
}
