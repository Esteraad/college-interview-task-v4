using Demo.models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    class Program
    {
        private static HttpClient httpClient = new HttpClient();

        static public async Task Main() {
            var cts = new CancellationTokenSource();
            var spacexApi = new SpacexApi(httpClient);

            try {
                Dictionary<int, string> rockets = await spacexApi.GetRocketsAsync(cts.Token);

                while (true) {
                    try {
                        Console.Clear();
                        DisplayRequest(rockets);
                        string input = Console.ReadLine();
                        Console.WriteLine();

                        if (!await DoAction(input, rockets, spacexApi, cts)) break;
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error occured, details: { ex.Message }");
                    }
                    Console.WriteLine("Hit enter to continue...");
                    Console.ReadLine();
                }
            }
            catch { Console.WriteLine("Error occured while loading rockets list. Program will exit."); }
        }

        private static async Task<bool> DoAction(string input, Dictionary<int, string> rockets, SpacexApi spacexApi, CancellationTokenSource cts) {
            if (input == "exit") return false;

            if (!isNumber(input) || !rockets.ContainsKey(int.Parse(input)))
                Console.WriteLine("Invalid input. Please provide correct rocket id or type 'exit' to exit." + Environment.NewLine);
            else {
                SpacexRocket spacexRocket = await spacexApi.GetRocketAsync(rockets[int.Parse(input)], cts.Token);
                Console.WriteLine(spacexRocket.ToString());
            }
            return true;
        }

        private static void DisplayRequest(Dictionary<int, string> rockets) {
            Console.WriteLine("Choose rocket id to see it's details or type 'exit' to exit.");
            foreach (KeyValuePair<int, string> rocket in rockets)
                Console.WriteLine($"{rocket.Key} {rocket.Value}");
            Console.Write("Rocket id: ");
        }

        private static bool isNumber(object value) {
            try {
                if (int.Parse((value.ToString() ?? "")).GetType().Equals(typeof(int)))
                    return true;
                else
                    return false;
            }
            catch {
                return false;
            }
        }
    }
}