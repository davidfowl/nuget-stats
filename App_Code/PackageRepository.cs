using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using NuGet;
using HttpUtility = System.Web.HttpUtility;

public static class PackageRepository {
    private static readonly DataServicePackageRepository _repository = new DataServicePackageRepository(new Uri("http://packages.nuget.org/v1/FeedService.svc"));
    internal static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(2);

    public static Statistics GetCurrentStatistics(Cache cache) {
        return cache.GetOrCreate<Statistics>("repositoryStats", GetStatisticsInternal, CacheTime);
    }

    private static Statistics GetStatisticsInternal() {
        var packages = _repository.GetPackages();
        var uniquePackages = packages.Cast<DataServicePackage>().Where(p => p.IsLatestVersion).ToList();

        var totalDownloads = uniquePackages.Sum(p => p.DownloadCount);
        var totalPackagesCount = packages.Count();
        var uniquePackagesCount = uniquePackages.Count;
        var latestPackages = uniquePackages.OrderByDescending(p => p.LastUpdated).Take(5);
        var topPackages = uniquePackages.OrderByDescending(p => p.DownloadCount).Take(5);

        return new Statistics {
            TotalCount = totalPackagesCount,
            UniqueCount = uniquePackagesCount,
            TotalDownloads = totalDownloads,
            LatestPackages = latestPackages.Select(GetStatsPackage),
            TopPackages = topPackages.Select(GetStatsPackage)
        };
    }

    private static StatsPackage GetStatsPackage(DataServicePackage package) {
        return new StatsPackage {
            Id = package.Id,
            Version = package.Version,
            DownloadCount = package.DownloadCount,
            // Workaround for the gallery url until it is fixed.
            Url = String.Format("http://nuget.org/List/Packages/{0}/{1}", HttpUtility.UrlPathEncode(package.Id), HttpUtility.UrlPathEncode(package.Version.ToString()))
        };
    }
}