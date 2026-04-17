using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using Exam_Prep.Models;

namespace Exam_Prep
{
    [TestFixture]

    public class Tests
    {
        private RestClient client;
        private static string LastCreatedIdeaId;
        private const string BaseUrl = "http://144.91.123.158:82";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJiOWE0ZDFlNC0zNTEwLTRkMGUtYjcwYi04ZmU4MjI1N2Y3OGEiLCJpYXQiOiIwNC8xNy8yMDI2IDIxOjAxOjQwIiwiVXNlcklkIjoiNWIyYmUyN2MtNjE5Ni00NGUxLTUzZjQtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJXb3JsZF9IZWxsb0BUaGlzLmNvbSIsIlVzZXJOYW1lIjoiV29ybGRfSGVsbG8iLCJleHAiOjE3NzY0ODEzMDAsImlzcyI6IklkZWFDZW50ZXJfQXBwX1NvZnRVbmkiLCJhdWQiOiJJZGVhQ2VudGVyX1dlYkFQSV9Tb2Z0VW5pIn0.dpJvZ4XaF52oR_nA3DcnI7RHXxAff_HGZeIqooohh9s";
        private const string LoginEmail = "World_Hello@This.com";
        private const string LoginPassword = "helloworld123";

        [OneTimeSetUp]

        public void Setup()
        {
            string jwtToken;
            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);
            
        }

        private string GetJwtToken(string email, string password)
        {
            var tempclient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            var response = tempclient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new Exception($"Failed to retrieve JWT token. Status Code: {response.StatusCode}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateIdea_WithValidData_ShouldReturnSuccess()
        {
            var idearequest = new IdeaDTO
            {
                Title = "Test Idea",
                Description = "This is a test Description",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(idearequest);
            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status Code 200 OK.");
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
        }
        [Order(2)]
        [Test]
        public void GetAllIdeas_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);
            var ideasResponse = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status Code 200 OK.");
            Assert.That(ideasResponse, Is.Not.Empty);
            Assert.That(ideasResponse, Is.Not.Null);
            LastCreatedIdeaId = ideasResponse.LastOrDefault()?.Id;

            if (string.IsNullOrWhiteSpace(LastCreatedIdeaId))
            {
                Assert.Fail("No ideas found to retrieve ID from.");
            }
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status Code 200 OK.");
        }
        [Order(3)]
        [Test]
        public void EditExistingIdea_ShouldReturnSuccess()
        {
            var editrequestdata = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is an edited test Description",
                Url = ""
            };
            // Pass the ID as part of the route instead of a query parameter
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", LastCreatedIdeaId);
            request.AddJsonBody(editrequestdata);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status Code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }
        [Order(4)]
        [Test]
        public void DeleteExistingIdea_ShouldReturnSuccess()
        {

            var request = new RestRequest("/api/Idea/Delete", Method.Delete);

            request.AddQueryParameter("ideaId", LastCreatedIdeaId);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status Code 200 OK.");
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
        }
        [Order(5)]
        [Test]
        public void CreateIdea_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var idearequest = new IdeaDTO
            {
                Title = "",
                Description = "This is a test Description",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(idearequest);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected Status Code 400 Bad Request.");
        }
        [Order(6)]
        [Test]
        public void EditIdea_WithInvalidId_ShouldReturnBadRequest()
        {
            var editrequestdata = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is an edited test Description",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", "invalid-id");
            request.AddJsonBody(editrequestdata);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected Status Code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }
        [Order(7)]
        [Test]
        public void DeleteIdea_WithInvalidId_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", "invalid-id");
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected Status Code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));

        }
        [OneTimeTearDown]

            public void TearDown()
            {
                // Clean up resources if needed
                this.client?.Dispose();
            }
        
    }
}

