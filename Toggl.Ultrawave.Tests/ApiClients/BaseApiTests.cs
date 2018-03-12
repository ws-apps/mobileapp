﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Multivac.Extensions;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Helpers;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Serialization;
using Xunit;
using static System.Net.HttpStatusCode;

namespace Toggl.Ultrawave.Tests.ApiClients
{
    public sealed class BaseApiTests
    {
        public sealed class TheGetAuthHeaderMethod
        {
            [Fact, LogIfTooSlow]
            public async Task CreatesRequestWithAppropriateHeaders()
            {
                var apiClient = Substitute.For<IApiClient>();
                var serializer = Substitute.For<IJsonSerializer>();
                var endpoint = Endpoint.Get(BaseUrls.ForApi(ApiEnvironment.Staging), "");

                apiClient.Send(Arg.Any<Request>()).Returns(x => new Response("It lives", true, "text/plain", new List<KeyValuePair<string, IEnumerable<string>>>(), OK));

                var credentials = Credentials.WithPassword(
                    "susancalvin@psychohistorian.museum".ToEmail(),
                    "theirobotmoviesucked123".ToPassword());
                const string expectedHeader = "c3VzYW5jYWx2aW5AcHN5Y2hvaGlzdG9yaWFuLm11c2V1bTp0aGVpcm9ib3Rtb3ZpZXN1Y2tlZDEyMw==";

                var testApi = new TestApi(endpoint, apiClient, serializer, credentials, endpoint);

                await testApi.Get();
                await apiClient.Received().Send(Arg.Is<Request>(request => verifyAuthHeader(request, expectedHeader)));
            }

            private static bool verifyAuthHeader(Request request, string expectedHeader)
            {
                var authHeader = request.Headers.ToList().Single();

                if (authHeader.Type != HttpHeader.HeaderType.Auth) return false;
                if (authHeader.Value != expectedHeader) return false;
                return true;
            }
        }

        public sealed class TheCreateObservableMethod
        {
            private IApiClient apiClient = Substitute.For<IApiClient>();
            private IJsonSerializer serializer = Substitute.For<IJsonSerializer>();

            [Fact, LogIfTooSlow]
            public async Task CreatesAnObservableThatReturnsASingleValue()
            {
                apiClient.Send(Arg.Any<Request>()).Returns(x => new Response("It lives", true, "text/plain", new List<KeyValuePair<string, IEnumerable<string>>>(), OK));

                var credentials = Credentials.WithPassword(
                    "susancalvin@psychohistorian.museum".ToEmail(),
                    "theirobotmoviesucked123".ToPassword());
                var endpoint = Endpoint.Get(BaseUrls.ForApi(ApiEnvironment.Staging), "");
                var testApi = new TestApi(endpoint, apiClient, serializer, credentials, endpoint);

                var observable = testApi.TestCreateObservable<string>(endpoint, Enumerable.Empty<HttpHeader>(), "");

                await observable.SingleAsync();
            }

            [Fact, LogIfTooSlow]
            public void EmitsAnOfflineExceptionIfTheApiClientThrowsAnHttpRequestException()
            {
                Exception caughtException = null;
                var httpRequestException = new HttpRequestException();
                apiClient.Send(Arg.Any<Request>()).Returns<IResponse>(_ => throw httpRequestException);
                var credentials = Credentials.None;
                var endpoint = Endpoint.Get(BaseUrls.ForApi(ApiEnvironment.Staging), "");
                var testApi = new TestApi(endpoint, apiClient, serializer, credentials, endpoint);

                try
                {
                    testApi.TestCreateObservable<string>(endpoint, Enumerable.Empty<HttpHeader>(), "").Wait();
                }
                catch (Exception e)
                {
                    caughtException = e;
                }

                caughtException.Should().NotBeNull();
                caughtException.Should().BeOfType<OfflineException>();
                caughtException.InnerException.Should().Be(httpRequestException);
            }


