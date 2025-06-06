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
                    try
                    {
                        _ = Console.ReadKey();
                    }
                    catch (Exception)
                    {
                        Environment.Exit(0);
                    }
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
            string outputString = "";
            switch (Config.consoleDateTime)
            {
                case 1:
                    outputString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    break;
                case 2:
                    outputString = DateTime.Now.ToString("HH:mm:ss");
                    break;
                case 3:
                    outputString = DateTime.Now.ToString("yyyy/MM/dd");
                    break;
                default:
                    outputString = string.Empty;
                    break;
            }
            return outputString;
        }
        public static void UserLogin()
        {
            bool isCredentialsValid = false;
            int attemptCount = 0;

            while (!isCredentialsValid && attemptCount < Config.maxLoginAttempts) 
            {
                isCredentialsValid = CheckCredential();
                attemptCount++;

                if (!isCredentialsValid && attemptCount < Config.maxLoginAttempts)
                {
                    PrintConsole(Config.sourceProgram, $"Tentative {attemptCount}/{Config.maxLoginAttempts} échouée. Veuillez réessayer.\n");
                }
            }

            if (!isCredentialsValid)
            {
                PrintConsole(Config.sourceProgram, "Nombre maximum de tentatives atteint. Fermeture de l'application.");
                return;
            }
            PrintConsole(Config.sourceProgram, "Connexion réussie, Veuillez Patientez...");
            Thread.Sleep(1700);
        }
        private static bool CheckCredential()
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
                    PrintConsole(Config.sourceMethodes, "Identifiants introuvable.");
                    return false;
                }

                string storedHash = result[0]["password_hash"];
                string salt = result[0]["salt"];

                bool isAuthenticated = VerifyPassword(password, storedHash, salt);

                /*

                // @WARNING: DO NOT use these lines below in production run.

                string text = "test";
                byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
                salt = Convert.ToBase64String(saltBytes);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                password: text,
                salt: saltBytes,
                iterations: 100000,
                hashAlgorithm: HashAlgorithmName.SHA256);

                byte[] hashBytes = pbkdf2.GetBytes(32);
                string hash = Convert.ToBase64String(hashBytes);

                Console.WriteLine($"Hash: {hash}");
                Console.WriteLine($"Salt: {salt}");

                */

                if (!Config.productionRun) PrintConsole(Config.sourceMethodes, $"Match: {isAuthenticated}");
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
        private static void Log(string logMessage)
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
        private static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            try
            {
                byte[] saltBytes = Convert.FromBase64String(storedSalt);
                byte[] hashBytes = Convert.FromBase64String(storedHash);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password: password,
                    salt: saltBytes,
                    iterations: 100000,
                    hashAlgorithm: HashAlgorithmName.SHA256);

                byte[] computedHash = pbkdf2.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(computedHash, hashBytes);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
