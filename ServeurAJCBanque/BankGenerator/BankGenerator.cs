using System.Configuration;
using System.Text;
using ServeurAJCBanque.Models;
using Transaction = ServeurAJCBanque.Models.Transaction;

namespace ServeurAJCBanque.MockBank
{
    public class BankGenerator
    {
        private string cardPath = ConfigurationManager.AppSettings["CardFilePath"];
        public static string[] devises = { "EUR", "USD", "GBP", "RON", "RUB", "TWD"};
        public static Random random = new Random();
        private static List<string> cards;

        public BankGenerator()
        {
            LoadCardNumbers(cardPath);
        }
        // Charger les numéros de carte depuis un fichier texte
        public void LoadCardNumbers(string filePath)
        {
            cards = new List<string>();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Le fichier cartes.txt n'existe pas.");
                return;
            }

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    cards.Add(line.Trim());
                }

            }
        }

        // Méthode pour obtenir une carte au hasard
        private static string GetRandomCard()
        {
            return cards[random.Next(cards.Count)];
        }

        // Methode pour generer des transactions en mock-random
        public static List<Transaction> GenererTransactions(int count)
        {
            
            var transactions = new List<Transaction>();
            for (int i = 0; i < count; i++)
            {
                string selectedCard = GetRandomCard();
                transactions.Add(GenererTransaction(selectedCard));
            }
            return transactions;
        }


        // GenereMontant - generer un montant aléatoire [10 - 1000]
        private static decimal GenererMontant(decimal min = 10, decimal max = 1000)
        {
            return Math.Round((decimal)random.NextDouble() * (max - min) + min, 2);
        }

        private static Transaction GenererTransaction(string cardNumber)
        {
            var transaction = new Transaction
            {
                NumeroCarte = cardNumber,
                Montant = GenererMontant(),
                TypeOp = GenererTypeOperation(),
                DateOperation = GenererDateOperation(),
                Devise = GenererDevise()
            };
            return transaction;
        }

        // Generer un type operation aleatoire
        private static TypeOperation GenererTypeOperation()
        {
            int operationIndex = random.Next(Enum.GetValues<TypeOperation>().Length);
            return (TypeOperation)operationIndex;
        }

        private static DateTime GenererDateOperation()
        {
            int daysBack = random.Next(365); // Generate transactions within the last year
            return DateTime.Now.Subtract(TimeSpan.FromDays(daysBack));
        }

        public static bool LuhnAlgorithm(string number)
        {
            int sum = 0;
            bool doubleDigit = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(number[i].ToString());
                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }
                sum += digit;
                doubleDigit = !doubleDigit;
            }

            return (sum % 10 == 0);
        }

        private static string GenererDevise()
        {
            return devises[random.Next(devises.Length)];
        }
    }
}