            [Fact, LogIfTooSlow]
            public void EmitsADeserializationErrorIfTheJsonSerializerThrowsAnException()
            {
                const string rawResponse = "It lives";
                serializer.Deserialize<string>(Arg.Any<string>()).Returns(_ => throw new Exception());
                apiClient.Send(Arg.Any<Request>()).Returns(x => new Response(rawResponse, true, "text/plain", new List<KeyValuePair<string, IEnumerable<string>>>(), OK));

                var credentials = Credentials.WithPassword(
                    "susancalvin@psychohistorian.museum".ToEmail(),
                    "theirobotmoviesucked123".ToPassword());
                var endpoint = Endpoint.Get(BaseUrls.ForApi(ApiEnvironment.Staging), "");
                var testApi = new TestApi(endpoint, apiClient, serializer, credentials, endpoint);

                var observable = testApi.TestCreateObservable<string>(endpoint, Enumerable.Empty<HttpHeader>(), "");

                Func<Task> theObservableReturnedWhenTheApiFails =
                    async () => await observable;

                theObservableReturnedWhenTheApiFails
                    .ShouldThrow<DeserializationException<string>>()
                    .Which.Json.Should().Be(rawResponse);
            }

            [Fact, LogIfTooSlow]
            public async void PassesTheResponseAndItsDataToTheValidator()
            {
                var expectedData = "It lives";
                var expectedResponse = new Response(expectedData, true, "text/plain",
                    new List<KeyValuePair<string, IEnumerable<string>>>(), OK);
                apiClient.Send(Arg.Any<Request>()).Returns(x => expectedResponse);
                serializer.Deserialize<string>(Arg.Any<string>()).Returns(expectedData);

                var credentials = Credentials.WithPassword(
                    "susancalvin@psychohistorian.museum".ToEmail(),
                    "theirobotmoviesucked123".ToPassword());
                var endpoint = Endpoint.Get(BaseUrls.ForApi(ApiEnvironment.Staging), "");
                var testApi = new TestApi(endpoint, apiClient, serializer, credentials, endpoint);

                IRequest receivedRequest = null;
                IResponse receivedResponse = null;
                string receivedData = null;
                var observable = testApi.TestCreateObservable<string>(endpoint, Enumerable.Empty<HttpHeader>(), "",
                    (request, response, data) => (receivedRequest, receivedResponse, receivedData) = (request, response, data));
                
                await observable.SingleAsync();

                receivedRequest.HttpMethod.Should().Be(endpoint.Method);
                receivedRequest.Endpoint.Should().Be(endpoint.Url);
                receivedResponse.Should().Be(expectedResponse);
                receivedData.Should().Be(expectedData);
            }
            
            [Fact, LogIfTooSlow]
            public void EmitsTheThrownExceptionIfTheValidatorThrows()
            {
                apiClient.Send(Arg.Any<Request>()).Returns(x => new Response("It lives", true, "text/plain", new List<KeyValuePair<string, IEnumerable<string>>>(), OK));

                var credentials = Credentials.WithPassword(
                    "susancalvin@psychohistorian.museum".ToEmail(),
                    "theirobotmoviesucked123".ToPassword());
                var endpoint = Endpoint.Get(BaseUrls.ForApi(ApiEnvironment.Staging), "");
                var testApi = new TestApi(endpoint, apiClient, serializer, credentials, endpoint);

                var exampleExceptionMessage = "What is this.";
                var exception = new TestException(exampleExceptionMessage);
                var observable = testApi.TestCreateObservable<string>(endpoint, Enumerable.Empty<HttpHeader>(), "",
                    (request, response, data) => throw exception);
                
                Func<Task> theObservableReturnedWhenTheValidatorThrows =
                    async () => await observable;

                theObservableReturnedWhenTheValidatorThrows
                    .ShouldThrow<TestException>()
                    .WithMessage(exampleExceptionMessage);
            }
        }
    }
}
