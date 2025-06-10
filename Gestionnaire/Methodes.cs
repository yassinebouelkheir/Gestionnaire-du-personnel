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
        public static void PrintConsole(string source, string text, bool exitMessage = false)
        {
            /*
                Writes a formatted message to the console.
                In production mode, shows a generic error and exits.
                @param source - origin file of the message
                @param text - message content
                @param exitMessage - if true, logs and terminates the program
            */
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
        public static string ReadUserInput(string text, bool ispassword = false)
        {
            /*
                Prompts the user and returns the entered text.
                Masks characters with * if ispassword is true.
                @param text - prompt shown to the user
                @param ispassword - true to hide input while typing
                @return the user-entered string
            */
            string timestamp = string.IsNullOrEmpty(PrintDateTime()) ? "" : PrintDateTime();
            if (Config.productionRun) Console.Write($"\n[Gestionnaire {timestamp}]: {text}");
            else Console.Write($"\n[Gestionnaire::Methodes {timestamp}]: {text}");

            if (!ispassword) return Console.ReadLine() ?? "";

            var input = "";
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
            return input;
        }
        public static string PrintDateTime(int formatDateTime = -1)
        {
            /*
                Formats the current date/time based on Config.consoleDateTime.
                @return formatted date/time string or empty if disabled
            */
            string outputString = "";
            int optionValue = ((formatDateTime == -1) ? Config.consoleDateTime : formatDateTime);
            switch (optionValue)
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
            /*
                Handles user login with limited retry attempts.
                Prints status messages and pauses briefly on success.
            */
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
            Thread.Sleep(1700);
        }
        public static bool IsNumeric(string value)
        {
            /*
                Validates that the supplied string is a positive integer.
                @param value - string to test
                @return true if numeric and > 0, otherwise false
            */
            return int.TryParse(value, out int result) && result > 0;
        }
        public static bool IsEmail(string email)
        {
            /*
                Checks whether a string is a well-formed email address.
                @param email - email string to validate
                @return true if valid, otherwise false
            */
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
        private static bool CheckCredential()
        {
            string username = Methodes.ReadUserInput("Nom d'utilisateur : ") ?? string.Empty;
            string password = Methodes.ReadUserInput("Mot de passe : ", true) ?? string.Empty;
            Thread.Sleep(1000);

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

        private static string SafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

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
    }
    class MinimalFontResolver : IFontResolver
    {
        private readonly byte[] fontData;

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

        public byte[] GetFont(string faceName) => fontData;

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            => new("LiberationSans");
    }
}

