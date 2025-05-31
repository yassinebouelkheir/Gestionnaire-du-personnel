using System.Security.Cryptography;
using System.IO;

namespace Gestionnaire
{
    class Methodes
    {
        public static void PrintConsole(string source, string text, bool exitMessage = false)
        {
            string timestamp = string.IsNullOrEmpty(PrintDateTime()) ? "" : PrintDateTime();
            if (Config.productionRun)
            {
                if (exitMessage)
                {
                    Console.WriteLine($"[Gestionnaire {timestamp}]: {Config.errorMessage}");
                    Console.WriteLine($"[Gestionnaire {timestamp}]: Appuyez sur n'importe quelle touche pour quitter...");
                    _ = Console.ReadKey();
                    Environment.Exit(0);
                }
                else Console.WriteLine($"[Gestionnaire {timestamp}]: {text}");
            }
            else
            {
                string logMessage = $"[Gestionnaire::{source} {timestamp}]: {text}";
                Console.WriteLine(logMessage);
            }

            if (exitMessage) 
            {
                string logMessage = $"[Gestionnaire::{source} {timestamp}]: {text}";
                Log(logMessage);
            }
        }

        public static string PrintDateTime()
        {
            switch (Config.consoleDateTime)
            {
                case 1:
                    return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                case 2:
                    return DateTime.Now.ToString("HH:mm:ss");
                case 3:
                    return DateTime.Now.ToString("yyyy/MM/dd");
                default:
                    return string.Empty;
            }
        }

        public static bool UserLogin()
        {
            Console.WriteLine("Nom d'utilisateur : ");
            string username = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Mot de passe : ");
            string password = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                PrintConsole(Config.sourceMethodes, "Nom d'utilisateur et mot de passe sont requis.");
                return false;
            }

            try
            {   
                var parameters = new Dictionary<string, object>
                {
                    { "@username", username }
                };

                string query = "SELECT password_hash, salt FROM Utilisateurs WHERE username = @username LIMIT 1";
                var result = Program.Controller.ReadData(query, parameters);

                if (result.Count == 0 || result[0].Columns.Count == 0)
                {
                    PrintConsole(Config.sourceMethodes, "Identifiants incorrects.");
                    return false;
                }

                string storedHash = result[0]["password_hash"];
                string salt = result[0]["salt"];
                
                bool isAuthenticated = PasswordHasher.VerifyPassword(password, storedHash, salt);

                if (!isAuthenticated)
                {
                    PrintConsole(Config.sourceMethodes, "Identifiants incorrects.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                PrintConsole(Config.sourceMethodes, ex.ToString(), true);
                return false;
            }
        }
        public static void Log(string logMessage)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
            File.AppendAllText(logPath, logMessage + "\n\n");

            if (!Config.productionRun)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(logMessage);
                Console.ResetColor();
            }
        }
    }

    class PasswordHasher // Source: DeepSeek
    {
        private const int SaltSize = 32;
        private const int Iterations = 310000;
        private const int HashSize = 32;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

        public static string HashPassword(string password, out string salt)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password: password,
                salt: saltBytes,
                iterations: Iterations,
                hashAlgorithm: Algorithm);

            byte[] hashBytes = pbkdf2.GetBytes(HashSize);
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            try
            {
                byte[] saltBytes = Convert.FromBase64String(storedSalt);
                byte[] hashBytes = Convert.FromBase64String(storedHash);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password: password,
                    salt: saltBytes,
                    iterations: Iterations,
                    hashAlgorithm: Algorithm);

                byte[] computedHash = pbkdf2.GetBytes(HashSize);
                return CryptographicOperations.FixedTimeEquals(computedHash, hashBytes);
            }
            catch
            {
                return false;
            }
        }
    }
}
