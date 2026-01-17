using Amazon;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ImageSearchService : IImageSearchService
    {
        public readonly ILogger<ImageSearchService> _logger;
        private readonly ISecretHelper _secretHelper;
        public const string className = nameof(ImageSearchService);
        public ImageSearchService(ILogger<ImageSearchService> logger, ISecretHelper secretHelper)
        {
            _logger = logger;
            _secretHelper = secretHelper;
           

        }

        public async Task<ImageSearchResponseDto> AnalyzeImage(ImageSearchRequestDto imagerequest)
        {
            const string methodName = nameof(AnalyzeImage);
            var imageSearch = new ImageSearchResponseDto();

            try
            {
                var credentialsJson = _secretHelper.GetAwsFireBaseCredentialKey().Result;

                _logger.LogInformation("{ClassName}.{MethodName}: Image Analysis process started for imagerequest: {imagerequest}", className, methodName, imagerequest.ToJson());

                var credential = GoogleCredential.FromJson(credentialsJson)
                  .CreateScoped(ImageAnnotatorClient.DefaultScopes);

                var client = new ImageAnnotatorClientBuilder
                {
                    ChannelCredentials = credential.ToChannelCredentials()
                }.Build();


                // Load the image
                byte[] byteArray = Convert.FromBase64String(imagerequest.Base64Image);

                Image image = Image.FromBytes(byteArray);
                _logger.LogInformation("{ClassName}.{MethodName}: sending request for Image search with requestdto: {imagerequest}", className, methodName, imagerequest.ToJson());

                // Call the Vision API with the desired features
                var response = await client.AnnotateAsync(new AnnotateImageRequest()
                {
                    Image = image,
                    Features = {
                new Feature() { Type = Feature.Types.Type.LabelDetection },
                new Feature() { Type = Feature.Types.Type.LogoDetection },
                new Feature() { Type = Feature.Types.Type.TextDetection }
            }
                });
                if (response != null )
                {
                    // Extract labels
                    if (response.LabelAnnotations.Count > 0)
                    {
                        var labels = response.LabelAnnotations.Select(label => label.Description).ToList();
                        imageSearch.lables = labels;
                    }

                    if (response.LabelAnnotations.Count > 0)
                    {
                        // Extract logos
                        var logos = response.LogoAnnotations.Select(logo => logo.Description).ToList();
                        imageSearch.logos = logos;
                    }

                    // Extract text (if any, like product names or serial numbers)
                    if (response.TextAnnotations.Any())
                    {
                        imageSearch.text = response.TextAnnotations[0].Description;
                    }
                    return imageSearch;
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName}: Image Analysis Response encountered error for imagerequest: {imagerequest}", className, methodName, imagerequest.ToJson());
                    imageSearch.ErrorCode = StatusCodes.Status204NoContent;
                    imageSearch.ErrorMessage = "Empty response recieved";

                }
                return imageSearch;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while  sending request for Image search with requestdto {imagerequest}. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, imagerequest.ToJson(), ex.Message, ex.StackTrace);
                throw;

            }

        }


    }
}
