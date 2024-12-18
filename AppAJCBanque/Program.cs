﻿using System.Diagnostics;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using System.Configuration;
using ApiAJCBanque;
using ApiAJCBanque.Datas;
using ApiAJCBanque.Dto;
using AppAJCBanque.DTO;
using LibAJCBanque;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AppAJCBanque.Services;

namespace AppAJCBanque
{

    internal class Program
    {
        //public event EventHandler<FileCheckerEventArgs> FileChecked;

        public static void Main(string[] args)
        {
            bool continueProgramme = true;

            // Instancier un FileWatcherService - espion sur dossier et abonnement a l'evenement
            string spyOnPath = ConfigurationManager.AppSettings["SpyFilePath"];
            var watcher = new FileWatcherService(spyOnPath);
            watcher.TransactionsUpdated += OnTransactionsUpdated;

            /*Console.WriteLine("--- Bienvenue sur l'application AJCBanque Services Employe ---");
            Console.WriteLine("");
            string folderPath = @"C:\code\Courscsharpajc\ConsoleApp\sln_AJCBanque\ServeurAJCBanque\datas\JSON"; // Remplacez ce chemin par le vôtre
            */
            Styles styles = new Styles();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(styles.AjcGreet);
            Console.WriteLine("--- Bienvenue sur l'application AJCBanque Services Employe ---");
            Console.ResetColor();
            Console.WriteLine("");
            Menu(continueProgramme);

            /*FileSystemWatcher watcher = new FileSystemWatcher(folderPath);
            EspionnerDossier(watcher);
            Menu(continueProgramme, watcher);*/
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
                    case ConsoleKey.NumPad3:
                        {
                            ExportXML(continueProgramme);
                            break;
                        }
                    case ConsoleKey.NumPad4:
                        {
                            
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

        /*private static void EspionnerDossier(FileSystemWatcher watcher)
        {
            // Configurer les événements à surveiller (création ou modification de fichiers)
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            // Définir les types de fichiers à surveiller (ici, les fichiers JSON)
            watcher.Filter = "*.json";
            //FileChecker fileChecker = new FileChecker();
            // Définir l'événement qui se déclenche lorsqu'un fichier est ajouté ou modifié
            watcher.Changed += OnFileChanged;

            // Commencer à surveiller
            watcher.EnableRaisingEvents = true;
        }
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(500); // Vous pouvez ajuster le délai si nécessaire
            Console.WriteLine($"Nouveau fichier à traiter : {e.Name} ");
        }
        private static void StopFileCheck(FileSystemWatcher watcher, bool continueProgramme)
        {
            watcher.EnableRaisingEvents = false;
            Menu(continueProgramme,watcher);

        }*/


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
        private static void ExportXML(bool continueProgramme)
        {
            Console.Clear();
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    | [2]  export Fichier XML  ---|");
            Console.WriteLine("    -------------------------------");
            TraitementExportXML();
            Menu(continueProgramme);
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
            var operations = System.Text.Json.JsonSerializer.Deserialize<List<OperationInputDto>>(json);
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
                string payloadJson = System.Text.Json.JsonSerializer.Serialize(operationPayload);
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
            var cartes = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Carte>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Permet d'ignorer la casse des noms des propriétés
            });
            
            var numeroCompte = cartes
                .Where(carte => carte.NumeroCarte == numeroCarte) // Filtrer par NumeroCarte
                .Select(carte => carte.Compte?.NumeroCompte)      // Sélectionner le NumeroCompte associé
                .FirstOrDefault();
            url = $"{baseUrl}/Comptes/UpdateSolde/{numeroCompte}";

            // Sérialisation du nouveau solde
            string json = System.Text.Json.JsonSerializer.Serialize(montant);
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
            Console.WriteLine("    | [3]  export Fichier XML  ---|");
            Console.WriteLine("    | [4]  stop écoute fichier ---|");
            Console.WriteLine("    | [Q]  Quitter             ---|");
            Console.WriteLine("    -------------------------------");
            Console.WriteLine("    Tapez votre choix [1,2,...]");
        }

        private static async void TraitementExportXML()
        {
            try
            {
                var operations = await FetchOperations();

                if (operations == null || !operations.Any())
                {
                    Console.WriteLine("Aucun operation disponibles pour export XML");
                    return;
                }

                // A changer selon vos machines dans App.config !
                string XMLPath = ConfigurationManager.AppSettings["XMLFilePath"];

                // Convertir les operation en DTO pour cacher infos sensibles
                var operationsXML = OperationXMLHelper.ConvertirPourXML(operations);

                GenerateXML(operationsXML, XMLPath);
                Console.WriteLine("Generation du XML - Succes. Consulter dans " + XMLPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur generation XML " + ex.Message);
            }
        }
        private static async Task<List<Operation>> FetchOperations()
        {
            //string apiUrl = "http://localhost:5207/api/Operations";   // MC
            string apiUrl = "http://localhost:5145/api/Operations";
            using var httpClient = new HttpClient();
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erreur lors de la récupération des données : {response.StatusCode}");
                    return null;
                }
                string respBody = await response.Content.ReadAsStringAsync();
                // Deserialiser le JSON
                var operations = JsonSerializer.Deserialize<List<Operation>>(respBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return operations ?? new List<Operation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la recuperation des operations: {ex.Message}");
                return null;
            }
        }

        private static void GenerateXML(List<OperationXML> operations, string XMLPath)
        {
            // ouvrir un file stream et un write stream
            using FileStream fs = new FileStream(XMLPath, FileMode.Create);
            using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);

            XmlSerializer serializer = new XmlSerializer(typeof(List<OperationXML>));
            serializer.Serialize(writer, operations);
            Console.WriteLine("XML généré avec succès.");
        }

        private static void OnTransactionsUpdated(List<Models.Transaction> txs)
        {
            Console.WriteLine($"Mise a jour détectée : {txs.Count} transactions détectées - a traiter");
            Console.WriteLine($"Mise a jour requises pour");
            foreach (var tx in txs)
            {
                Console.WriteLine($"Compte {tx.NumeroCarte} Montant {tx.Montant}");
            }
            Console.WriteLine("traitement...");
            Thread.Sleep(900);
            try
            {
                ImportJSON(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} {ex.Source}");
            }
        }

        private static void TraitementExportPDF()
        {
            Console.WriteLine("Entrez un numéro Client");
            int idClient = Int16.Parse(Console.ReadLine());
            //int idClient = 3;
            DateTime dateDebut = new DateTime(2024, 01, 01);
            DateTime dateFin = new DateTime(2024, 12, 31);

            // Appeler la méthode Connect depuis CreePdf dans LibAJCBanque
            CreatePdf.GeneratePdf(@"C:\code\Courscsharpajc\ConsoleApp\sln_AJCBanque\AppAJCBanque\EXPORTS", idClient, dateDebut, dateFin);

            // Si vous avez besoin de l'autre CreePdf, vous pouvez aussi l'utiliser de cette façon :
            // CreePdfApi.Connect(idClient, dateDebut, dateFin);

            Console.WriteLine("Programme exportPDF terminé.");
        }

    }

}