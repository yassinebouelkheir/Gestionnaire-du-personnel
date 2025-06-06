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
                //Console.Clear();
                Methodes.PrintConsole(Config.sourceApplicationController, "Bienvenue!");
                Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:");
                Methodes.PrintConsole(Config.sourceApplicationController, "1. L'absence/présence d'un membre");
                Methodes.PrintConsole(Config.sourceApplicationController, "2. Les membres en congé");
                Methodes.PrintConsole(Config.sourceApplicationController, "3. Les membres formation");
                Methodes.PrintConsole(Config.sourceApplicationController, "4. Les membres en mission");
                Methodes.PrintConsole(Config.sourceApplicationController, "5. Les membres en déplacements");
                Methodes.PrintConsole(Config.sourceApplicationController, "6. Paramètres global du société\n");
                if (ErrorMessage != "") Methodes.PrintConsole(Config.sourceApplicationController, "\n" + ErrorMessage); ErrorMessage = "";
                Methodes.PrintConsole(Config.sourceApplicationController, "Votre choix (1-6): ");
                string serviceText = Console.ReadLine() ?? string.Empty;
                bool prasedInput = int.TryParse(serviceText, out int serviceNumber);

                if (prasedInput) ErrorMessage = RunService(serviceNumber);
                else ErrorMessage = "Erreur, Votre choix doit être entre 1 et 6, Veuillez réssayer s'il vous plaît...";
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
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer (fullname: "+fullname+")";

                        Absence absence = new(contractorId, unixdate);
                        if (absence.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune absence enregistré" + ((unixdate > 0) ? (" ce jour là.") : (".")) + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: Methodes.PrintConsole(Config.sourceApplicationController, "Vous pouvez séléctionner combien d'absence que vous voulez voir? (1-30):");
                                string response = Console.ReadLine() ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int absenceCount);
                                if (absenceCount > 30 || absenceCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {absenceCount} absence pour {fullname}:");

                                int maxToShow = Math.Min(absenceCount, Math.Min(30, absence.ListAbsence.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = absence.ListAbsence;
                                    string reason = (!string.IsNullOrWhiteSpace(row[i]["reason"]) ? row[i]["reason"] : "Non spécifiée");
                                    string doc = (!string.IsNullOrWhiteSpace(row[i]["justificativeDocument"]) ? row[i]["justificativeDocument"] : "Aucun document");
                                    string formattedDate = (!string.IsNullOrWhiteSpace(row[i]["date"]) ? row[i]["date"] : "-1");

                                    bool parsedDate = long.TryParse(formattedDate, out long unixTimestamp);
                                    if (parsedDate)
                                    {
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Raison : {reason}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " a été absent le " + date.ToString("dd/MM/yyyy") + " Raison: " + ((!string.IsNullOrWhiteSpace(absence.Reason) ? absence.Reason : "Non spécifiée")) + ".\n");
                                if (!string.IsNullOrWhiteSpace(absence.JustificativeDocument))
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Est-ce que vous voulez télécharger le justificative fournis par le membre? (OUI/NON):");
                                    string response = Console.ReadLine() ?? string.Empty;
                                    if (!string.IsNullOrWhiteSpace(response) && response.Equals("oui", StringComparison.OrdinalIgnoreCase))
                                    {
                                        System.Diagnostics.Process.Start("https://"+Config.serverAddress+"/staff/"+contractorId+"/justificatives_absences/"+absence.JustificativeDocument);
                                    }
                                }
                            }
                        }
                        showContinuePrompt();
                        break;
                    }
                case 3:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";
                        showContinuePrompt();
                        break;
                    }
                case 4:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";
                        showContinuePrompt();
                        break;
                    }
                case 5:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";
                        showContinuePrompt();
                        break;
                    }
                case 6:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";
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
            contractorName = Console.ReadLine() ?? string.Empty;
            Methodes.PrintConsole(Config.sourceApplicationController, "Enter une date précise (facultative) (dd/mm/aaaa): ");
            string date = Console.ReadLine() ?? string.Empty;

            Methodes.PrintConsole(Config.sourceApplicationController, "Votre demande est en cours de traitement, Veuillez patientezs'il vous plaît...");
            Contracts contractor = new(contractorName);

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
                unixdate = dto.AddDays(1).ToUnixTimeSeconds();
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