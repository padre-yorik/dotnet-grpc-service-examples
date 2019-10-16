using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NewVoiceMedia.DotNetGrpcServiceExamples.Models;
using Xunit;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Controllers
{
    [Trait("Category", "Integration")]
    public class ToDoItemsControllerTests
    {
        private const string ApiVersion = "3";

        [Fact]
        public async void GetTodos_ReturnsEmptyList()
        {
            var user = new ApiClient.User();
            using (var client = await ApiClient.CreateHttpClient(user, ApiVersion))
            {
                var response = await client.GetAsync("todos");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsAsync<ToDoItem[]>();
                content.Should().HaveCount(0);
            }
        }

        [Theory]
        [InlineData("3", HttpStatusCode.OK)]
        [InlineData("1.0", HttpStatusCode.BadRequest)]
        [InlineData(null, HttpStatusCode.BadRequest)]
        [InlineData("0", HttpStatusCode.BadRequest)]
        [InlineData("2", HttpStatusCode.BadRequest)]
        public async void GetSchedules_RequiresApiVersion3(string apiVersion, HttpStatusCode expectedResult)
        {
            var user = new ApiClient.User();
            using (var client = await ApiClient.CreateHttpClient(user, apiVersion))
            {
                var response = await client.GetAsync("todos");
                response.StatusCode.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async void GetNonExistentToDo_ReturnsNotFound()
        {
            var user = new ApiClient.User();
            using (var client = await ApiClient.CreateHttpClient(user, ApiVersion))
            {
                var response = await client.GetAsync("todos/12345678-abcd-1234-abcd-1234567890ab");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task PostToDoItem_CreatedANewToDoItemAsync()
        {
            var user = new ApiClient.User();
            using (var client = await ApiClient.CreateHttpClient(user, ApiVersion))
            {
                var todo = new ToDoItem
                {
                    Name = "TODO 1",
                    IsComplete = false
                };
                var response = await client.PostAsJsonAsync("todos", todo);
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                response.Headers.Location.Should().NotBeNull();

                var getResponse = await client.GetAsync(response.Headers.Location);
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var savedToDo = await getResponse.Content.ReadAsAsync<ToDoItem>();
                savedToDo.ToDoId.Should().NotBeEmpty();
                savedToDo.Name.Should().Be("TODO 1");
                savedToDo.IsComplete.Should().Be(false);
            }
        }

        [Fact]
        public async void DeleteToDoItem_DeletesToDoItem()
        {
            var user = new ApiClient.User();
            using (var client = await ApiClient.CreateHttpClient(user, ApiVersion))
            {
                var todo = new ToDoItem
                {
                    Name = "TODO 1",
                    IsComplete = false
                };

                var response = await client.PostAsJsonAsync("todos", todo);
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                response.Headers.Location.Should().NotBeNull();

                var initialGetResponse = await client.GetAsync(response.Headers.Location);
                initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var deleteResponse = await client.DeleteAsync(response.Headers.Location);
                deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var afterDeleteGetResponse = await client.GetAsync(response.Headers.Location);
                afterDeleteGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
