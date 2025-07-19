using System.Security.Cryptography;

namespace Gestionnaire
{
    public class Methodes
    {
        public interface IExceptionGenerator
        {
            void GenerateException(string source, string message);
        }
        public class GestionnaireException(string message) : Exception(message) { }
        public class ExceptionGenerator : IExceptionGenerator
        {
            public void GenerateException(string source, string message)
            {
                throw new GestionnaireException(message);
            }
        }

        /// <summary>
        /// Écrit un message formaté dans la console.
        /// En mode production, affiche un message d'erreur générique et quitte si demandé.
        /// </summary>
        /// <param name="source">Fichier d'origine du message</param>
        /// <param name="text">Contenu du message</param>
        /// <param name="exitMessage">Si true, affiche le message puis termine le programme</param>
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
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write("  ");
                Console.ResetColor();
                string logMessage = $"[Gestionnaire::{source} {timestamp}]: {text}";
                Console.WriteLine(logMessage);
            }

            if (exitMessage)
            {
                ExceptionGenerator Generator = new();
                try
                {
                    Generator.GenerateException(Config.sourceMethodes, text);
                }
                catch (GestionnaireException ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write("  ");
                    Console.ResetColor();
                    Console.WriteLine($"[Gestionnaire::{source} {timestamp} Exception capturée]: " + ex.Message);
                }

                string logMessage = $"[Gestionnaire::{source} {timestamp}]: {text}";
                Log(logMessage);
            }
        }
        
        /// Affiche une invite à l'utilisateur et retourne la saisie.
        /// Masque les caractères avec * si ispassword est vrai.
        /// @param text - invite affichée à l'utilisateur
        /// @param ispassword - vrai pour cacher la saisie
        /// @return chaîne saisie par l'utilisateur
        public static string ReadUserInput(string text, bool ispassword = false)
        {
            var input = "";
            string timestamp = string.IsNullOrEmpty(PrintDateTime()) ? "" : PrintDateTime();
            if (Config.productionRun) Console.Write($"\n[Gestionnaire {timestamp}]: {text}");
            else
            {
                Console.Write("\n");
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write("  ");
                Console.ResetColor();
                Console.Write($"[Gestionnaire::Methodes {timestamp}]: {text}");
            }

            if (Config.productionRun || Program.TestProgression > 132)
            {
                if (!ispassword) return Console.ReadLine() ?? "";

                input = "";
                ConsoleKeyInfo key;
                while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                {
                    if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                    {
                        input = input[..^1];
                        Console.Write("\b \b");
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        input += key.KeyChar;
                        Console.Write("*");
                    }
                }
                Console.WriteLine();
            }
            else
            {
                TestsAgent Agent = new();
                input = Agent.RunTest(Program.TestProgression);
                Program.TestProgression += 1;
                Console.Write(input + "\n");
            }
            return input;
        }

        /// Formate la date/heure courante selon Config.consoleDateTime.
        /// @return chaîne formatée ou vide si désactivé
        public static string PrintDateTime(int formatDateTime = -1)
        {
            string outputString = "";
            int optionValue = ((formatDateTime == -1) ? Config.consoleDateTime : formatDateTime);
            outputString = optionValue switch
            {
                1 => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                2 => DateTime.Now.ToString("HH:mm:ss"),
                3 => DateTime.Now.ToString("yyyy/MM/dd"),
                _ => string.Empty,
            };
            return outputString;
        }

        /// <summary>
        /// Gère la connexion utilisateur avec un nombre limité de tentatives.
        /// </summary>
        /// <param name="credentialChecker">
        /// Fonction de validation des identifiants. Utilise <see cref="CheckCredential"/> par défaut.
        /// </param>
        public static void UserLogin(Func<bool>? credentialChecker = null)
        {
            credentialChecker ??= CheckCredential;  // Si aucun delegate, utilise CheckCredential par défaut

            bool isCredentialsValid = false;
            int attemptCount = 0;

            while (!isCredentialsValid && attemptCount < Config.maxLoginAttempts)
            {
                isCredentialsValid = credentialChecker();
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
            PrintConsole(Config.sourceProgram, "Connexion réussie, Veuillez Patientez...\n");
            PrintConsole(Config.sourceApplicationController, "Bienvenue au Gestionnaire du personnel v1.0");
        }


        /// Valide qu'une chaîne représente un entier positif.
        /// @param value - chaîne à tester
        /// @return vrai si numérique et > 0, sinon faux
        public static bool IsNumeric(string value)
        {
            return int.TryParse(value, out int result) && result > 0;
        }

        /// Vérifie si une chaîne est une adresse email valide.
        /// @param email - chaîne email à valider
        /// @return vrai si valide, sinon faux
        public static bool IsEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    return false;

                var domain = email.Split('@').Last();
                return domain.Contains('.');
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Vérifie la validité des identifiants saisis par l'utilisateur en les comparant aux données stockées en base.
        /// </summary>
        /// <returns>
        /// True si les identifiants sont corrects et authentifiés, sinon false.
        /// </returns>
        /// <exception cref="ArgumentNullException">Levée si le nom d'utilisateur ou le mot de passe est null ou vide.</exception>
        /// <exception cref="Exception">Levée en cas d'erreur lors de la lecture des données ou du processus d'authentification.</exception>
        private static bool CheckCredential()
        {
            Methodes UserConsole = new();
            string username = ReadUserInput("Nom d'utilisateur : ") ?? string.Empty;
            string password = ReadUserInput("Mot de passe : ", true) ?? string.Empty;

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

                string query = "SELECT password_hash, salt FROM Users WHERE username = @username LIMIT 1";
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

        /// <summary>
        /// Vérifie un mot de passe en le comparant au hash stocké.
        /// </summary>
        /// <param name="password">Le mot de passe en texte clair à vérifier.</param>
        /// <param name="storedHash">Le hash stocké du mot de passe.</param>
        /// <param name="storedSalt">Le sel utilisé pour le hashage, encodé en base64.</param>
        /// <returns>True si le mot de passe correspond, sinon false.</returns>
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

        /// <summary>
        /// Enregistre un message d’erreur dans un fichier log, et affiche dans la console si en mode développement.
        /// </summary>
        /// <param name="logMessage">Le message à enregistrer.</param>
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
    }
}

