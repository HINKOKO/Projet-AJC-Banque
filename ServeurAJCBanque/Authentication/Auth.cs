using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using ServeurAJCBanque.DbConnection;

namespace ServeurAJCBanque.Authentication
{
    public class Auth
    {
        public static bool AuthenticateUser()
        {
            Console.Write("Nom utilisateur : ");
            string username = Console.ReadLine();

            Console.Write("Mot de passe : ");
            string password = ReadPassword();

            // recuperer username et pass hash en DB
            string hashedPassDB = GetHashedPassDB(username);
            if (hashedPassDB == null)
            {
                Console.WriteLine("mauvaise combinaison");
                return false;
            }
            string hashedInput = HashPassword(password);
            return hashedInput == hashedPassDB;
        }

        // Recuperer hash en base de données
        private static string GetHashedPassDB(string logon)
        {
            string hashedPass = null;
            using (SqlConnection conn = DatabaseConnection.Instance.GetConnection("dbAuth"))
            {
                string query = "SELECT hashPassword FROM users WHERE logon = @Logon";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Logon", logon);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            hashedPass = rdr["hashPassword"].ToString();
                        }
                    }
                }
            }
            return hashedPass;
        }

        // Fonction pour lire le mot de passe rentré en mode 'blind typing'
        private static string ReadPassword()
        {
            StringBuilder pass = new StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    // donner possibilité de revenir en arriere si mauvaise saisie
                    pass.Length--;
                    Console.Write("\b \b");

                }
                else if (!char.IsControl(key.KeyChar))
                {
                    pass.Append(key.KeyChar);
                    Console.Write("*"); // cacher la saisie
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return pass.ToString();
        }

        // Fonction pour hacher les mots de passe
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
