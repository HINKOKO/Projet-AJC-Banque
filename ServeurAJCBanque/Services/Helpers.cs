using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeurAJCBanque.Services
{
    public class Helpers
    {
        public static bool LuhnCheck(string number)
        {
            int sum = 0;
            bool isSecond = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(number[i].ToString());
                if (isSecond)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }
                sum += digit;
                isSecond = !isSecond;
            }

            return (10 - (sum % 10)) % 10 == 0;
        }
    }
}

