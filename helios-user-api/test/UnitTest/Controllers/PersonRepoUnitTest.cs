using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Type;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.User.Tests.Infrastructure.Repositories
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
            return Task.FromResult(Execute<TResult>(expression));
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
    public class PersonRepoTests
    {
        private readonly Mock<ILogger<BaseRepo<PersonModel>>> _loggerMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly PersonRepo _personRepo;

        public PersonRepoTests()
        {
            _loggerMock = new Mock<ILogger<BaseRepo<PersonModel>>>();
            _sessionMock = new Mock<ISession>();
            _personRepo = new PersonRepo(_loggerMock.Object, _sessionMock.Object);
        }

        [Fact]
        public async Task GetConsumerPersons_ShouldReturnList_WhenValidInputProvided()
        {
            // Arrange
            var tenantCode = "testTenant";
            var searchTerm = "John";
            var skip = 0;
            var take = 10;

            var consumers = new List<ConsumerModel>
            {
                new ConsumerModel { PersonId = 1, TenantCode = tenantCode, DeleteNbr = 0 },
                new ConsumerModel { PersonId = 2, TenantCode = tenantCode, DeleteNbr = 0 }
            }.AsQueryable();

            var persons = new List<PersonModel>
            {
                new PersonModel { PersonId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", DeleteNbr = 0 },
                new PersonModel { PersonId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", DeleteNbr = 0 }
            }.AsQueryable();

            // Mock the session queries to return the IQueryable objects
            _sessionMock.Setup(s => s.Query<ConsumerModel>()).Returns(new TestingQueryable<ConsumerModel>(consumers));
            _sessionMock.Setup(s => s.Query<PersonModel>()).Returns(new TestingQueryable<PersonModel>(persons));

            // Act
            var result = await _personRepo.GetConsumerPersons(tenantCode, searchTerm, skip, take);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Expecting one match based on search term
            Assert.Equal("John", result[0].PersonModel.FirstName);
        }

        [Fact]
        public async Task GetConsumerPersons_ShouldThrowInvalidOperationException_WhenExceptionOccurs()
        {
            // Arrange
            var tenantCode = "testTenant";
            var searchTerm = "John";
            var skip = 0;
            var take = 10;

            // Setup the session mock to throw an exception when querying
            _sessionMock.Setup(s => s.Query<ConsumerModel>()).Throws<Exception>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _personRepo.GetConsumerPersons(tenantCode, searchTerm, skip, take));

            Assert.Contains("Error in querying Consumers and persons for Tenantcode", exception.Message);
        }

        [Fact]
        public async Task GetConsumerPersons_ShouldReturnJoinedRecords_WhenDataExists()
        {
            // Arrange
            var consumerCodes = new List<string> { "C1", "C2" };
            var tenantCode = "T1";

            var consumers = new List<ConsumerModel>
    {
        new ConsumerModel { ConsumerCode = "C1", TenantCode = "T1", DeleteNbr = 0, PersonId = 1 },
        new ConsumerModel { ConsumerCode = "C2", TenantCode = "T1", DeleteNbr = 0, PersonId = 2 }
    }.AsQueryable();

            var persons = new List<PersonModel>
    {
        new PersonModel { PersonId = 1, DeleteNbr = 0 },
        new PersonModel { PersonId = 2, DeleteNbr = 0 }
    }.AsQueryable();

            _sessionMock.Setup(s => s.Query<ConsumerModel>()).Returns(new TestingQueryable<ConsumerModel>(consumers));
            _sessionMock.Setup(s => s.Query<PersonModel>()).Returns(new TestingQueryable<PersonModel>(persons));


            // Act
            var result = await _personRepo.GetConsumerPersons(consumerCodes, tenantCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Contains(r.ConsumerModel.ConsumerCode, consumerCodes));
        }

        [Fact]
        public async Task GetConsumerPersonsolverload_ShouldThrowInvalidOperationException_WhenExceptionOccurs()
        {
            // Arrange
            // Arrange
            var consumerCodes = new List<string> { "C1", "C2" };
            var tenantCode = "T1";

            // Setup the session mock to throw an exception when querying
            _sessionMock.Setup(s => s.Query<ConsumerModel>()).Throws<Exception>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _personRepo.GetConsumerPersons(consumerCodes, tenantCode));

            Assert.Contains("Error in querying Consumers and persons for Tenantcode", exception.Message);
        }

    }
}
