using Mysqlx;

namespace Gestionnaire
{
    class ApplicationController
    {
        private string ErrorMessage = "";
        public ApplicationController()
        {
            while (true)
            {
                Console.Clear();
                Methodes.PrintConsole(Config.sourceApplicationController, "Bienvenue!");
                Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:");
                Methodes.PrintConsole(Config.sourceApplicationController, "1. La présence d'un membre");
                Methodes.PrintConsole(Config.sourceApplicationController, "2. L'absence d'un membre");
                Methodes.PrintConsole(Config.sourceApplicationController, "3. Les membres en congé");
                Methodes.PrintConsole(Config.sourceApplicationController, "4. Les membres formation");
                Methodes.PrintConsole(Config.sourceApplicationController, "5. Les membres en mission");
                Methodes.PrintConsole(Config.sourceApplicationController, "6. Les membres en déplacements");
                Methodes.PrintConsole(Config.sourceApplicationController, "7. Paramètres global du société\n");
                if (ErrorMessage != "") Methodes.PrintConsole(Config.sourceApplicationController, "\n" + ErrorMessage); ErrorMessage = "";
                Methodes.PrintConsole(Config.sourceApplicationController, "Votre choix (1-7): ");
                string ServiceText = Console.ReadLine() ?? string.Empty;
                _ = int.TryParse(ServiceText, out int ServiceNumber);
                ErrorMessage = RunService(ServiceNumber);
            }
        }
        private static string RunService(int number)
        {
            Console.WriteLine("\n");
            switch (number)
            {
                case 1:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer";

                        Absence absence = new Absence(contractorId);
                        if (absence.isNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune absence enregistré.\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune absence enregistré.\n");
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " a été absent le " + date.ToString("dd/MM/yyyy") + " motif: " + absence.Reason + ".\n");
                                if (!string.IsNullOrWhiteSpace(absence.JustificativeDocument))
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Est-ce que vous voulez télécharger le justificative fournis par le membre? (OUI/NON):");
                                    string response = Console.ReadLine() ?? string.Empty;
                                    if (!string.IsNullOrWhiteSpace(response) && response.Equals("oui", StringComparison.OrdinalIgnoreCase))
                                    {
                                        
                                    }
                                }
                            }
                        }
                        showContinuePrompt();
                        break;
                    }
                case 2:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer";
                        showContinuePrompt();
                        break;
                    }
                case 3:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer";
                        showContinuePrompt();
                        break;
                    }
                case 4:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer";
                        showContinuePrompt();
                        break;
                    }
                case 5:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer";
                        showContinuePrompt();
                        break;
                    }
                case 6:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer";
                        showContinuePrompt();
                        break;
                    }
                case 7:
                    {
                        showContinuePrompt();
                        break;
                    }
                default:
                    {
                        return "Error, Votre choix doit être entre 1 et 7, Veuillez réssayer.";
                    }
            }
            return "";
        }
        private static void GetData(out string contractorName, out int contractorId, out long unixdate)
        {
            Methodes.PrintConsole(Config.sourceApplicationController, "Enter le nom et prénom du membre ciblé(e): ");
            string fullname = Console.ReadLine() ?? string.Empty;
            Methodes.PrintConsole(Config.sourceApplicationController, "Enter une date précise (facultative) (dd/mm/aaaa): ");
            string date = Console.ReadLine() ?? string.Empty;
            Methodes.PrintConsole(Config.sourceApplicationController, "Votre demande est en cours de traitement, veuillez patientez..");
            Contracts contractor = new Contracts(fullname);

            if (contractor.isNull)
            {
                contractorId = -1;
                contractorName = "";
            }
            else
            {
                contractorId = contractor.ContractorId;
                contractorName = contractor.Fullname;
            }

            if (DateTime.TryParse(date, out DateTime dateTime))
            {
                DateTimeOffset dto = new(dateTime.ToUniversalTime());
                unixdate = dto.ToUnixTimeSeconds();
            }
            else
            {
                unixdate = -1;
            }
        }
        private static void showContinuePrompt()
        {
            Methodes.PrintConsole(Config.sourceApplicationController, "Appuyez sur n'importe quelle touche pour revenir au menu principal...");
            try
            {
                _ = Console.ReadKey();
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
        }
    }
}