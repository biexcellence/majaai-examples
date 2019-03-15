using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MajaMobile.Models
{
    public class Airline
    {
        public string FsCode { get; }
        public string Iata { get; }
        public string Icao { get; }
        public string Name { get; }

        public Airline(JObject jobj)
        {
            JToken token = null;
            if (jobj.TryGetValue("fs", out token))
                FsCode = (string)token;
            if (jobj.TryGetValue("iata", out token))
                Iata = (string)token;
            if (jobj.TryGetValue("icao", out token))
                Icao = (string)token;
            if (jobj.TryGetValue("name", out token))
                Name = (string)token;
        }
    }

    public class Airport
    {
        public string FsCode { get; }
        public string Iata { get; }
        public string Icao { get; }
        public string Name { get; }
        public string City { get; }
        public string CityCode { get; }
        public string Country { get; }
        public string CountryCode { get; }
        public string Region { get; }
        public string TimeZoneRegionName { get; }
        public DateTime? LocalTime { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public Airport(JObject jobj)
        {
            JToken token = null;
            if (jobj.TryGetValue("fs", out token))
                FsCode = (string)token;
            if (jobj.TryGetValue("iata", out token))
                Iata = (string)token;
            if (jobj.TryGetValue("icao", out token))
                Icao = (string)token;
            if (jobj.TryGetValue("name", out token))
                Name = (string)token;
            if (jobj.TryGetValue("city", out token))
                City = (string)token;
            if (jobj.TryGetValue("cityCode", out token))
                CityCode = (string)token;
            if (jobj.TryGetValue("countryName", out token))
                Country = (string)token;
            if (jobj.TryGetValue("countryCode", out token))
                CountryCode = (string)token;
            if (jobj.TryGetValue("regionName", out token))
                Region = (string)token;
            if (jobj.TryGetValue("timeZoneRegionName", out token))
                TimeZoneRegionName = (string)token;
            if (jobj.TryGetValue("localTime", out token))
            {
                LocalTime = FlightStatus.TokenToDateTime(token);
            }
            if (jobj.TryGetValue("latitude", out token))
                Latitude = (double)token;
            if (jobj.TryGetValue("longitude", out token))
                Longitude = (double)token;
        }
    }

    public class FlightStatus
    {
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";
        private const string DateTimeFormatUtc = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public DateTime DateTimeUtc { get; } = DateTime.UtcNow;

        public long FlightId { get; }
        public string CarrierFsCode { get; }
        public string FlightNumber { get; }
        public string DepartureAirportFsCode { get; }
        public string ArrivalAirportFsCode { get; }
        public string Status { get; }

        public DateTime? DepartureDate { get; }
        public DateTime? ArrivalDate { get; }

        public DateTime? PublishedDeparture { get; }
        public DateTime? PublishedArrival { get; }

        public DateTime? ScheduledGateDeparture { get; }
        public DateTime? EstimatedGateDeparture { get; }
        public DateTime? ActualGateDeparture { get; }
        public DateTime? GateDepartureUtc { get; }

        public DateTime? ScheduledRunwayDeparture { get; }
        public DateTime? EstimatedRunwayDeparture { get; }
        public DateTime? ActualRunwayDeparture { get; }

        public DateTime? ScheduledGateArrival { get; }
        public DateTime? EstimatedGateArrival { get; }
        public DateTime? GateArrivalUtc { get; }
        public DateTime? ActualGateArrival { get; }

        public DateTime? FlightPlanPlannedDeparture { get; }
        public DateTime? FlightPlanPlannedArrival { get; }

        public DateTime? ScheduledRunwayArrival { get; }
        public DateTime? EstimatedRunwayArrival { get; }
        public DateTime? ActualRunwayArrival { get; }

        public int DepartureGateDelayMinutes { get; }
        public int DepartureRunwayDelayMinutes { get; }
        public int ArrivalGateDelayMinutes { get; }
        public int ArrivalRunwayDelayMinutes { get; }

        public string DepartureTerminal { get; } = "-";
        public string DepartureGate { get; } = "-";
        public string ArrivalTerminal { get; } = "-";
        public string ArrivalGate { get; } = "-";

        public Airline Airline { get; }
        public Airport DepartureAirport { get; }
        public Airport ArrivalAirport { get; }

        public FlightStatus(JObject jobj, IEnumerable<Airline> airlines, IEnumerable<Airport> airports)
        {
            JToken token = null;
            if (jobj.TryGetValue("flightId", out token))
                FlightId = (long)token;
            if (jobj.TryGetValue("carrierFsCode", out token))
                CarrierFsCode = (string)token;
            if (jobj.TryGetValue("flightNumber", out token))
                FlightNumber = (string)token;
            if (jobj.TryGetValue("departureAirportFsCode", out token))
                DepartureAirportFsCode = (string)token;
            if (jobj.TryGetValue("arrivalAirportFsCode", out token))
                ArrivalAirportFsCode = (string)token;
            if (jobj.TryGetValue("status", out token))
                Status = (string)token;
            if (jobj.TryGetValue("departureDate", out token))
                DepartureDate = GetDateLocal((JObject)token);
            if (jobj.TryGetValue("arrivalDate", out token))
                ArrivalDate = GetDateLocal((JObject)token);
            if (jobj.TryGetValue("operationalTimes", out var operationTimesToken))
            {
                var operationTimes = (JObject)operationTimesToken;
                if (operationTimes.TryGetValue("publishedDeparture", out token))
                    PublishedDeparture = GetDateLocal((JObject)token);
                if (operationTimes.TryGetValue("publishedArrival", out token))
                    PublishedArrival = GetDateLocal((JObject)token);

                if (operationTimes.TryGetValue("actualGateDeparture", out token))
                {
                    ActualGateDeparture = GetDateLocal((JObject)token);
                    GateDepartureUtc = GetDateUtc((JObject)token);
                }
                if (operationTimes.TryGetValue("scheduledGateDeparture", out token))
                {
                    ScheduledGateDeparture = GetDateLocal((JObject)token);
                    if (!GateDepartureUtc.HasValue)
                    {
                        GateDepartureUtc = GetDateUtc((JObject)token);
                    }
                }
                if (operationTimes.TryGetValue("estimatedGateDeparture", out token))
                    EstimatedGateDeparture = GetDateLocal((JObject)token);

                if (operationTimes.TryGetValue("scheduledRunwayDeparture", out token))
                    ScheduledRunwayDeparture = GetDateLocal((JObject)token);
                if (operationTimes.TryGetValue("estimatedRunwayDeparture", out token))
                    EstimatedRunwayDeparture = GetDateLocal((JObject)token);
                if (operationTimes.TryGetValue("actualRunwayDeparture", out token))
                    ActualRunwayDeparture = GetDateLocal((JObject)token);

                if (operationTimes.TryGetValue("estimatedGateArrival", out token))
                {
                    EstimatedGateArrival = GetDateLocal((JObject)token);
                    GateArrivalUtc = GetDateUtc((JObject)token);
                }
                if (operationTimes.TryGetValue("scheduledGateArrival", out token))
                {
                    ScheduledGateArrival = GetDateLocal((JObject)token);
                    if (!GateArrivalUtc.HasValue)
                    {
                        GateArrivalUtc = GetDateUtc((JObject)token);
                    }
                }
                if (operationTimes.TryGetValue("actualGateArrival", out token))
                    ActualGateArrival = GetDateLocal((JObject)token);

                if (operationTimes.TryGetValue("flightPlanPlannedDeparture", out token))
                    FlightPlanPlannedDeparture = GetDateLocal((JObject)token);
                if (operationTimes.TryGetValue("flightPlanPlannedArrival", out token))
                    FlightPlanPlannedArrival = GetDateLocal((JObject)token);

                if (operationTimes.TryGetValue("scheduledRunwayArrival", out token))
                    ScheduledRunwayArrival = GetDateLocal((JObject)token);
                if (operationTimes.TryGetValue("estimatedRunwayArrival", out token))
                    EstimatedRunwayArrival = GetDateLocal((JObject)token);
                if (operationTimes.TryGetValue("actualRunwayArrival", out token))
                    ActualRunwayArrival = GetDateLocal((JObject)token);
            }

            if (jobj.TryGetValue("delays", out var delaysToken))
            {
                var delays = (JObject)delaysToken;
                if (delays.TryGetValue("departureGateDelayMinutes", out token))
                    DepartureGateDelayMinutes = (int)token;
                if (delays.TryGetValue("departureRunwayDelayMinutes", out token))
                    DepartureRunwayDelayMinutes = (int)token;
                if (delays.TryGetValue("arrivalGateDelayMinutes", out token))
                    ArrivalGateDelayMinutes = (int)token;
                if (delays.TryGetValue("arrivalRunwayDelayMinutes", out token))
                    ArrivalRunwayDelayMinutes = (int)token;
            }

            if (jobj.TryGetValue("airportResources", out var airportResourcesToken))
            {
                var airportResources = (JObject)airportResourcesToken;
                if (airportResources.TryGetValue("departureTerminal", out token))
                    DepartureTerminal = (string)token;
                if (airportResources.TryGetValue("departureGate", out token))
                    DepartureGate = (string)token;
                if (airportResources.TryGetValue("arrivalTerminal", out token))
                    ArrivalTerminal = (string)token;
                if (airportResources.TryGetValue("arrivalGate", out token))
                    ArrivalGate = (string)token;
            }

            Airline = airlines.FirstOrDefault(a => a.FsCode == CarrierFsCode);
            DepartureAirport = airports.FirstOrDefault(a => a.FsCode == DepartureAirportFsCode);
            ArrivalAirport = airports.FirstOrDefault(a => a.FsCode == ArrivalAirportFsCode);
        }

        private DateTime? GetDateLocal(JObject jobj)
        {
            if (jobj.TryGetValue("dateLocal", out var token))
                return TokenToDateTime(token);
            return null;
        }

        internal static DateTime? TokenToDateTime(JToken token)
        {
            if (token.Type == JTokenType.Date)
                return (DateTime)token;
            if (DateTime.TryParseExact((string)token, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateTime))
                return dateTime;
            return null;
        }

        private DateTime? GetDateUtc(JObject jobj)
        {
            if (jobj.TryGetValue("dateUtc", out var token))
            {
                if (token.Type == JTokenType.Date)
                    return (DateTime)token;
                if (DateTime.TryParseExact((string)token, DateTimeFormatUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
                    return dateTime;
            }
            return null;
        }

        public static IEnumerable<FlightStatus> GetFlightStatusesFromJson(string data)
        {
            var flightStatuses = new List<FlightStatus>();
            var airports = new List<Airport>();
            var airlines = new List<Airline>();
            var obj = JObject.Parse(data);
            JToken token = null;
            if (obj.TryGetValue("appendix", out var appendixToken))
            {
                var appendix = (JObject)appendixToken;
                if (appendix.TryGetValue("airlines", out token))
                {
                    foreach (var airline in token)
                    {
                        airlines.Add(new Airline((JObject)airline));
                    }
                }
                if (appendix.TryGetValue("airports", out token))
                {
                    foreach (var airport in token)
                    {
                        airports.Add(new Airport((JObject)airport));
                    }
                }
            }
            if (obj.TryGetValue("flightStatuses", out token))
            {
                foreach (var status in token)
                {
                    flightStatuses.Add(new FlightStatus((JObject)status, airlines, airports));
                }
            }
            return flightStatuses;
        }
    }

    public static class FlightStatusCodes
    {
        public const string Active = "A";
        public const string Canceled = "C";
        public const string Diverted = "D";
        public const string DataSourceNeeded = "DN";
        public const string Landed = "L";
        public const string NotOperational = "NO";
        public const string Redirected = "R";
        public const string Scheduled = "S";
        public const string Unknown = "U";
    }
}