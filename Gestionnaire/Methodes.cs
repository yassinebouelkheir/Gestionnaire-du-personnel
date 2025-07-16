using System.Security.Cryptography;
using System.IO;
using System.Data;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Reflection;

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

            if (Config.productionRun || Program.TestProgression > 133)
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

        /// Gère la connexion utilisateur avec nombre limité de tentatives.
        /// Affiche les messages d'état et attend brièvement en cas de succès.
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
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// Stocke un PDF justificatif dans la base de données.
        /// @param fileName - nom logique du fichier
        /// @param fileData - données PDF en base64
        public static void UploadJustificative(string fileName, string fileData)
        {
            /*
                Stores a justificative PDF in the database.
                @param fileName - logical file name
                @param fileData - PDF data as base64 string
            */
            string query = "INSERT IGNORE INTO pdf_files (fileName, fileData) VALUES (@filename, @filedata)";
            var parameters = new Dictionary<string, object>
            {
                { "@filename", fileName },
                { "@filedata", fileData }
            };
            Program.Controller.InsertData(query, parameters);
        }

        /// Récupère un PDF justificatif depuis la base et ouvre le dossier.
        /// @param fileName - nom du PDF à télécharger
        public static void DownloadJustificative(string fileName)
        {
            /*
                Retrieves a justificative PDF from the database and opens its folder.
                @param fileName - name of the PDF to download
            */
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = Path.Combine(desktop, fileName);

            var query = "SELECT fileData FROM PDF_files WHERE fileName = @filename";
            var parameters = new Dictionary<string, object> { { "@filename", fileName } };
            List<QueryResultRow> rows = Program.Controller.ReadData(query, parameters);

            if (rows.Count == 0)
            {
                Methodes.PrintConsole(Config.sourceMethodes, "le justificative est introuvable mais il est encodé dans la table 'Absence'.", true);
                return;
            }
            byte[] fileBytes = Convert.FromBase64String(rows[0]["fileData"]);
            File.WriteAllBytes(path, fileBytes);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "explorer" :
                        OperatingSystem.IsMacOS() ? "open" : "xdg-open",
                Arguments = $"\"{desktop}\"",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        /// Génère les fiches de paie mensuelles au format PDF (optionnellement pour un contractant),
        /// les compresse en ZIP, sauvegarde l'archive sur le bureau, et ouvre le dossier.
        /// @param contractorId - ID du contractant spécifique ou -1 pour tous
        public static void GenerateAndZipPayslips(int contractorId = -1)
        {
            /*
                Generates monthly payslip PDFs (optionally for one contractor),
                zips them, saves the archive to the desktop, and opens the folder.
                @param contractorId - specific contractor ID or -1 for all
            */
            try
            {
                var payments = GetPaymentsForCurrentMonth(contractorId);

                string tempFolder = Path.Combine(Path.GetTempPath(), "Fiche_de_paie_" + Guid.NewGuid());
                Directory.CreateDirectory(tempFolder);

                foreach (var payment in payments)
                {
                    GeneratePdfForPayment(payment, tempFolder);
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (!Directory.Exists(desktopPath))
                    desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                string zipFilePath = Path.Combine(desktopPath, $"Fiches_de_paie_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);

                ZipFile.CreateFromDirectory(tempFolder, zipFilePath);
                Directory.Delete(tempFolder, true);

                OpenFolderAndSelectFile(zipFilePath);
            }
            catch (Exception ex)
            {
                PrintConsole(Config.sourceMethodes, ex.ToString(), true);
            }
        }

        /// <summary>
        /// Vérifie la validité des identifiants saisis par l'utilisateur.
        /// </summary>
        /// <returns>True si les identifiants sont corrects, sinon false</returns>
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
        /// Récupère les paiements du mois courant, optionnellement pour un contractant.
        /// </summary>
        /// <param name="contractorId">ID du contractant (optionnel, -1 pour tous).</param>
        /// <returns>Liste des paiements correspondant aux critères.</returns>
        private static List<QueryResultRow> GetPaymentsForCurrentMonth(int contractorId = -1)
        {
            string query = @"
                SELECT p.*, c.fullname
                FROM Payments p
                JOIN Contracts c ON p.contractorId = c.contractorId
                WHERE YEAR(p.payment_date) = @year AND MONTH(p.payment_date) = @month";

            var parameters = new Dictionary<string, object>
            {
                ["@year"] = DateTime.Now.Year,
                ["@month"] = DateTime.Now.Month
            };

            if (contractorId != -1)
            {
                query += " AND p.contractorId = @contractorId";
                parameters["@contractorId"] = contractorId;
            }

            var payments = Program.Controller.ReadData(query, parameters);
            return payments;
        }

        /// <summary>
        /// Génère un fichier PDF pour une fiche de paie donnée.
        /// </summary>
        /// <param name="payment">Les données de paiement sous forme de QueryResultRow.</param>
        /// <param name="folderPath">Le dossier de destination pour le fichier PDF.</param>
        private static void GeneratePdfForPayment(QueryResultRow payment, string folderPath)
        {
            using var document = new PdfDocument();
            document.Info.Title = "Fiche de paie";

            var page = document.AddPage();
            using var gfx = XGraphics.FromPdfPage(page);

            GlobalFontSettings.FontResolver = new MinimalFontResolver();

            var titleFont = new XFont("LiberationSans", 20, XFontStyleEx.Bold);
            var contentFont = new XFont("LiberationSans", 12, XFontStyleEx.Regular);

            gfx.DrawString("Fiche de paie", titleFont, XBrushes.Black,
                new XRect(0, 20, page.Width.Point, 40), XStringFormats.TopCenter);

            DateTime paymentDate = DateTime.Parse(payment.Columns["payment_date"]);
            DateTime periodStart = DateTime.Parse(payment.Columns["period_start"]);
            DateTime periodEnd = DateTime.Parse(payment.Columns["period_end"]);

            var lines = new List<string>
            {
                $"Nom complet : {payment.Columns["fullname"]}",
                $"ID de paiement : {payment.Columns["id"]}",
                $"ID du contractant : {payment.Columns["contractorId"]}",
                $"Date de paiement : {paymentDate:d}",
                $"Montant : € {payment.Columns["amount"]} net",
                $"Début de la période : {periodStart:d}",
                $"Fin de la période : {periodEnd:d}",
                $"Type de travail : {payment.Columns["job_type"]}",
                $"Jours d'absence payés : {payment.Columns["paid_absence_days"]}",
                $"Jours d'absence non payés : {payment.Columns["unpaid_absence_days"]}"
            };

            double x = 40;
            double y = 80;
            double lineHeight = contentFont.GetHeight();

            foreach (var line in lines)
            {
                gfx.DrawString(line, contentFont, XBrushes.Black, new XPoint(x, y), XStringFormats.TopLeft);
                y += lineHeight + 2; // Add some spacing between lines
            }

            string monthName = paymentDate.ToString("MMMM", new CultureInfo("fr-FR"));
            string fileName = $"fiche_de_paie_{monthName}_{paymentDate:yyyy}_{SafeFileName(payment.Columns["contractorId"])}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);

            document.Save(fullPath);
        }

        /// <summary>
        /// Rend un nom de fichier valide en remplaçant les caractères interdits.
        /// </summary>
        /// <param name="name">Nom de fichier potentiellement invalide.</param>
        /// <returns>Nom de fichier sécurisé.</returns>
        private static string SafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        /// <summary>
        /// Ouvre le dossier contenant un fichier et le sélectionne dans l'explorateur de fichiers.
        /// </summary>
        /// <param name="filePath">Chemin du fichier à sélectionner.</param>
        private static void OpenFolderAndSelectFile(string filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath) ?? "";

            if (OperatingSystem.IsWindows())
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            else if (OperatingSystem.IsMacOS())
                System.Diagnostics.Process.Start("open", folderPath);
            else if (OperatingSystem.IsLinux())
                System.Diagnostics.Process.Start("xdg-open", folderPath);
            else
                PrintConsole(Config.sourceMethodes, "Erreur, Système d'explotation incompatible.", true);
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
    /// <summary>
    /// Fournisseur de police personnalisé pour PdfSharp.
    /// </summary>
    class MinimalFontResolver : IFontResolver
    {
        private readonly byte[] fontData;

        /// <summary>
        /// Charge les données de police intégrées depuis les ressources.
        /// </summary>
        public MinimalFontResolver()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream? fontStream = assembly.GetManifestResourceStream("Gestionnaire.fonts.LiberationSans-Regular.ttf");
            if (fontStream == null)
                Methodes.PrintConsole(Config.sourceMethodes, "Erreur, la police Gestionnaire.fonts.LiberationSans-Regular.ttf introuvable.", true);

            using MemoryStream ms = new MemoryStream();
            fontStream?.CopyTo(ms);
            fontData = ms.ToArray();
        }
        /// <summary>
        /// Récupère les données binaires de la police.
        /// </summary>
        /// <param name="faceName">Nom de la police.</param>
        /// <returns>Données binaires de la police.</returns>
        public byte[] GetFont(string faceName) => fontData;
        
        /// <summary>
        /// Résout le type de police à utiliser pour un style donné.
        /// </summary>
        /// <param name="familyName">Nom de la famille de police.</param>
        /// <param name="isBold">Indique si la police est en gras.</param>
        /// <param name="isItalic">Indique si la police est en italique.</param>
        /// <returns>Informations sur la police résolue.</returns>
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            => new("LiberationSans");
    }
}

