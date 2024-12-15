using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata;

namespace LibAJCBanque
{
    public class CreatePdf
    {
        // Enumération des types d'opération
        public enum TypeOperation : byte
        {
            R = 1,  // Retrait
            V = 2,  // Virement
            C = 3   // Crédit
        }

        public static void GeneratePdf(string outputFilePath, int id, DateTime datedebut, DateTime datefin)
        {
            // Définir la chaîne de connexion pour SQL Server
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=dbAJCBanque;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

            try
            {
                // Créer et ouvrir la connexion SQL
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    // Préparer la commande SQL
                    string query = @"
            SELECT 
                Cli.Identifiant AS IdClient,
                Cli.Nom,
                Cli.Sexe,
                C.NumeroCompte,
                CA.NumeroCarte,
                O.DateO AS DateOperation,
                O.TypeOperation,
                O.Montant
            FROM Operation O
            LEFT JOIN Compte C 
                ON O.CompteNumeroCompte = C.NumeroCompte
            LEFT JOIN Carte CA 
                ON O.CarteNumeroCarte = CA.NumeroCarte
            LEFT JOIN Client Cli 
                ON Cli.Identifiant = C.ClientIdentifiant
            WHERE Cli.Identifiant = @IdClient
              AND O.DateO BETWEEN @DateDebut AND @DateFin
            ORDER BY 
                C.NumeroCompte, 
                CA.NumeroCarte, 
                O.DateO;";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Ajouter les paramètres pour éviter les injections SQL
                        cmd.Parameters.AddWithValue("@IdClient", id);
                        cmd.Parameters.AddWithValue("@DateDebut", datedebut);
                        cmd.Parameters.AddWithValue("@DateFin", datefin);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Début du PDF
                            QuestPDF.Settings.License = LicenseType.Community;

                            // Générer le nom du fichier avec la date et l'heure
                            string dateSuffix = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            string finalOutputPath = $"{outputFilePath}\\Operation{id}_{dateSuffix}.pdf";

                            // Générer le document PDF
                            QuestPDF.Fluent.Document.Create(container =>
                            {
                                container.Page(page =>
                                {
                                    // Définir la taille et les marges de la page
                                    page.Size(PageSizes.A4);
                                    page.Margin(3, Unit.Centimetre);
                                    page.Background(Colors.White);

                                    // Définir le style par défaut
                                    page.DefaultTextStyle(x => x.FontSize(16).FontColor(Colors.Blue.Medium));

                                    // Ajouter un en-tête
                                    page.Header()
                                        .AlignCenter()
                                        .Text("Rapport de Transaction Bancaire\n")
                                        .FontSize(30)
                                        .FontColor("#1E3A8A")  // Couleur bleu foncé (hexadécimal)
                                        .Bold();



                                    // Ajouter du contenu principal
                                    page.Content()
                                        .Column(column =>
                                        {
                                            // Parcours des résultats
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    // Lire les données retournées par la requête
                                                    int IdCliCour = reader.IsDBNull(reader.GetOrdinal("IdClient")) ? 0 : reader.GetInt32(reader.GetOrdinal("IdClient"));
                                                    string NomCour = reader.IsDBNull(reader.GetOrdinal("Nom")) ? null : reader.GetString(reader.GetOrdinal("Nom"));

                                                    // Récupérer et convertir le sexe
                                                    byte SexeByteCour = reader.IsDBNull(reader.GetOrdinal("Sexe")) ? (byte)0 : reader.GetByte(reader.GetOrdinal("Sexe"));
                                                    string DesignationSexeCour = SexeByteCour == 1 ? "Monsieur" : SexeByteCour == 2 ? "Madame" : "Monsieur/dame";

                                                    string NumeroCompteCour = reader.IsDBNull(reader.GetOrdinal("NumeroCompte")) ? null : reader.GetString(reader.GetOrdinal("NumeroCompte"));
                                                    string NumeroCarteCour = reader.IsDBNull(reader.GetOrdinal("NumeroCarte")) ? null : reader.GetString(reader.GetOrdinal("NumeroCarte"));
                                                    DateTime DateOperationCour = reader.IsDBNull(reader.GetOrdinal("DateOperation")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("DateOperation"));
                                                    byte TypeOperationByteCour = reader.IsDBNull(reader.GetOrdinal("TypeOperation")) ? (byte)0 : reader.GetByte(reader.GetOrdinal("TypeOperation"));
                                                    decimal MontantCour = reader.IsDBNull(reader.GetOrdinal("Montant")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Montant"));

                                                    // Convertir le type d'opération en Enum
                                                    TypeOperation TypeOperationEnumCour = (TypeOperation)TypeOperationByteCour;
                                                    string typeOperationStrCour = TypeOperationEnumCour.ToString();

                                                    // Ajouter les données au PDF
                                                    column.Item().Text($"Client : {IdCliCour}, {DesignationSexeCour}, {NomCour}, " +
                                                                        $"Compte : {NumeroCompteCour}, Carte : {NumeroCarteCour}, " +
                                                                        $"Date : {DateOperationCour:dd/MM/yyyy}, " +
                                                                        $"Type Opération : {typeOperationStrCour}, Montant : {MontantCour:C}")
                                                        .FontSize(14);
                                                }
                                            }
                                            else
                                            {
                                                column.Item()
                                                    .Text("Aucune opération trouvée pour les critères spécifiés.")
                                                    .FontSize(16)
                                                    .FontColor("#FF0000"); // Rouge (hexadécimal)
                                            }
                                        });

                                    // Ajouter un pied de page
                                    page.Footer()
                                        .AlignCenter()
                                        .Text(text =>
                                        {
                                            text.CurrentPageNumber()
                                                .FontColor("#333333"); // Gris foncé (hexadécimal)
                                            text.Span(" / ");
                                            text.TotalPages()
                                                .FontColor("#333333"); // Gris foncé (hexadécimal)
                                        });
                                });
                            }).GeneratePdf(finalOutputPath);

                            Console.WriteLine($"PDF créé avec succès : {finalOutputPath}");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Erreur SQL : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }
    }
}
