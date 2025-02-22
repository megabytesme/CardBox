using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

public static class LocationService
{
    private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org/search";

    public static async Task<List<Location>> GetNearbyLocationsAsync(double latitude, double longitude, string query, int radiusInKm = 20, double mergeDistanceKm = 0.1)
    {
        var locations = new List<Location>();
        using (var client = new HttpClient())
        {
            var bbox = CalculateBoundingBox(latitude, longitude, radiusInKm);
            var url = $"{NominatimBaseUrl}?q={Uri.EscapeDataString(query)}&format=xml&polygon_kml=1&addressdetails=1&viewbox={bbox}&bounded=1";

            client.DefaultRequestHeaders.UserAgent.ParseAdd("CardBox/1.0");

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var xdoc = XDocument.Parse(content);
                foreach (var element in xdoc.Descendants("place"))
                {
                    if (double.TryParse(element.Attribute("lat")?.Value, out double lat) &&
                        double.TryParse(element.Attribute("lon")?.Value, out double lon))
                    {
                        var displayName = element.Attribute("display_name")?.Value;

                        var name = displayName;
                        if (!string.IsNullOrEmpty(displayName))
                        {
                            var commaIndex = displayName.IndexOf(',');
                            if (commaIndex > 0)
                            {
                                name = displayName.Substring(0, commaIndex).Trim();
                            }
                        }

                        var location = new Location
                        {
                            Name = name,
                            Latitude = lat,
                            Longitude = lon,
                            Address = element.Element("road")?.Value + ", " +
                                      element.Element("suburb")?.Value + ", " +
                                      element.Element("town")?.Value + ", " +
                                      element.Element("county")?.Value + ", " +
                                      element.Element("postcode")?.Value + ", " +
                                      element.Element("country")?.Value
                        };
                        locations.Add(location);
                    }
                }
            }

            return MergeNearbyLocations(locations, mergeDistanceKm);
        }
    }

    private static string CalculateBoundingBox(double latitude, double longitude, int radiusInKm)
    {
        const double EarthRadiusKm = 6371.0;
        double latRadian = latitude * (Math.PI / 180.0);
        double lonDegree = (radiusInKm / EarthRadiusKm) * (180.0 / Math.PI) / Math.Cos(latRadian);
        double latDegree = radiusInKm / EarthRadiusKm * (180.0 / Math.PI);

        double northLat = latitude + latDegree;
        double southLat = latitude - latDegree;
        double eastLon = longitude + lonDegree;
        double westLon = longitude - lonDegree;

        return $"{westLon},{northLat},{eastLon},{southLat}";
    }

    public static List<Location> MergeNearbyLocations(List<Location> locations, double maxDistance = 0.1)
    {
        var mergedLocations = new List<Location>();
        var processed = new HashSet<Location>();

        foreach (var location in locations)
        {
            if (processed.Contains(location)) continue;

            var nearbyLocations = new List<Location> { location };
            processed.Add(location);

            foreach (var otherLocation in locations)
            {
                if (otherLocation == location || processed.Contains(otherLocation)) continue;

                if (CalculateDistance(location, otherLocation) <= maxDistance)
                {
                    nearbyLocations.Add(otherLocation);
                    processed.Add(otherLocation);
                }
            }

            if (nearbyLocations.Count > 1)
            {
                var mergedLatitude = nearbyLocations.Average(l => l.Latitude);
                var mergedLongitude = nearbyLocations.Average(l => l.Longitude);

                var mergedName = nearbyLocations[0].Name;

                string mergedAddress = null;
                if (nearbyLocations.Any() && !string.IsNullOrEmpty(nearbyLocations[0].Address))
                {
                    mergedAddress = nearbyLocations[0].Address.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                }

                mergedLocations.Add(new Location
                {
                    Name = mergedName,
                    Latitude = mergedLatitude,
                    Longitude = mergedLongitude,
                    Address = mergedAddress
                });
            }
            else
            {
                mergedLocations.Add(location);
            }
        }

        return mergedLocations;
    }

    public static double CalculateDistance(Location loc1, Location loc2)
    {
        var earthRadiusKm = 6371;
        var dLat = DegreesToRadians(loc2.Latitude - loc1.Latitude);
        var dLon = DegreesToRadians(loc2.Longitude - loc1.Longitude);

        var lat1Rad = DegreesToRadians(loc1.Latitude);
        var lat2Rad = DegreesToRadians(loc2.Latitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1Rad) * Math.Cos(lat2Rad);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public class Location
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }
}