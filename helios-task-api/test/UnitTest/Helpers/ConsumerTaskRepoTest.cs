using Microsoft.Extensions.Logging;
using Moq;
using NHibernate.Linq;
using NHibernate.Type;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using System.Collections;
using Xunit;
using System.Linq.Expressions;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
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
            return System.Threading.Tasks.Task.FromResult(Execute<TResult>(expression));
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
    public class ConsumerTaskRepoTest
    {
        private readonly Mock<ILogger<BaseRepo<ConsumerTaskModel>>> _loggerMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly ConsumerTaskRepo _consumerTaskRepo;

        public ConsumerTaskRepoTest()
        {
            _loggerMock = new Mock<ILogger<BaseRepo<ConsumerTaskModel>>>();
            _sessionMock = new Mock<ISession>();
            _consumerTaskRepo = new ConsumerTaskRepo(_loggerMock.Object, _sessionMock.Object);
        }


        [Fact]
        public async System.Threading.Tasks.Task GetConsumerTask_ShouldReturnList_WhenValidInputProvided()
        {
            // Arrange
            var getConsumerTaskByTaskId = new GetConsumerTaskByTaskId()
            {
                TenantCode = "TenantCode",
                PageSize = 10,
                Skip = 5,
                TaskId = 1,
                StartDate = new DateTime(),
                EndDate = new DateTime()
            }; 
            


            var consumers = new List<ConsumerTaskModel>
            {
                new ConsumerTaskModel { TaskId = 1, TenantCode = "", DeleteNbr = 0 },
                new ConsumerTaskModel { TaskId = 2, TenantCode = "tenantCode", DeleteNbr = 0 }
            }.AsQueryable();



            // Mock the session queries to return the IQueryable objects
            _sessionMock.Setup(s => s.Query<ConsumerTaskModel>())
                .Returns(new TestingQueryable<ConsumerTaskModel>(consumers));


            // Act
            var result = await _consumerTaskRepo.GetPaginatedConsumerTask(getConsumerTaskByTaskId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumerPersons_ShouldThrowInvalidOperationException_WhenExceptionOccurs()
        {


            // Arrange
            var getConsumerTaskByTaskId = new GetConsumerTaskByTaskId()
            {
                TenantCode = "TenantCode",
                PageSize = 10,
                Skip = 5,
                TaskId = 1,
                StartDate = new DateTime(),
                EndDate = new DateTime()
            };



            var consumers = new List<ConsumerTaskModel>
            {
                new ConsumerTaskModel { TaskId = 1, TenantCode = "", DeleteNbr = 0 },
                new ConsumerTaskModel { TaskId = 2, TenantCode = "tenantCode", DeleteNbr = 0 }
            }.AsQueryable();

            _sessionMock.Setup(s => s.Query<ConsumerTaskModel>()).Throws<Exception>();


            var exception = await Assert.ThrowsAsync<Exception>(
               () => _consumerTaskRepo.GetPaginatedConsumerTask(getConsumerTaskByTaskId));

            Assert.Contains("Exception of type 'System.Exception'", exception.Message);
        }
    }
}
