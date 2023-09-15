using System.Text;
using System;


namespace NeoNovaAPI.Services
{
    public class SeedUserGeneratorServices
    {
        private readonly Random _random;
        private const string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$&?";

        public SeedUserGeneratorServices()
        {
            _random = new Random();
        }

        public string SeedPasswordGenerator(string role)
        {
            int remainingChars = 20 - role.Length - 4; // Subtract 4 to save spots for each type of character

            // Randomly generate one uppercase letter, one lowercase letter, and one special character
            char upperCaseLetter = (char)_random.Next('A', 'Z' + 1);
            char lowerCaseLetter = (char)_random.Next('a', 'z' + 1);
            char specialCharacter = "!@#$%^&*()-=_+[]{}|;:',.<>?".ToCharArray()[_random.Next(0, 28)];

            // Randomly generate a digit between 0 and 9
            var digit = _random.Next(0, 10).ToString();

            var password = new StringBuilder(role); // Start with the role
            password.Append(upperCaseLetter); // Add one uppercase letter
            password.Append(lowerCaseLetter); // Add one lowercase letter
            password.Append(specialCharacter); // Add one special character

            for (int i = 0; i < remainingChars; i++) // Loop to fill the remaining spots with alphanumeric characters
            {
                password.Append(_chars[_random.Next(_chars.Length)]);
            }

            // Append a random digit to the end
            password.Append(digit);

            return password.ToString();
        }

        public string SeedUsernameGenerator(string role)
        {
            return $"{role}Agent{_random.Next(000001, 999999)}";
        }
    }
}
