using Microsoft.Extensions.Logging;
using Moq;
using NHibernate.Linq;
using NHibernate.Type;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Xunit;
using System.Threading.Tasks;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    
    public class TestingQueryable<T> : IOrderedQueryable<T>
    {
        private readonly IQueryable<T> _queryable;

        public TestingQueryable(IQueryable<T> queryable)
        {
            _queryable = queryable;
            Provider = new TestingQueryProvider<T>(_queryable);
        }

        public Type ElementType => _queryable.ElementType;

        public Expression Expression => _queryable.Expression;

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }
    }

    public class TestingQueryProvider<T> : INhQueryProvider
    {
        public TestingQueryProvider(IQueryable<T> source)
        {
            Source = source;
        }

        public IQueryable<T> Source { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestingQueryable<TElement>(Source.Provider.CreateQuery<TElement>(expression));
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return Source.Provider.Execute<TResult>(expression);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return  System.Threading.Tasks.Task.FromResult(Execute<TResult>(expression));
        }

        public int ExecuteDml<T1>(QueryMode queryMode, Expression expression)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteDmlAsync<T1>(QueryMode queryMode, Expression expression, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IFutureEnumerable<TResult> ExecuteFuture<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IFutureValue<TResult> ExecuteFutureValue<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public void SetResultTransformerAndAdditionalCriteria(IQuery query, NhLinqExpression nhExpression, IDictionary<string, Tuple<object, IType>> parameters)
        {
            throw new NotImplementedException();
        }
    }
    public class TaskRewardRepoUnitTest
    {
        private readonly Mock<ILogger<BaseRepo<TaskRewardModel>>> _loggerMock;
        private readonly Mock<ISession> mockSession;
        private readonly TaskRewardRepo _repo;
        private readonly Mock<IMapper> _mapper;
        public TaskRewardRepoUnitTest()
        {
            _loggerMock = new Mock<ILogger<BaseRepo<TaskRewardModel>>>();
             mockSession = new Mock<NHibernate.ISession>();
            _mapper = new Mock<IMapper>();
            _repo = new TaskRewardRepo(_loggerMock.Object, mockSession.Object,_mapper.Object);
        }
        [Fact]
        public async void GetTaskRewardDetails_ReturnsPreferredLanguage_WhenAvailable()
        {
            // Arrange
            var tenantCode = "TENANT1";
            var languageCode = "fr-FR";
            var fallbackLanguage =  "en-US";

            var taskId = 1;

            var taskRewards = new List<TaskRewardModel>
        {
            new TaskRewardModel { TaskId = taskId, TenantCode = tenantCode, TaskExternalCode = "EXT123", DeleteNbr = 0 }
        }.AsQueryable();

            var tasks = new List<TaskModel>
        {
            new TaskModel { TaskId = taskId, DeleteNbr = 0 }
        }.AsQueryable();

            var taskDetails = new List<TaskDetailModel>
        {
            new TaskDetailModel { TaskId = taskId, TenantCode = tenantCode, LanguageCode = languageCode, DeleteNbr = 0 },
            new TaskDetailModel { TaskId = taskId, TenantCode = tenantCode, LanguageCode = fallbackLanguage, DeleteNbr = 0 }
        }.AsQueryable();
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            // Setup queryables to simulate NHibernate IQueryable
            mockSession.Setup(s => s.Query<TaskRewardModel>()).Returns(new TestingQueryable<TaskRewardModel>(taskRewards));
            mockSession.Setup(s => s.Query<TaskModel>()).Returns(new TestingQueryable<TaskModel>(tasks));
            mockSession.Setup(s => s.Query<TaskDetailModel>()).Returns(new TestingQueryable<TaskDetailModel>(taskDetails));


            // Act
            var result = await _repo.GetTaskRewardDetails(tenantCode, null, languageCode);

            // Assert
            Assert.NotNull(result);

        }
      
    }
}
