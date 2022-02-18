using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace MajaMobile.Models
{
    public class Airline
    {
        public string FsCode { get; }
        public string Iata { get; }
        public string Icao { get; }
        public string Name { get; }

        public Airline(JsonElement jobj)
        {
            JsonElement token;
            if (jobj.TryGetProperty("fs", out token))
                FsCode = token.GetString();
            if (jobj.TryGetProperty("iata", out token))
                Iata = token.GetString();
            if (jobj.TryGetProperty("icao", out token))
                Icao = token.GetString();
            if (jobj.TryGetProperty("name", out token))
                Name = token.GetString();
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

        public Airport(JsonElement jobj)
        {
            JsonElement token;
            if (jobj.TryGetProperty("fs", out token))
                FsCode = token.GetString();
            if (jobj.TryGetProperty("iata", out token))
                Iata = token.GetString();
            if (jobj.TryGetProperty("icao", out token))
                Icao = token.GetString();
            if (jobj.TryGetProperty("name", out token))
                Name = token.GetString();
            if (jobj.TryGetProperty("city", out token))
                City = token.GetString();
            if (jobj.TryGetProperty("cityCode", out token))
                CityCode = token.GetString();
            if (jobj.TryGetProperty("countryName", out token))
                Country = token.GetString();
            if (jobj.TryGetProperty("countryCode", out token))
                CountryCode = token.GetString();
            if (jobj.TryGetProperty("regionName", out token))
                Region = token.GetString();
            if (jobj.TryGetProperty("timeZoneRegionName", out token))
                TimeZoneRegionName = token.GetString();
            if (jobj.TryGetProperty("localTime", out token))
            {
                LocalTime = FlightStatus.TokenToDateTime(token);
            }
            if (jobj.TryGetProperty("latitude", out token))
                Latitude = token.GetDouble();
            if (jobj.TryGetProperty("longitude", out token))
                Longitude = token.GetDouble();
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

        public FlightStatus(JsonElement jobj, IEnumerable<Airline> airlines, IEnumerable<Airport> airports)
        {
            JsonElement token;
            if (jobj.TryGetProperty("flightId", out token))
                FlightId = token.GetInt64();
            if (jobj.TryGetProperty("carrierFsCode", out token))
                CarrierFsCode = token.GetString();
            if (jobj.TryGetProperty("flightNumber", out token))
                FlightNumber = token.GetString();
            if (jobj.TryGetProperty("departureAirportFsCode", out token))
                DepartureAirportFsCode = token.GetString();
            if (jobj.TryGetProperty("arrivalAirportFsCode", out token))
                ArrivalAirportFsCode = token.GetString();
            if (jobj.TryGetProperty("status", out token))
                Status = token.GetString();
            if (jobj.TryGetProperty("departureDate", out token))
                DepartureDate = GetDateLocal(token);
            if (jobj.TryGetProperty("arrivalDate", out token))
                ArrivalDate = GetDateLocal(token);
            if (jobj.TryGetProperty("operationalTimes", out var operationTimes))
            {
                if (operationTimes.TryGetProperty("publishedDeparture", out token))
                    PublishedDeparture = GetDateLocal(token);
                if (operationTimes.TryGetProperty("publishedArrival", out token))
                    PublishedArrival = GetDateLocal(token);

                if (operationTimes.TryGetProperty("actualGateDeparture", out token))
                {
                    ActualGateDeparture = GetDateLocal(token);
                    GateDepartureUtc = GetDateUtc(token);
                }
                if (operationTimes.TryGetProperty("scheduledGateDeparture", out token))
                {
                    ScheduledGateDeparture = GetDateLocal(token);
                    if (!GateDepartureUtc.HasValue)
                    {
                        GateDepartureUtc = GetDateUtc(token);
                    }
                }
                if (operationTimes.TryGetProperty("estimatedGateDeparture", out token))
                    EstimatedGateDeparture = GetDateLocal(token);

                if (operationTimes.TryGetProperty("scheduledRunwayDeparture", out token))
                    ScheduledRunwayDeparture = GetDateLocal(token);
                if (operationTimes.TryGetProperty("estimatedRunwayDeparture", out token))
                    EstimatedRunwayDeparture = GetDateLocal(token);
                if (operationTimes.TryGetProperty("actualRunwayDeparture", out token))
                    ActualRunwayDeparture = GetDateLocal(token);

                if (operationTimes.TryGetProperty("estimatedGateArrival", out token))
                {
                    EstimatedGateArrival = GetDateLocal(token);
                    GateArrivalUtc = GetDateUtc(token);
                }
                if (operationTimes.TryGetProperty("scheduledGateArrival", out token))
                {
                    ScheduledGateArrival = GetDateLocal(token);
                    if (!GateArrivalUtc.HasValue)
                    {
                        GateArrivalUtc = GetDateUtc(token);
                    }
                }
                if (operationTimes.TryGetProperty("actualGateArrival", out token))
                    ActualGateArrival = GetDateLocal(token);

                if (operationTimes.TryGetProperty("flightPlanPlannedDeparture", out token))
                    FlightPlanPlannedDeparture = GetDateLocal(token);
                if (operationTimes.TryGetProperty("flightPlanPlannedArrival", out token))
                    FlightPlanPlannedArrival = GetDateLocal(token);

                if (operationTimes.TryGetProperty("scheduledRunwayArrival", out token))
                    ScheduledRunwayArrival = GetDateLocal(token);
                if (operationTimes.TryGetProperty("estimatedRunwayArrival", out token))
                    EstimatedRunwayArrival = GetDateLocal(token);
                if (operationTimes.TryGetProperty("actualRunwayArrival", out token))
                    ActualRunwayArrival = GetDateLocal(token);
            }

            if (jobj.TryGetProperty("delays", out var delays))
            {
                if (delays.TryGetProperty("departureGateDelayMinutes", out token))
                    DepartureGateDelayMinutes = token.GetInt32();
                if (delays.TryGetProperty("departureRunwayDelayMinutes", out token))
                    DepartureRunwayDelayMinutes = token.GetInt32();
                if (delays.TryGetProperty("arrivalGateDelayMinutes", out token))
                    ArrivalGateDelayMinutes = token.GetInt32();
                if (delays.TryGetProperty("arrivalRunwayDelayMinutes", out token))
                    ArrivalRunwayDelayMinutes = token.GetInt32();
            }

            if (jobj.TryGetProperty("airportResources", out var airportResources))
            {
                if (airportResources.TryGetProperty("departureTerminal", out token))
                    DepartureTerminal = token.GetString();
                if (airportResources.TryGetProperty("departureGate", out token))
                    DepartureGate = token.GetString();
                if (airportResources.TryGetProperty("arrivalTerminal", out token))
                    ArrivalTerminal = token.GetString();
                if (airportResources.TryGetProperty("arrivalGate", out token))
                    ArrivalGate = token.GetString();
            }

            Airline = airlines.FirstOrDefault(a => a.FsCode == CarrierFsCode);
            DepartureAirport = airports.FirstOrDefault(a => a.FsCode == DepartureAirportFsCode);
            ArrivalAirport = airports.FirstOrDefault(a => a.FsCode == ArrivalAirportFsCode);
        }

        private DateTime? GetDateLocal(JsonElement jobj)
        {
            if (jobj.TryGetProperty("dateLocal", out var token))
                return TokenToDateTime(token);
            return null;
        }

        internal static DateTime? TokenToDateTime(JsonElement token)
        {
            if (token.TryGetDateTime(out var date))
                return date;
            if (DateTime.TryParseExact(token.GetString(), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateTime))
                return dateTime;
            return null;
        }

        private DateTime? GetDateUtc(JsonElement jobj)
        {
            if (jobj.TryGetProperty("dateUtc", out var token))
            {
                if (token.TryGetDateTime(out var date))
                    return date;
                if (DateTime.TryParseExact(token.GetString(), DateTimeFormatUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
                    return dateTime;
            }
            return null;
        }

        public static IEnumerable<FlightStatus> GetFlightStatusesFromJson(string data)
        {
            var flightStatuses = new List<FlightStatus>();
            var airports = new List<Airport>();
            var airlines = new List<Airline>();
            var doc = JsonDocument.Parse(data);
            JsonElement token;
            if (doc.RootElement.TryGetProperty("appendix", out var appendix))
            {
                if (appendix.TryGetProperty("airlines", out token))
                {
                    foreach (var airline in token.EnumerateArray())
                    {
                        airlines.Add(new Airline(airline));
                    }
                }
                if (appendix.TryGetProperty("airports", out token))
                {
                    foreach (var airport in token.EnumerateArray())
                    {
                        airports.Add(new Airport(airport));
                    }
                }
            }
            if (doc.RootElement.TryGetProperty("flightStatuses", out token))
            {
                foreach (var status in token.EnumerateArray())
                {
                    flightStatuses.Add(new FlightStatus(status, airlines, airports));
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