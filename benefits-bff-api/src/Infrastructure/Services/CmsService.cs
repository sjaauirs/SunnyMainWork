using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class CmsService : ICmsService
    {
        private readonly ILogger<ICmsService> _cmsServiceLogger;
        private readonly ICmsClient _cmsClient;
        private readonly ICohortConsumerService _cohortConsumerService;
        private readonly ICommonHelper _commonHelperService;
        private const string className = nameof(CmsService);

        private static readonly HashSet<string> ExactCohortMatchComponentTypeList =
            new(StringComparer.OrdinalIgnoreCase) { CommonConstants.purseComponent };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmsServiceLogger"></param>
        /// <param name="cmsClient"></param>
        public CmsService(ILogger<ICmsService> cmsServiceLogger, ICmsClient cmsClient, ICohortConsumerService cohortConsumerService, ICommonHelper commonHelperService)
        {
            _cmsClient = cmsClient;
            _cmsServiceLogger = cmsServiceLogger;
            _cohortConsumerService = cohortConsumerService;
            _commonHelperService = commonHelperService;
        }

        public async Task<GetComponentListResponseDto> GetCmsComponentList(GetComponentListRequestDto getComponentListRequestDto, string? requestId = null)
        {
            const string methodName = nameof(GetCmsComponentList);
            var totalStopwatch = Stopwatch.StartNew();
            requestId ??= Guid.NewGuid().ToString("N")[..16]; // Generate if not provided
            try
            {
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Started for TenantCode: {TenantCode}, ComponentName: {ComponentName}, RequestId: {RequestId}",
                    className, methodName, getComponentListRequestDto?.TenantCode, getComponentListRequestDto?.ComponentName, requestId);

                GetComponentListResponseDto responseDto = new();
                
                if (!string.IsNullOrEmpty(getComponentListRequestDto.ComponentName) && getComponentListRequestDto.ComponentName.ToLower().Equals(CmsConstants.FOR_YOU.ToLower()))
                {
                    var consumerCodeStopwatch = Stopwatch.StartNew();
                    if (string.IsNullOrEmpty(getComponentListRequestDto.ConsumerCode))
                    {
                        getComponentListRequestDto.ConsumerCode = _commonHelperService.GetUserConsumerCodeFromHttpContext();
                    }
                    consumerCodeStopwatch.Stop();
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Step 1: GetConsumerCodeFromHttpContext took {ElapsedMs}ms, RequestId: {RequestId}",
                        className, methodName, consumerCodeStopwatch.ElapsedMilliseconds, requestId);

                    var getComponentListStopwatch = Stopwatch.StartNew();
                    var componentList = await GetComponentList(getComponentListRequestDto, requestId);
                    getComponentListStopwatch.Stop();
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Step 2: GetComponentList (CMS API call) took {ElapsedMs}ms, ComponentsCount: {Count}, RequestId: {RequestId}",
                        className, methodName, getComponentListStopwatch.ElapsedMilliseconds, componentList?.Components?.Count ?? 0, requestId);
                    
                    var filterCohortStopwatch = Stopwatch.StartNew();
                    var filteredCohortComponents = await GetFilteredCohortComponents(componentList?.Components, getComponentListRequestDto?.TenantCode,
                        getComponentListRequestDto?.ConsumerCode, getComponentListRequestDto?.ComponentName, requestId);
                    filterCohortStopwatch.Stop();
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Step 3: GetFilteredCohortComponents took {ElapsedMs}ms, FilteredCount: {Count}, RequestId: {RequestId}",
                        className, methodName, filterCohortStopwatch.ElapsedMilliseconds, filteredCohortComponents?.Count ?? 0, requestId);
                    
                    responseDto.Components = filteredCohortComponents;
                }
                else
                {
                    var getComponentListStopwatch = Stopwatch.StartNew();
                    responseDto = await GetComponentList(getComponentListRequestDto, requestId);
                    getComponentListStopwatch.Stop();
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Step 1: GetComponentList (CMS API call - non-for-you) took {ElapsedMs}ms, ComponentsCount: {Count}, RequestId: {RequestId}",
                        className, methodName, getComponentListStopwatch.ElapsedMilliseconds, responseDto?.Components?.Count ?? 0, requestId);
                }
                
                totalStopwatch.Stop();
                
                // Overall timing summary for service level
                if (!string.IsNullOrEmpty(getComponentListRequestDto.ComponentName) && getComponentListRequestDto.ComponentName.ToLower().Equals(CmsConstants.FOR_YOU.ToLower()))
                {
                    _cmsServiceLogger.LogInformation(
                        "⏱️ SERVICE TIMING SUMMARY - TotalTime: {TotalMs}ms, TenantCode: {TenantCode}, ComponentName: {ComponentName}, FinalComponentsCount: {Count}, RequestId: {RequestId}",
                        totalStopwatch.ElapsedMilliseconds, getComponentListRequestDto?.TenantCode, 
                        getComponentListRequestDto?.ComponentName, responseDto?.Components?.Count ?? 0, requestId);
                }
                else
                {
                    _cmsServiceLogger.LogInformation(
                        "⏱️ SERVICE TIMING SUMMARY - TotalTime: {TotalMs}ms, TenantCode: {TenantCode}, ComponentName: {ComponentName}, ComponentsCount: {Count}, RequestId: {RequestId}",
                        totalStopwatch.ElapsedMilliseconds, getComponentListRequestDto?.TenantCode, 
                        getComponentListRequestDto?.ComponentName, responseDto?.Components?.Count ?? 0, requestId);
                }
                
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved GetComponentList Successfully for TenantCode: {TenantCode}, ComponentName: {ComponentName}, TotalTime: {TotalMs}ms, RequestId: {RequestId}",
                   className, methodName, getComponentListRequestDto?.TenantCode, getComponentListRequestDto?.ComponentName, totalStopwatch.ElapsedMilliseconds, requestId);
                return responseDto;
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                _cmsServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while Retrieving GetComponentList for TenantCode: {TenantCode}, ComponentName: {ComponentName}, ErrorCode: {ErrorCode}, ERROR: {Msg}, TotalTime: {TotalMs}ms, RequestId: {RequestId}",
                    className, methodName, getComponentListRequestDto?.TenantCode, getComponentListRequestDto?.ComponentName, StatusCodes.Status500InternalServerError, ex.Message, totalStopwatch.ElapsedMilliseconds, requestId);
                throw;
            }
        }
        public async Task<GetComponentListResponseDto> GetComponentList(GetComponentListRequestDto getComponentListRequestDto, string? requestId = null)
        {
            const string methodName = nameof(GetComponentList);
            var stopwatch = Stopwatch.StartNew();
            requestId ??= Guid.NewGuid().ToString("N")[..16]; // Generate if not provided
            try
            {
                var componentList = await _cmsClient.Post<GetComponentListResponseDto>("cms/component-list", getComponentListRequestDto);
                stopwatch.Stop();
                
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - CMS API call completed, TenantCode: {TenantCode}, ComponentName: {ComponentName}, HttpCallTime: {ElapsedMs}ms, ComponentsCount: {Count}, RequestId: {RequestId}",
                   className, methodName, getComponentListRequestDto?.TenantCode, getComponentListRequestDto?.ComponentName, stopwatch.ElapsedMilliseconds, componentList?.Components?.Count ?? 0, requestId);
                return componentList;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _cmsServiceLogger.LogError(ex, "{ClassName}.{MethodName} - CMS API call failed, TenantCode: {TenantCode}, ComponentName: {ComponentName}, ErrorCode: {ErrorCode}, ERROR: {Msg}, HttpCallTime: {ElapsedMs}ms, RequestId: {RequestId}",
                    className, methodName, getComponentListRequestDto?.TenantCode, getComponentListRequestDto?.ComponentName, StatusCodes.Status500InternalServerError, ex.Message, stopwatch.ElapsedMilliseconds, requestId);
                throw;
            }
        }

        public async Task<List<ComponentDto>> GetFilteredCohortComponents(IList<ComponentDto>? componentList, string? tenantCode, string? consumerCode, string? componentName, string? requestId = null)
        {
            const string methodName = nameof(GetFilteredCohortComponents);
            var totalStopwatch = Stopwatch.StartNew();
            requestId ??= Guid.NewGuid().ToString("N")[..16]; // Generate if not provided

            try
            {
                if (componentList == null || componentList.Count == 0)
                {
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - ComponentList is null or empty, returning empty list, RequestId: {RequestId}",
                        className, methodName, requestId);
                    return new List<ComponentDto>();
                }

                var parseMetadataStopwatch = Stopwatch.StartNew();
                var filteredComponents = new List<ComponentDto>();
                var cohortComponentList = new List<CohortComponentListDto>();
                var allCohorts = new List<string>();

                foreach (var component in componentList)
                {
                    if (IsMetadataEmpty(component.MetadataJson))
                    {
                        filteredComponents.Add(component);
                        continue;
                    }

                    var tagCohorts = ExtractCohortsFromMetadata(component.MetadataJson!);

                    cohortComponentList.Add(new CohortComponentListDto
                    {
                        componentDto = component,
                        CohortName = tagCohorts
                    });

                    allCohorts.AddRange(tagCohorts);
                }
                parseMetadataStopwatch.Stop();
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Step 3.1: ParseMetadataAndExtractCohorts took {ElapsedMs}ms, TotalComponents: {TotalCount}, ComponentsWithCohorts: {CohortCount}, UniqueCohorts: {UniqueCount}, RequestId: {RequestId}",
                    className, methodName, parseMetadataStopwatch.ElapsedMilliseconds, componentList.Count, cohortComponentList.Count, allCohorts.Distinct().Count(), requestId);

                if (allCohorts.Count > 0)
                {
                    var cohortApiStopwatch = Stopwatch.StartNew();
                    var consumerEnrolledCohorts = await GetConsumerEnrolledCohortsAsync(
                        tenantCode,
                        consumerCode,
                        allCohorts.Distinct().ToList(),
                        requestId
                    );
                    cohortApiStopwatch.Stop();
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Step 3.2: GetConsumerEnrolledCohortsAsync (Cohort API call) took {ElapsedMs}ms, EnrolledCohortsCount: {Count}, RequestId: {RequestId}",
                        className, methodName, cohortApiStopwatch.ElapsedMilliseconds, consumerEnrolledCohorts?.Count ?? 0, requestId);

                    var filterStopwatch = Stopwatch.StartNew();
                    var matchingComponents = cohortComponentList
                        .Where(c => c.CohortName.Any(cohort =>
                            consumerEnrolledCohorts.Contains(cohort) ||
                            cohort.Equals(CommonConstants.ALL, StringComparison.OrdinalIgnoreCase) ||
                            cohort.Equals(CommonConstants.EVERYONE, StringComparison.OrdinalIgnoreCase)))
                        .Select(c => c.componentDto)
                        .ToList();
                    filterStopwatch.Stop();
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Step 3.3: FilterMatchingComponents took {ElapsedMs}ms, MatchingComponentsCount: {Count}, RequestId: {RequestId}",
                        className, methodName, filterStopwatch.ElapsedMilliseconds, matchingComponents.Count, requestId);

                    filteredComponents.AddRange(matchingComponents);
                }
                else
                {
                    _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - No cohorts found in metadata, adding all components, RequestId: {RequestId}",
                        className, methodName, requestId);
                    filteredComponents.AddRange(componentList);
                }

                totalStopwatch.Stop();
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved filtered cohort components for TenantCode: {TenantCode}, ComponentName: {ComponentName}, TotalTime: {TotalMs}ms, FinalCount: {Count}, RequestId: {RequestId}",
                    className, methodName, tenantCode, componentName, totalStopwatch.ElapsedMilliseconds, filteredComponents.Count, requestId);

                return filteredComponents.Distinct().ToList();
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                _cmsServiceLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName} - Error filtering cohort components for TenantCode: {TenantCode}, ComponentName: {ComponentName}, ErrorCode: {ErrorCode}, ERROR: {Msg}, TotalTime: {TotalMs}ms",
                    className, methodName, tenantCode, componentName, StatusCodes.Status500InternalServerError, ex.Message, totalStopwatch.ElapsedMilliseconds);

                throw;
            }
        }

        private static bool IsMetadataEmpty(string? metadataJson) =>
            string.IsNullOrWhiteSpace(metadataJson) || metadataJson == "{}";

        private static List<string> ExtractCohortsFromMetadata(string metadataJson)
        {
            try
            {
                return JsonDocument.Parse(metadataJson)
                    .RootElement
                    .GetProperty("tags")
                    .EnumerateArray()
                    .Select(t => t.GetString())
                    .Where(t => !string.IsNullOrEmpty(t) && t.StartsWith("cohort:", StringComparison.OrdinalIgnoreCase))
                    .Select(t => t!["cohort:".Length..]) // Remove "cohort:" prefix
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private async Task<List<string>> GetConsumerEnrolledCohortsAsync(
            string? tenantCode,
            string? consumerCode,
            List<string> cohortNames,
            string? requestId = null)
        {
            const string methodName = nameof(GetConsumerEnrolledCohortsAsync);
            var stopwatch = Stopwatch.StartNew();
            requestId ??= Guid.NewGuid().ToString("N")[..16]; // Generate if not provided
            try
            {
                var request = new GetConsumerByCohortsNameRequestDto
                {
                    ConsumerCode = consumerCode ?? string.Empty,
                    TenantCode = tenantCode ?? string.Empty,
                    CohortName = cohortNames
                };

                var cohortList = await _cohortConsumerService.GetConsumerCohorts(request, requestId);
                stopwatch.Stop();

                var enrolledCohorts = cohortList?.ConsumerCohorts
                    .Select(x => x.CohortName)
                    .ToList() ?? new List<string>();

                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Cohort API call completed, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, HttpCallTime: {ElapsedMs}ms, EnrolledCohortsCount: {Count}, RequestId: {RequestId}",
                    className, methodName, tenantCode, consumerCode, stopwatch.ElapsedMilliseconds, enrolledCohorts.Count, requestId);

                return enrolledCohorts;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _cmsServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error calling Cohort API for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, ErrorCode: {ErrorCode}, ERROR: {Msg}, HttpCallTime: {ElapsedMs}ms, RequestId: {RequestId}",
                    className, methodName, tenantCode, consumerCode, StatusCodes.Status500InternalServerError, ex.Message, stopwatch.ElapsedMilliseconds, requestId);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findStoreRequestDTO"></param>
        /// <returns></returns>
        public async Task<List<StoreResponseMockDTO>> FindStoreMock(FindStoreRequestDTO findStoreRequestDTO)
        {
            const string methodName = nameof(FindStoreMock);
            try
            {
                var res = await SetMockData(findStoreRequestDTO);
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved FindStoreMock Successfully for  Latitude :{Latitude},Longitude :{Longitude}"
                    , className, methodName, findStoreRequestDTO.coords.Latitude, findStoreRequestDTO.coords.Longitude);
                return res;
            }
            catch (Exception ex)
            {
                _cmsServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while  Retrieving for  Latitude :{Latitude},Longitude :{Longitude}, ErrorCode:{ErrorCode}, ERROR: msg : {Msg}",
                    className, methodName, findStoreRequestDTO.coords.Latitude, findStoreRequestDTO.coords.Longitude, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findStoreRequestDTO"></param>
        /// <returns></returns>
        private async Task<List<StoreResponseMockDTO>> SetMockData(FindStoreRequestDTO findStoreRequestDTO)
        {


            List<StoreResponseMockDTO> response = new List<StoreResponseMockDTO>
        {
            new StoreResponseMockDTO
            {
                Name = "Costco Wholesale",
                Address = "123 Main Street, Boston, MA 02345",
                PhoneNumber = "617-123-4567",
                ImageUrl = "https://webautomation.io/static/images/domain_images/costco_6MWqLoX.png",
                Metrics = new List<Metric>
                {
                    new Metric { Value = "1.1", Unit = "m" },
                    new Metric { Value = "5", Unit = "min" }
                },
                TravelTimeValue = "15",
                TravelTimeUnit = "min",
                IsOpen = true,
                ClosingDescription = "Closes at 8PM",
                Location = new Location { Latitude = 42.362189, Longitude = -71.053147 },
                Hours = new List<string>
                {
                    "Monday-Friday: 10:00AM - 08:30PM",
                    "Saturday: 09:30AM - 06:00PM",
                    "Sunday: 10:00AM - 06:00PM"
                },
                Benefits = new List<string>
                {
                    "OTC",
                    "Food",
                    "Fuel",
                    "Vision",
                    "Hearing",
                    "Transportation"
                }
            },
            new StoreResponseMockDTO
            {
                Name = "CVS",
                Address = "123 Main Street, Boston, MA 02345",
                PhoneNumber = "617-123-4567",
                ImageUrl = "https://assets.stickpng.com/images/609f95f202f45d00046b1be2.png",
                Metrics = new List<Metric>
                {
                    new Metric { Value = "1.1", Unit = "m" },
                    new Metric { Value = "5", Unit = "min" }
                },
                TravelTimeValue = "15",
                TravelTimeUnit = "min",
                IsOpen = true,
                ClosingDescription = "Closes at 8PM",
                Location = new Location { Latitude = 42.361951, Longitude = -71.052965 },
                Hours = new List<string>
                {
                    "Monday-Friday: 10:00AM - 08:30PM",
                    "Saturday: 09:30AM - 06:00PM",
                    "Sunday: 10:00AM - 06:00PM"
                },
                Benefits = new List<string>
                {
                    "OTC",
                    "Food",
                    "Fuel",
                    "Vision",
                    "Hearing",
                    "Transportation"
                }
            },
            new StoreResponseMockDTO
            {
                Name = "Walmart",
                Address = "123 Main Street, Boston, MA 02345",
                PhoneNumber = "617-123-4567",
                ImageUrl = "https://cdn.icon-icons.com/icons2/2699/PNG/512/walmart_logo_icon_170230.png",
                Metrics = new List<Metric>
                {
                    new Metric { Value = "1.1", Unit = "m" },
                    new Metric { Value = "5", Unit = "min" }
                },
                TravelTimeValue = "15",
                TravelTimeUnit = "min",
                IsOpen = true,
                ClosingDescription = "Closes at 8PM",
                Location = new Location { Latitude = 42.360638, Longitude = -71.053954 },
                Hours = new List<string>
                {
                    "Monday-Friday: 10:00AM - 08:30PM",
                    "Saturday: 09:30AM - 06:00PM",
                    "Sunday: 10:00AM - 06:00PM"
                },
                Benefits = new List<string>
                {
                    "OTC",
                    "Food",
                    "Fuel",
                    "Vision",
                    "Hearing",
                    "Transportation"
                }
            },
            new StoreResponseMockDTO
            {
                Name = "Albertsons",
                Address = "123 Main Street, Boston, MA 02345",
                PhoneNumber = "617-123-4567",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/96/Albertsons_logo_vertical.svg/2560px-Albertsons_logo_vertical.svg.png",
                Metrics = new List<Metric>
                {
                    new Metric { Value = "1.1", Unit = "m" },
                    new Metric { Value = "5", Unit = "min" }
                },
                TravelTimeValue = "15",
                TravelTimeUnit = "min",
                IsOpen = true,
                ClosingDescription = "Closes at 8PM",
                Location = new Location { Latitude = 42.361375, Longitude = -71.053332 },
                Hours = new List<string>
                {
                    "Monday-Friday: 10:00AM - 08:30PM",
                    "Saturday: 09:30AM - 06:00PM",
                    "Sunday: 10:00AM - 06:00PM"
                },
                Benefits = new List<string>
                {
                    "OTC",
                    "Food",
                    "Fuel",
                    "Vision",
                    "Hearing",
                    "Transportation"
                }
            },
            new StoreResponseMockDTO
            {
                Name = "CVS",
                Address = "123 Main Street, Boston, MA 02345",
                PhoneNumber = "617-123-4567",
                ImageUrl = "https://assets.stickpng.com/images/609f95f202f45d00046b1be2.png",
                Metrics = new List<Metric>
                {
                    new Metric { Value = "1.1", Unit = "m" },
                    new Metric { Value = "5", Unit = "min" }
                },
                TravelTimeValue = "15",
                TravelTimeUnit = "min",
                IsOpen = true,
                ClosingDescription = "Closes at 8PM",
                Location = new Location { Latitude = 42.361263, Longitude = -71.052328 },
                Hours = new List<string>
                {
                    "Monday-Friday: 10:00AM - 08:30PM",
                    "Saturday: 09:30AM - 06:00PM",
                    "Sunday: 10:00AM - 06:00PM"
                },
                Benefits = new List<string>
                {
                    "OTC",
                    "Food",
                    "Fuel",
                    "Vision",
                    "Hearing",
                    "Transportation"
                }
            }
        };

            return response;
        }

        public async Task<FaqSectionResponseDto> GetFaqSection(FaqRetriveRequestDto faqRetriveRequestDto)
        {
            const string methodName = nameof(GetFaqSection);
            try
            {
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Started Processing Faq for TenantCode:{TenantCode}", className, methodName, faqRetriveRequestDto.TenantCode);
                var faqSectionResponse = new FaqSectionResponseDto
                {
                    FaqSections = new List<FaqSectionDto>()
                };
                var sectionListRequest = new ComponentListRequestDto
                {
                    TenantCode = faqRetriveRequestDto.TenantCode,
                    ComponentName = CmsConstants.FAQ_SECTION,
                    LanguageCode = faqRetriveRequestDto.LanguageCode

                };
                var tenantComponentByTypeNameRequest = new GetTenantComponentByTypeNameRequestDto
                {
                    TenantCode = faqRetriveRequestDto.TenantCode,
                    ComponentTypeName = CmsConstants.FAQ,
                    LanguageCode = faqRetriveRequestDto.LanguageCode,
                    ConsumerCode = faqRetriveRequestDto.ConsumerCode,
                    ApplyCohortFilter = true
                };
                string HtmlContentUrl = string.Empty;

                if (faqRetriveRequestDto.richContentEnabled ?? false)
                {
                    var htmlComponentResponse = await GetTenantComponentsByTypeName(tenantComponentByTypeNameRequest);

                    if (htmlComponentResponse?.Components?.Any() == true)
                    {
                        _cmsServiceLogger.LogInformation(
                            "{ClassName}.{MethodName} - Processing rich FAQ HTML content for TenantCode: {TenantCode}",
                            className, methodName, faqRetriveRequestDto.TenantCode);

                        // Fetch first valid HTML content URL
                        foreach (var component in htmlComponentResponse.Components)
                        {
                            var cmsComponentData = string.IsNullOrEmpty(component.DataJson)
                                ? new CmsComponentData()
                                : JsonConvert.DeserializeObject<CmsComponentData>(component.DataJson);

                            HtmlContentUrl = cmsComponentData?.Data?.Details?.HtmlContentUrl ?? string.Empty;
                            if (!string.IsNullOrEmpty(HtmlContentUrl))
                                break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(HtmlContentUrl))
                {
                    var sectionList = await _cmsClient.Post<GetComponentListResponseDto>(CmsConstants.FAQ_SECTION_RETRIVE_URL, sectionListRequest);
                    if (sectionList.Components != null)
                    {
                        _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Started Processing Faq-sectionCollection for TenantCode : {TenantCode} and sectionCollection: {SectionCollection}",
                            className, methodName, sectionListRequest.TenantCode, sectionListRequest.ComponentName);
                        foreach (var section in sectionList.Components)
                        {
                            var cmsComponentDataJson = string.IsNullOrEmpty(section.DataJson)
                                  ? new CmsComponentData()
                                  : JsonConvert.DeserializeObject<CmsComponentData>(section.DataJson);

                            var faqSection = new FaqSectionDto
                            {
                                HeaderText = cmsComponentDataJson?.Data?.Details?.HeaderText,
                                FaqItems = new List<FaqItemDto>()
                            };

                            var itemListRequest = new ComponentListRequestDto
                            {
                                TenantCode = faqRetriveRequestDto.TenantCode,
                                ComponentName = cmsComponentDataJson?.Data?.Details?.SectionCollectionName,
                                LanguageCode = faqRetriveRequestDto.LanguageCode

                            };

                            var itemList = await _cmsClient.Post<GetComponentListResponseDto>(CmsConstants.FAQ_SECTION_RETRIVE_URL, itemListRequest);

                            if (string.IsNullOrEmpty(faqRetriveRequestDto.ConsumerCode))
                            {
                                faqRetriveRequestDto.ConsumerCode = _commonHelperService.GetUserConsumerCodeFromHttpContext();
                            }

                            // Filters FAQ items (questions & answers) based on cohort
                            if (faqRetriveRequestDto.ApplyCohortFilter ?? false)
                            {
                                itemList.Components = await GetFilteredCohortComponents(itemList.Components, faqRetriveRequestDto.TenantCode, faqRetriveRequestDto.ConsumerCode, cmsComponentDataJson?.Data?.Details?.SectionCollectionName);
                            }
                            if (itemList.Components != null)
                            {
                                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} -  Started Processing Faq-items  for TenantCode: {TenantCode} and Faq-Items: {FaqItems}",
                                    className, methodName, itemListRequest.TenantCode, itemListRequest.ComponentName);
                                foreach (var component in itemList.Components)
                                {

                                    var cmsItemDataJson = string.IsNullOrEmpty(component.DataJson)
                                 ? new CmsComponentData()
                                 : JsonConvert.DeserializeObject<CmsComponentData>(component.DataJson);
                                    var faqItem = new FaqItemDto
                                    {
                                        HeaderText = cmsItemDataJson?.Data?.Details?.HeaderText,
                                        DescriptionText = cmsItemDataJson?.Data?.Details?.DescriptionText
                                    };
                                    faqSection.FaqItems.Add(faqItem);
                                }
                            }
                            faqSectionResponse.FaqSections.Add(faqSection);
                        }
                    }
                }
                faqSectionResponse.HtmlContentUrl = HtmlContentUrl;
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved GetFaqSection Successfully for TenantCode : {TenantCode}", className, methodName, faqRetriveRequestDto.TenantCode);
                return faqSectionResponse;

            }
            catch (Exception ex)
            {
                _cmsServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing Faq for TenantCode:{TenantCode}, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, faqRetriveRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<GetComponentResponseDto> GetComponent(GetComponentRequestDto getComponentRequestDto)
        {
            const string methodName = nameof(GetFaqSection);
            try
            {
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Started Processing for TenantCode:{TenantCode}", className, methodName, getComponentRequestDto.TenantCode);
                var componentResponse = await _cmsClient.Post<GetComponentResponseDto>(CmsConstants.GET_COMPONENT_API_URL, getComponentRequestDto);
                if (componentResponse.ErrorCode != null)
                {
                    return componentResponse;
                }
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved component Successfully for TenantCode : {TenantCode}", className, methodName, getComponentRequestDto.TenantCode);
                return componentResponse;
            }
            catch (Exception ex)
            {
                _cmsServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing Faq for TenantCode:{TenantCode}, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, getComponentRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
        public async Task<GetComponentByCodeResponseDto> GetComponentBycode(GetComponentByCodeRequestDto getComponentRequestDto)
        {
            const string methodName = nameof(GetComponentBycode);
            try
            {
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Started Processing for componentCode:{componentCode}", className, methodName, getComponentRequestDto.componentCode);
                var componentResponse = await _cmsClient.Post<GetComponentByCodeResponseDto>(CmsConstants.GET_COMPONENT_BY_CODE_API_URL, getComponentRequestDto);
                if (componentResponse.ErrorCode != null)
                {
                    return componentResponse;
                }
                _cmsServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved component Successfully for componentCode : {componentCode}", className, methodName, getComponentRequestDto.componentCode);
                return componentResponse;
            }
            catch (Exception ex)
            {
                _cmsServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing Faq for componentCode:{componentCode}, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, getComponentRequestDto.componentCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<GetComponentsResponseDto> GetTenantComponentsByTypeName(GetTenantComponentByTypeNameRequestDto requestDto)
        {
            const string methodName = nameof(GetTenantComponentsByTypeName);
            try
            {
                _cmsServiceLogger.LogInformation(
                    "{ClassName}.{MethodName} - Started Processing for TenantCode:{TenantCode}",
                    className, methodName, requestDto.TenantCode);

                requestDto.ConsumerCode ??= _commonHelperService.GetUserConsumerCodeFromHttpContext();

                var responseDto = await FetchComponentsFromCms(requestDto);

                if (responseDto.ErrorCode != null)
                {
                    _cmsServiceLogger.LogError(
                        "{ClassName}.{MethodName} - Error retrieving components for TenantCode: {TenantCode}, ErrorCode: {ErrorCode}, ERROR: {Msg}",
                        className, methodName, requestDto.TenantCode, responseDto.ErrorCode, responseDto.ErrorMessage);

                    return CreateErrorResponse(responseDto.ErrorCode, responseDto.ErrorMessage);
                }

                if (requestDto.ApplyCohortFilter ?? false)
                {
                    if (ExactCohortMatchComponentTypeList.Contains(requestDto.ComponentTypeName))
                    {
                        responseDto.Components = await GetExactMatchCohortComponents(
                           responseDto.Components,
                           requestDto.TenantCode,
                           requestDto.ConsumerCode,
                           requestDto.ComponentTypeName);
                    }
                    else
                    {
                        responseDto.Components = await GetFilteredCohortComponents(
                            responseDto.Components,
                            requestDto.TenantCode,
                            requestDto.ConsumerCode,
                            requestDto.ComponentTypeName);
                    }
                }

                _cmsServiceLogger.LogInformation(
                    "{ClassName}.{MethodName} - Retrieved component successfully for TenantCode: {TenantCode}",
                    className, methodName, requestDto.TenantCode);

                return responseDto;
            }
            catch (Exception ex)
            {
                _cmsServiceLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName} - Error occurred while processing component for TenantCode:{TenantCode}, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, requestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                throw;
            }
        }


        private async Task<List<ComponentDto>> GetExactMatchCohortComponents(IList<ComponentDto>? componentList, string? tenantCode, string? consumerCode, string? componentName)
        {
            const string methodName = nameof(GetExactMatchCohortComponents);

            try
            {
                if (componentList == null || componentList.Count == 0)
                {
                    return new List<ComponentDto>();
                }

                var filteredComponents = new List<ComponentDto>();


                var consumerEnrolledCohorts = await GetConsumerEnrolledCohortsAsync(
                       tenantCode!,
                       consumerCode!
                   );

                foreach (var component in componentList)
                {

                    if (IsMetadataEmpty(component.MetadataJson))
                    {
                        filteredComponents.Add(component);
                        continue;
                    }

                    var tagCohorts = ExtractCohortsFromMetadata(component.MetadataJson!);

                    if (tagCohorts.Count == 0)
                    {
                        filteredComponents.Add(component);
                        continue;
                    }

                    // Global match
                    if (tagCohorts.Any(t =>
                        t.Equals(CommonConstants.ALL, StringComparison.OrdinalIgnoreCase) ||
                        t.Equals(CommonConstants.EVERYONE, StringComparison.OrdinalIgnoreCase)))
                    {
                        filteredComponents.Add(component);
                        continue;
                    }

                    bool isMatch = tagCohorts.All(cohort =>
                    consumerEnrolledCohorts.Contains(cohort, StringComparer.OrdinalIgnoreCase));

                    if (isMatch)
                    {
                        filteredComponents.Add(component);
                    }

                }

                return filteredComponents;
            }
            catch (Exception ex)
            {
                _cmsServiceLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName} - Error filtering cohort components for TenantCode: {TenantCode}, ComponentName: {ComponentName}, ErrorCode: {ErrorCode}, ERROR: {Msg}",
                    className, methodName, tenantCode, componentName, StatusCodes.Status500InternalServerError, ex.Message);

                throw;
            }
        }

        private async Task<List<string?>> GetConsumerEnrolledCohortsAsync(
            string tenantCode,
            string consumerCode)
        {
            var cohortList = await _cohortConsumerService.GetConsumerAllCohorts(tenantCode, consumerCode);

            // Handle null or empty cohorts gracefully
            if (cohortList?.Cohorts == null || cohortList.Cohorts.Count == 0)
            {
                return new List<string?>();
            }

            return cohortList.Cohorts
                .Select(x => x.CohortName)
                .ToList();
        }

        private async Task<GetComponentsResponseDto> FetchComponentsFromCms(GetTenantComponentByTypeNameRequestDto requestDto)
        {
            var tenantComponentByNameRequestDto = new TenantComponentByTypeNameRequestDto
            {
                TenantCode = requestDto.TenantCode,
                ComponentTypeName = requestDto.ComponentTypeName,
                LanguageCode = requestDto.LanguageCode
            };

            return await _cmsClient.Post<GetComponentsResponseDto>(
                CmsConstants.GET_TENANT_COMPONENTS_BY_TYPE_NAME_API_URL, tenantComponentByNameRequestDto);
        }


        public async Task<GetTenantComponentsByTypeNamesResponseDto> GetTenantComponentsByTypeNames(GetTenantComponentsByTypeNamesRequestDto requestDto)
        {
            const string methodName = nameof(GetTenantComponentsByTypeNames);
            var response = new GetTenantComponentsByTypeNamesResponseDto();
            
            try
            {
                _cmsServiceLogger.LogInformation(
                    "{ClassName}.{MethodName} - Started Processing for TenantCode: {TenantCode}, ComponentTypeNames: {ComponentTypeNames}",
                    className, methodName, requestDto.TenantCode, string.Join(", ", requestDto.ComponentTypeNames));

                // Process each component type name
                foreach (var componentTypeName in requestDto.ComponentTypeNames)
                {
                    try
                    {
                        var singleTypeRequest = new GetTenantComponentByTypeNameRequestDto
                        {
                            TenantCode = requestDto.TenantCode,
                            ComponentTypeName = componentTypeName,
                            LanguageCode = requestDto.LanguageCode,
                            ConsumerCode = requestDto.ConsumerCode,
                            ApplyCohortFilter = requestDto.ApplyCohortFilter
                        };

                        var singleTypeResponse = await GetTenantComponentsByTypeName(singleTypeRequest);

                        // Add components to the dictionary, even if there's an error for this specific type
                        // We'll include empty list for failed types so the client knows which types failed
                        if (singleTypeResponse.ErrorCode == null)
                        {
                            response.ComponentsByType[componentTypeName] = singleTypeResponse.Components?.ToList() ?? new List<ComponentDto>();
                            _cmsServiceLogger.LogInformation(
                                "{ClassName}.{MethodName} - Successfully retrieved {Count} components for ComponentTypeName: {ComponentTypeName}",
                                className, methodName, singleTypeResponse.Components?.Count ?? 0, componentTypeName);
                        }
                        else
                        {
                            // Log error but continue processing other types
                            _cmsServiceLogger.LogWarning(
                                "{ClassName}.{MethodName} - Error retrieving components for ComponentTypeName: {ComponentTypeName}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                                className, methodName, componentTypeName, singleTypeResponse.ErrorCode, singleTypeResponse.ErrorMessage);
                            response.ComponentsByType[componentTypeName] = new List<ComponentDto>();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error for this specific type but continue with others
                        _cmsServiceLogger.LogError(
                            ex,
                            "{ClassName}.{MethodName} - Exception occurred while processing ComponentTypeName: {ComponentTypeName}, Error: {Error}",
                            className, methodName, componentTypeName, ex.Message);
                        response.ComponentsByType[componentTypeName] = new List<ComponentDto>();
                    }
                }

                _cmsServiceLogger.LogInformation(
                    "{ClassName}.{MethodName} - Completed Processing for TenantCode: {TenantCode}, Retrieved {TypeCount} component types",
                    className, methodName, requestDto.TenantCode, response.ComponentsByType.Count);

                return response;
            }
            catch (Exception ex)
            {
                _cmsServiceLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName} - Error occurred while processing multiple component types for TenantCode: {TenantCode}, ErrorCode: {ErrorCode}, ERROR: {Msg}",
                    className, methodName, requestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                response.ErrorCode = StatusCodes.Status500InternalServerError;
                response.ErrorMessage = $"An error occurred while processing component types: {ex.Message}";
                return response;
            }
        }

        private static GetComponentsResponseDto CreateErrorResponse(int? errorCode, string? errorMessage)
        {
            return new GetComponentsResponseDto
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
