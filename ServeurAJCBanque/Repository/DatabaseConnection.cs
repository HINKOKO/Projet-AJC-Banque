using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ServeurAJCBanque.DbConnection
{
    /// <summary>
    /// DatabaseConnection - classe pour centraliser la gestion de 
    /// connection avec la base de données
    /// </summary>
    public class DatabaseConnection
    {
        private static readonly object _lock = new object();
        private static DatabaseConnection _instance;
        private readonly Dictionary<string, SqlConnection> _connections;


        // Constructeur privé pour empêcher l'instanciation directe
        private DatabaseConnection()
        {
            _connections = new Dictionary<string, SqlConnection>
            {
                {
                    "dbTransactions",
                    new SqlConnection(ConfigurationManager.ConnectionStrings["dbTransactions"].ConnectionString)
                },
                {
                    "dbAuth",
                    new SqlConnection(ConfigurationManager.ConnectionStrings["dbAuth"].ConnectionString)
                }
            };
        }

        // Mode Singleton - propriété pour obtenir l'instance unique 
        public static DatabaseConnection Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseConnection();
                    }
                    return _instance;
                }
            }
        }

        // Methode - Obtenir la connexion
        public SqlConnection GetConnection(string dbIdentifier)
        {
            // rejetez les bases inconnues
            if (!_connections.ContainsKey(dbIdentifier))
            {
                throw new ArgumentException($"La base de données '{dbIdentifier}' n'est pas configurée.");
            }

            var connection = _connections[dbIdentifier];

            if (connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur connexion a la base de données {dbIdentifier} Contactez Admin Steeve");
                    Console.WriteLine($"Details" + ex.Message + " "  + ex.StackTrace);
                    throw;
                }
            }
            return connection;
        }

        // Methode - Fermer la connexion
        public void CloseAllConnection()
        {
            foreach (var conn in _connections.Values)
            if (conn.State == ConnectionState.Open)
            {
                try
                {
                    conn.Close();
                    Console.WriteLine("Connexion Base de données fermée avec succès");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur Fermeture connexion. Contactez Admin Steeve." + ex.Message);
                }
            }
        }
    }
}
