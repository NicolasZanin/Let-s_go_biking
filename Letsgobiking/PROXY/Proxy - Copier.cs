using System;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static readonly HttpClient client = new HttpClient();

    static async Task test()
    {
        await ContractChoose();
    }

    static async Task ContractChoose()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("https://api.jcdecaux.com/vls/v3/contracts?apiKey=0d238e8d9993c554ac2e5a7ce158e357f8457dbe");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(responseBody);
            List<string> namesContract = new List<string>();

            foreach (JsonElement item in document.RootElement.EnumerateArray())
            {
                namesContract.Add(item.GetProperty("name").GetString());
            }

            for (int i = 0; i < namesContract.Count; i++)
            {
                Console.WriteLine((i + 1).ToString() + ": " + namesContract[i]);
            }

            Console.WriteLine("\nVeuillez choisir un contract : ");

            int indexContract = 0;
            if (!int.TryParse(Console.ReadLine(), out indexContract))
            {
                Environment.Exit(1);
            }

            if (indexContract <= 0 || indexContract > namesContract.Count)
            {
                Environment.Exit(1);
            }

            string contractChoisi = namesContract[indexContract - 1];
            await StationChoose(contractChoisi);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message:{0}", e.Message);
        }
    }

    static async Task StationChoose(string contractChoisi)
    {
        string contrat = contractChoisi;
        try
        {
            string httpRequest = "https://api.jcdecaux.com/vls/v3/stations?contract=" + contrat + "&apiKey=0d238e8d9993c554ac2e5a7ce158e357f8457dbe";
            Console.WriteLine("Contrat choisie : " + contrat);
            HttpResponseMessage response = await client.GetAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(responseBody);
            List<string> namesStations = new List<string>();

            foreach (JsonElement item in document.RootElement.EnumerateArray())
            {
                namesStations.Add(item.GetProperty("name").GetString());
            }

            for (int i = 0; i < namesStations.Count; i++)
            {
                Console.WriteLine((i + 1).ToString() + ": " + namesStations[i]);
            }

            Console.WriteLine("\nVeuillez choisir une Station : ");

            int indexContract = 0;
            if (!int.TryParse(Console.ReadLine(), out indexContract))
            {
                Environment.Exit(1);
            }

            if (indexContract <= 0 || indexContract > namesStations.Count)
            {
                Environment.Exit(1);
            }

            string StationChoisi = namesStations[indexContract - 1];
            Console.WriteLine(StationChoisi);
            await LaPlusProche(StationChoisi, contrat);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message:{0}", e.Message);
        }
    }

    static async Task LaPlusProche(string stationChoisie, string contrat)
    {
        StationInfo stationChoosen = null;
        StationInfo stationLaPLusProche = null;


        try
        {
            string httpRequest = "https://api.jcdecaux.com/vls/v3/stations?contract=" + contrat + "&apiKey=0d238e8d9993c554ac2e5a7ce158e357f8457dbe";
            Console.WriteLine("Contrat choisi : " + contrat);
            HttpResponseMessage response = await client.GetAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(responseBody);
            List<StationInfo> stations = new List<StationInfo>();

            foreach (JsonElement item in document.RootElement.EnumerateArray())
            {
                string name = item.GetProperty("name").GetString();
                double latitude = item.GetProperty("position").GetProperty("latitude").GetDouble();
                double longitude = item.GetProperty("position").GetProperty("longitude").GetDouble();
                StationInfo station = new StationInfo(name, latitude, longitude);
                if (name == stationChoisie)
                {
                    stationChoosen = station;
                }
                else
                {
                    stations.Add(station);
                }
            }

            foreach (StationInfo station in stations)
            {
                if (stationLaPLusProche == null) { stationLaPLusProche = station; }
                else
                {
                    if (Distance(stationChoosen, station) < Distance(stationChoosen, stationLaPLusProche))
                    {
                        stationLaPLusProche = station;
                    }
                }
            }
            Console.WriteLine(stationChoosen.Name + " " + stationChoosen.Latitude + " " + stationChoosen.Longitude);
            Console.WriteLine(stationLaPLusProche.Name + " " + stationLaPLusProche.Latitude + " " + stationLaPLusProche.Longitude);
            Console.ReadLine();

        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message : {0}", e.Message);
        }
    }

    class StationInfo
    {
        public string Name { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public StationInfo(string name, double latitude, double longitude)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    static double Distance(StationInfo station1, StationInfo station2)
    {
        const double EarthRadius = 6371; // Rayon de la Terre en kilomètres

        double lat1 = station1.Latitude;
        double lon1 = station1.Longitude;
        double lat2 = station2.Latitude;
        double lon2 = station2.Longitude;

        // Convertir les coordonnées de degrés en radians
        double dLat = Math.PI * (lat2 - lat1) / 180.0;
        double dLon = Math.PI * (lon2 - lon1) / 180.0;

        // Formule haversine
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(Math.PI * lat1 / 180.0) * Math.Cos(Math.PI * lat2 / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = EarthRadius * c;
        return distance;
    }
}
