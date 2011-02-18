using System.Collections.Generic;

public class Statistics {
    public int TotalCount { get; set; }
    public int UniqueCount { get; set; }
    public int TotalDownloads { get; set; }
    public IEnumerable<dynamic> LatestPackages { get; set; }
    public IEnumerable<dynamic> TopPackages { get; set; }
}
