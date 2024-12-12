using System.Net.NetworkInformation;
using System.Text;
using ServeurAJCBanque.Models;
using Transaction = ServeurAJCBanque.Models.Transaction;

namespace ServeurAJCBanque.MockBank
{
    public class BankGenerator
    {
        public string baseNumber = "497401850223"; // base de carte crédit - sans les 4 derniers
        public string[] devises = { "EUR", "USD", "GBP" };
        public Random random = new Random();

        public List<Transaction> GenererTransactions(int count)
        {
            var transactions = new List<Transaction>();
            for (int i = 0; i < count; i++)
            {
                string fullCardNumber = GenererCarte(baseNumber);
                if (i % 2 == 0 || i % 8 == 0)
                {
                    string corrupted = AlterCardNumber(fullCardNumber);
                    transactions.Add(GenererTransaction(corrupted));
                }
                else
                {
                    transactions.Add(GenererTransaction(fullCardNumber));
                }
            }

            return transactions;
        }

        public static string GenererCarte(string prefix)
        {
            Random random = new Random();

            // Generate a 3-digit suffix
            string randomNums = random.Next(0, 1000).ToString("D3");
            string card15 = prefix + randomNums;

            // Calculate the Luhn checksum digit
            int sum = 0;
            // alternate is true here because start reverse at 15
            bool alternate = true;

            for (int i = card15.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(card15[i].ToString());

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                alternate = !alternate;
            }

            int checkDigit = (10 - (sum % 10)) % 10;

            // Append the checksum digit to the card number
            string fullcardNumber = card15 + checkDigit.ToString();

            return fullcardNumber;
        }

        public string AlterCardNumber(string cardNumber)
        {
            int indexToAlter = random.Next(0, cardNumber.Length - 1);
            char altered = (char)((cardNumber[indexToAlter] - '0' + 1) % 10 + '0');
            StringBuilder alteredCard = new StringBuilder(cardNumber);
            alteredCard[indexToAlter] = altered;
            return alteredCard.ToString();
        }

        // GenereMontant - generer un montant aléatoire [10 - 1000]
        private decimal GenererMontant(decimal min = 10, decimal max = 1000)
        {
            return Math.Round((decimal)random.NextDouble() * (max - min) + min, 2);
        }

        private Transaction GenererTransaction(string fullCardNumber)
        {
            var transaction = new Transaction();
            transaction.NumeroCarte = fullCardNumber;
            transaction.Montant = GenererMontant();
            transaction.TypeOp = GenererTypeOperation();
            transaction.DateOperation = GenererDateOperation();
            transaction.Devise = GenererDevise();
            transaction.IsValid = LuhnAlgorithm(fullCardNumber); // Check card validity
            return transaction;
        }

        // Generer un type operation aleatoire
        private TypeOperation GenererTypeOperation()
        {
            int operationIndex = random.Next(Enum.GetValues<TypeOperation>().Length);
            return (TypeOperation)operationIndex;
        }

        private DateTime GenererDateOperation()
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

        private string GenererDevise()
        {
            return devises[random.Next(devises.Length)];
        }
    }
}
