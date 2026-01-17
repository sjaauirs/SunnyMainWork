using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class ConsumerTaskMockModel : ConsumerTaskModel
    {
        public ConsumerTaskMockModel()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerTaskId = 203;
            Progress = 0;
            Notes = "done";
            TaskStartTs = DateTime.UtcNow;
            TaskCompleteTs = DateTime.UtcNow.AddYears(-1);
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskStatus = "COMPLETED";
            TaskId = 2;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
        }
        public static List<ConsumerTaskModel> consumerData()
        {
            return new List<ConsumerTaskModel>()
            {
                new ConsumerTaskMockModel(),
                new ConsumerTaskMockModel
               {
                    TaskStatus = "COMPLETED",
                    ConsumerTaskId = 204, 
                },
                new ConsumerTaskMockModel
                {
                    TaskStatus = "IN_PROGRESS",
                    ConsumerTaskId = 205, 
                },
            };
        }

    }


    public class ConsumerTaskIdMockModel : ConsumerTaskModel
    {
        public ConsumerTaskIdMockModel()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerTaskId = 203;
            Progress = 0;
            Notes = "done";
            TaskStartTs = DateTime.UtcNow;
            TaskCompleteTs = DateTime.UtcNow.AddMonths(-1);
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskStatus = "";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
        }


        public static List<ConsumerTaskModel> consumIdData()
        {
            return new List<ConsumerTaskModel>()
            {
                new ConsumerTaskIdMockModel()
            };
        }

    }

    public class ConsumerTaskIdZeroMockModel : ConsumerTaskModel
    {
        public ConsumerTaskIdZeroMockModel()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerTaskId = 0;
            Progress = 0;
            Notes = "done";
            TaskStartTs = DateTime.UtcNow;
            TaskCompleteTs = DateTime.UtcNow.AddMonths(-1);
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskStatus = "COMPLETED";
            TaskId = 0;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            ProgressDetail = "sunnyDetails";
        }
        public static List<ConsumerTaskModel> consumerTaskData()
        {
            return new List<ConsumerTaskModel>()
            {
                new ConsumerTaskIdZeroMockModel()
            };
        }
        
    }
}
