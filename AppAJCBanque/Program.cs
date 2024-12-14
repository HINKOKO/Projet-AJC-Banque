
using System.Diagnostics;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using ApiAJCBanque;
using ApiAJCBanque.Datas;
using ApiAJCBanque.Dto;
using LibAJCBanque;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppAJCBanque
{

    internal class Program
    {
    
        public static void Main(string[] args)
        {
            bool continueProgramme = true;
            Console.WriteLine("--- Bienvenue sur l'application AJCBanque Services Employe ---");
            Console.WriteLine("");
            Menu(continueProgramme);
        }

        private static void Menu(bool continueProgramme)
        {

            while (!continueProgramme);
            {
                ConsoleKeyInfo touche = new ConsoleKeyInfo();
                AfficheMenu();

                touche = Console.ReadKey();

                switch (touche.Key)
                {
                    case ConsoleKey.NumPad1:
                        {
                            ImportJSON(continueProgramme);
                            break;
                        }
                    case ConsoleKey.NumPad2:
                        {
                            ExportPDF(continueProgramme);
                            break;
                        }
                    case ConsoleKey.Q:
                        {
                            Quitter(continueProgramme);
                            break;
                        }
                    default:
                        {
                            MauvaiseTouche(continueProgramme);
                            break;
                        }
                }

            }


        }

        private static void Quitter(bool continueProgramme)
        {
            Console.Clear();
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    | [Q]  Quitter             ---|");
            Console.WriteLine("    | Fin du programme         ---|");
            Console.WriteLine("    -------------------------------");
            continueProgramme = false;
        }

        private static void MauvaiseTouche(bool continueProgramme)
        {
            Console.Clear();
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    Mauvaise touche        ");
            Console.WriteLine("");
            Menu(continueProgramme);
        }

        private static void ExportPDF(bool continueProgramme)
        {
            Console.Clear();
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    | [2]  export Fichier PDF  ---|");
            Console.WriteLine("    -------------------------------");
            TraitementExportPDF();
            Menu(continueProgramme);
        }

        private static void TraitementExportPDF()
        {
            throw new NotImplementedException();
        }

        private static async void ImportJSON(bool continueProgramme)
        {
            Console.Clear();
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    | [1]  import Fichier JSON ---|");
            Console.WriteLine("    -------------------------------");

            TraitementImportJSON().Wait();
            Menu(continueProgramme);
        }

        private static async Task TraitementImportJSON()
        {
            //nbTransac nbTransacTraitee
            int nbTransac = 0;
            int nbTransacTraitee = 0;

            // Recuperation du fichier // Chemin vers votre fichier JSON
            string rep = @"C:\code\Courscsharpajc\ConsoleApp\sln_AJCBanque\ServeurAJCBanque\datas\JSON\";
            string filePath = rep + "transactions.json";

            string apiUrl = "http://localhost:5207/api/Operations";

            string json = File.ReadAllText(filePath);
            var operations = JsonSerializer.Deserialize<List<OperationInputDto>>(json);
            if (operations == null || operations.Count == 0)
            {
                Console.WriteLine("Aucune opération à traiter.");
                return;
            }
            // Decoupage et boucle
            using var httpClient = new HttpClient();
            foreach (var operation in operations)
            {
                nbTransac++;
                // Créer l'objet JSON pour l'API
                var operationPayload = new
                {
                    numeroCarte = operation.NumeroCarte,
                    montant = operation.Montant,
                    typeOp = operation.TypeOp,
                    dateOperation = operation.DateOperation,
                    devise = operation.Devise,
                    isValid = operation.IsValid,
                    rate = operation.Rate == null ? 1 : operation.Rate
                };
                //Console.WriteLine(operationPayload);
                string payloadJson = JsonSerializer.Serialize(operationPayload);
                payloadJson = payloadJson.Replace("\"Rate\":null", "\"Rate\":1");

                Console.WriteLine("Enregistrement : traité :"+payloadJson);

                var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

                try
                {
                    // Appel POST à l'API
                    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Opération avec NumeroCarte {operation.NumeroCarte} envoyée avec succès.");
                        nbTransacTraitee++;
                        //httpClient.CancelPendingRequests();
                        MajSolde(operation.NumeroCarte,operation.Montant,operation.TypeOp).Wait(); ;
                    }
                    else
                    {
                        Console.WriteLine($"Erreur pour l'opération {operation.NumeroCarte}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception pour l'opération {operation.NumeroCarte}: {ex.Message}");
                }
            }
            //Console.Clear();
            Console.WriteLine($"Traitement Fin [Nb transaction(s)]:{nbTransac} / [Nb transaction(s) traitée(s)]:{nbTransacTraitee}");
            nbTransac = 0;
            nbTransacTraitee = 0;

        }

 

        private static async Task MajSolde(string numeroCarte, decimal montant, TypeOperation typeOperation)
        {
            switch (typeOperation)
            {
                case TypeOperation.R:   {montant = -montant;break;}
                case TypeOperation.C:   { montant = -montant; break; }
                case TypeOperation.V:   {break; }
                default: { break; }
            }
                

            string baseUrl = "http://localhost:5207/api"; // Remplacez par l'URL de votre API
            string url = "";

            using var httpClient2 = new HttpClient();

            url = $"{baseUrl}/Cartes";
            HttpResponseMessage response = await httpClient2.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            // Vérifier si la réponse est vide
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                Console.WriteLine("La réponse est vide !");
            }
            var cartes = JsonSerializer.Deserialize<IEnumerable<Carte>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Permet d'ignorer la casse des noms des propriétés
            });
            
            var numeroCompte = cartes
                .Where(carte => carte.NumeroCarte == numeroCarte) // Filtrer par NumeroCarte
                .Select(carte => carte.Compte?.NumeroCompte)      // Sélectionner le NumeroCompte associé
                .FirstOrDefault();
            url = $"{baseUrl}/Comptes/UpdateSolde/{numeroCompte}";

            // Sérialisation du nouveau solde
            string json = JsonSerializer.Serialize(montant);
            HttpContent content = new  StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                response = await httpClient2.PutAsync(url,content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Solde mis à jour avec succès pour le compte {numeroCompte}.");
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erreur ({response.StatusCode}): {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'appel à l'API : {ex.Message}");
            }
        }

        private static void AfficheMenu()
        {
        
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    |---        Menu           ---|");
            Console.WriteLine("    | [1]  import Fichier JSON ---|");
            Console.WriteLine("    | [2]  export Fichier PDF  ---|");
            Console.WriteLine("    | [Q]  Quitter             ---|");
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    Tapez votre choix [1,2]");
        }
    }

}