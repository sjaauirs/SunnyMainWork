using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class TransactionBySectionResponseMockDto : TransactionBySectionResponseDto
    {
        public TransactionBySectionResponseMockDto()
        {
            Transaction = new Dictionary<string, List<TransactionEntryDto>>
            {
                { "DefaultKey", new List<TransactionEntryDto>
                    {
                        new TransactionEntryDto
                        {
                            Transaction = new TransactionDto
                            {
                                    TransactionId = 5,
                                    WalletId = 3,
                                    TransactionCode = "ten67766mhggh",
                                    TransactionType = "update",
                                    PreviousBalance = 800,
                                    TransactionAmount = 100,
                                    Balance = 400,
                                    PrevWalletTxnCode = "pre455rfmmfmg-67",
                                    TransactionDetailId = 45,
                                    CreateTs = DateTime.Now

                            },

                            TransactionDetail = new TransactionDetailDto
                            {
                                 TransactionDetailId = 1,
                                 TransactionDetailType = "sunny",
                                 ConsumerCode = "c457c5257c59451d8a93ea941a9f2e0a",
                                 TaskRewardCode = "Tas567565kb54",
                                 Notes = "five",
                                 RedemptionRef = "success",
                                 RedemptionItemDescription = "update task",
                                 RewardDescription="update description ",
                                 IsSpouse =true,
                                 IsDependent =true,
                                 CreateTs = DateTime.Now
    }

                        }
                    }
                }
            };
            TaskReward = new Dictionary<string, IEnumerable<TaskRewardDetailDto>>
            {
                { "DefaultKey",  new List<TaskRewardDetailDto>
                    {
                        new TaskRewardDetailDto
                        {
                            Task = new TaskDto
                            {
                                TaskId =2,
                                TaskTypeId =2,
                                TaskCode ="tas46543",
                                TaskName ="sunny reward",
                                SelfReport=true,
                                ConfirmReport =true,
                                TaskCategoryId =2,
                                IsSubtask =true
                            },

                            TaskReward= new TaskRewardDto
                            {
                                TaskId =8,
                                TaskRewardId =2,
                                RewardTypeId =4,
                                TenantCode="ten-ecada21e57154928a2bb959e8365b8b4",
                                TaskRewardCode="tas-928a2bb959e836",
                                TaskActionUrl ="sunny",
                                Reward ="sunny",
                                Priority =232,
                                Expiry =DateTime.Now,
                                MinTaskDuration =12,
                                MaxTaskDuration =10,
                                TaskExternalCode ="ext-928a2bb959e836",
                                ValidStartTs=DateTime.Now,
                                IsRecurring =true,
                                RecurrenceDefinitionJson ="recurrent reward"
                            },
                            TaskDetail = new TaskDetailDto
                            {
                                TaskId=2 ,
                                TaskDetailId =2,
                                TermsOfServiceId =5,
                                TaskHeader="sunny header",
                                TaskDescription ="description ",
                                LanguageCode ="lan-928a2bb959e836",
                                TenantCode ="ten-ecada21e57154928a2bb959e8365b8b4",
                                TaskCtaButtonText ="text"
                            },
                            TermsOfService= new TermsOfServiceDto
                            {
                                TermsOfServiceId=4,
                                TermsOfServiceText="service-text",
                                LanguageCode ="lan-928a2bb959e836"
                             },

                            TenantTaskCategory = new TenantTaskCategoryDto
                            {
                                TenantTaskCategoryId =6,
                                TaskCategoryId =5,
                                TenantCode ="ten-ecada21e57154928a2bb959e8365b8b4",
                                ResourceJson ="resource-sunny"
                            },

                            TaskType= new TaskTypeDto
                            {
                                TaskTypeId =8,
                                TaskTypeCode ="tas-928a2bb959e836",
                                TaskTypeName="sunny" ,
                                TaskTypeDescription ="sunny type"
                            },

                            RewardTypeName = "rjkgdr",
                        }
                    }
                }

            };
        }

    }
}





