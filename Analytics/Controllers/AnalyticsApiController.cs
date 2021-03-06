﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Analytics.Models;
using Skybrud.Social.Google;
using Skybrud.Social.Google.Analytics;
using Skybrud.Social.Google.Analytics.Objects;
using Skybrud.Social.Google.Analytics.Responses;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Analytics.SkybrudSocialExtensionMethods;

namespace Analytics.Controllers
{
    [PluginController("Analytics")]
    public class AnalyticsApiController : UmbracoAuthorizedApiController
    {  
        private GoogleService GetGoogleService() {
            return GoogleService.CreateFromRequestToken(AnalyticsConfig.ClientId, AnalyticsConfig.ClientSecret, AnalyticsConfig.RefreshToken);
        }

        /// <summary>
        /// Get's Accounts on this authenticated user account
        /// </summary>
        /// <returns></returns>
        public AnalyticsAccount[] GetAccounts()
        {
            // Get the accounts from the Google Analytics API
            AnalyticsAccount[] accounts = GetGoogleService().Analytics.GetAccounts().Items;

            return accounts;
        }

        /// <summary>
        /// Get's Profiles on this authenticated user account
        /// </summary>
        /// <returns></returns>
        public AnalyticsProfile[] GetProfiles()
        {

            // Get the profiles from the Google Analytics API
            AnalyticsProfile[] profiles = GetGoogleService().Analytics.GetProfiles().Items;

            // Return the profiles
            return profiles;
        }

        /// <summary>
        /// Get Profiles from a specific Account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public AnalyticsProfile[] GetProfilesFromAccount(string accountId)
        {
            //Get Account
            var account = GetGoogleService().Analytics.GetAccounts().Items.SingleOrDefault(x => x.Id == accountId);

            // Get the profiles from the Google Analytics API
            var profiles = GetGoogleService().Analytics.GetProfiles(account).Items;

            // Return the profiles
            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ChartData GetVisitsOverTime(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            //Span of time
            TimeSpan span = endDate.Value - startDate.Value;

            //Dimensions that changes based on time period
            AnalyticsDimensionCollection dimensions;


            //If less than 60 days show days
            if (span.TotalDays < 60)
            {
                dimensions = AnalyticsDimension.Year + AnalyticsDimension.Month + AnalyticsDimension.Day;
            }
            else
            {
                dimensions = AnalyticsDimension.Year + AnalyticsDimension.Month;
            }

           

            // Get the visits from the Google Analytics API
            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = dimensions,
                Sorting = new AnalyticsSortOptions().AddAscending(AnalyticsDimension.Year)
            });

            //Store API result in our new object along with chart data
            var visitsMonthResult = ChartHelper.GetLineChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return visitsMonthResult;

        }

        /// <summary>
        /// Get Visits
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public AnalyticsDataResponse GetVisits(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
             if (!endDate.HasValue)
                 endDate = DateTime.Now;

            // Get the visits from the Google Analytics API
            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.PagePath,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            // Return the data as JSON
            return data;
        }

        /// <summary>
        /// Get Sources
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public AnalyticsDataResponse GetSources(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.Source,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            // Return the data as JSON
            return data;
        }

        /// <summary>
        /// Get Keywords
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public StatsApiResult GetKeywords(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.Keyword,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var keywordsResult          = new StatsApiResult();
            keywordsResult.ApiResult    = data;                             //The data back from Google's API
            keywordsResult.ChartData    = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return keywordsResult;
        }

        /// <summary>
        /// Get Browser Vendors
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetBrowser(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.Browser,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var browsersResult          = new StatsApiResult();
            browsersResult.ApiResult    = data;                             //The data back from Google's API
            browsersResult.ChartData    = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return browsersResult;
        }

        /// <summary>
        /// Get Browser Specific Version
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetBrowserVersion(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.Browser + AnalyticsDimension.BrowserVersion,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var browserVersionsResult       = new StatsApiResult();
            browserVersionsResult.ApiResult = data;                             //The data back from Google's API
            browserVersionsResult.ChartData = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return browserVersionsResult;
        }

        /// <summary>
        /// Get Device Types
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetDeviceTypes(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.DeviceCategory,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var deviceResult        = new StatsApiResult();
            deviceResult.ApiResult  = data;                             //The data back from Google's API
            deviceResult.ChartData  = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper
            
            // Return the data as JSON
            return deviceResult;
        }

        /// <summary>
        /// Get Devices
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetDevices(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.MobileDeviceBranding + AnalyticsDimension.MobileDeviceModel,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var devicesResult = new StatsApiResult();
            devicesResult.ApiResult = data;                             //The data back from Google's API
            devicesResult.ChartData = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return devicesResult;
        }


        /// <summary>
        /// Get Social Network Sources
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetSocialNetworkSources(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.SocialNetwork,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var socialResult       = new StatsApiResult();
            socialResult.ApiResult = data;                             //The data back from Google's API
            socialResult.ChartData = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return socialResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetOperatingSystems(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.OperatingSystem,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var osResult        = new StatsApiResult();
            osResult.ApiResult  = data;                             //The data back from Google's API
            osResult.ChartData  = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return osResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetOperatingSystemVersions(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.OperatingSystem + AnalyticsDimension.OperatingSystemVersion,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var osVersionResult         = new StatsApiResult();
            osVersionResult.ApiResult   = data;                             //The data back from Google's API
            osVersionResult.ChartData   = ChartHelper.GetChartData(data);   //Add chart data to device result via Helper

            // Return the data as JSON
            return osVersionResult;
        }



        /// <summary>
        /// Get the screen resolutions
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public AnalyticsDataResponse GetScreenRes(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.ScreenResolution,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            // Return the data as JSON
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public StatsApiResult GetCountry(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.Country,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            //Store API result in our new object along with chart data
            var countryResult       = new StatsApiResult();
            countryResult.ApiResult = data;                                 //The data back from Google's API
            countryResult.ChartData = ChartHelper.GetGeoChartData(data);    //Add chart data to device result via Helper

            // Return the data as JSON
            return countryResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public AnalyticsDataResponse GetLanguage(string profile, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.Subtract(TimeSpan.FromDays(31));
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            //Profile, Start Date, End Date, Metrics (Array), Dimensions (Array)

            AnalyticsDataResponse data = GetGoogleService().Analytics.GetData(profile, new AnalyticsDataOptions {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Metrics = AnalyticsMetric.Visits + AnalyticsMetric.Pageviews,
                Dimensions = AnalyticsDimension.Language,
                Sorting = new AnalyticsSortOptions().AddDescending(AnalyticsMetric.Visits)
            });

            // Return the data as JSON
            return data;
        }
        
    }

}
