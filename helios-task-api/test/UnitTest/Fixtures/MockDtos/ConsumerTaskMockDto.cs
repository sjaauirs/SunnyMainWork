using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class ConsumerTaskMockDto: ConsumerTaskDto
    {
        public ConsumerTaskMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerTaskId = 203;
            Progress = 0;
            Notes = "done";
            TaskStartTs = DateTime.UtcNow;
            TaskCompleteTs = DateTime.UtcNow;
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskStatus = "completed";
            TaskId = 5;
            CreateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            
        }
    }

    public class UpdateConsumerTaskMockDto : UpdateConsumerTaskDto
    {
        public UpdateConsumerTaskMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerTaskId = 203;
            Progress = 0;
            Notes = "done";
            TaskStartTs = DateTime.UtcNow;
            TaskCompleteTs = DateTime.UtcNow;
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskStatus = "completed";
            TaskId = 5;
            CreateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            TaskCode = "tsk-1234";
        }
    }

    public class UpdateConsumerTaskStatusMockDto : UpdateConsumerTaskDto
    {
        public UpdateConsumerTaskStatusMockDto()
        {
            ConsumerTaskId = 203;
            TaskId = 0;
            TaskStatus = "InProgress";
            Progress = 50;
            Notes = "Halfway through the task";
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            TaskStartTs = DateTime.UtcNow;
            TaskCompleteTs = DateTime.UtcNow.AddHours(2);
            AutoEnrolled = true;
            ProgressDetail = "Details of progress";
            ParentConsumerTaskId = 98765;
            TaskCode = "TASK001";
            TaskExternalCode = "tsk-1234";
            SpinWheelTaskEnabled = true;
            CreateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            
        }
    }

}
