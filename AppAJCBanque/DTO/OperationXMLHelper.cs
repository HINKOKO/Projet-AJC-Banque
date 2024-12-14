using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibAJCBanque;

namespace AppAJCBanque.DTO
{
    
    public static class OperationXMLHelper
    {
        public static List<OperationXML> ConvertirPourXML(List<Operation> operations)
        {
            // Utiliser la puissance de LINQ pour 1- trier par date / 2 - trier par type operation
            return operations.
                OrderByDescending(op => op.DateO) 
                .ThenBy(op => op.TypeOperation)
                .Select(op => new OperationXML
            {
                NumeroCompte = op.Compte.NumeroCompte,
                DateOperation = op.DateO,
                TypeOperation = ConvertTypeOperation(op.TypeOperation),
                Montant = op.Montant,
                NumeroCarteMasque = MasquerNumeroCarte(op.Carte.NumeroCarte)
            }).ToList();
        }

        private static string MasquerNumeroCarte(string numeroCarte)
        {
            if (string.IsNullOrEmpty(numeroCarte)) return "XXXX-XXXX-XXXX-XXXX";

            return numeroCarte.Substring(0, numeroCarte.Length - 4).PadRight(numeroCarte.Length, 'X');
        }

        private static string ConvertTypeOperation(TypeOperation typeOperation)
        {
            // Convertit l'énumération TypeOperation en une chaîne avec la valeur correspondante
            switch (typeOperation)
            {
                case TypeOperation.R:
                    return "Retrait";
                case TypeOperation.C:
                    return "Paiement Carte";
                case TypeOperation.V:
                    return "Versement";
                default:
                    return "Inconnu"; // Au cas où un type d'opération non prévu apparaît
            }
        }
    }
}
