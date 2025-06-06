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
                Methodes.PrintConsole(Config.sourceApplicationController, "6. Paramètres global du société");
                if (ErrorMessage != "") Methodes.PrintConsole(Config.sourceApplicationController, "\n" + ErrorMessage); ErrorMessage = "";
                Methodes.PrintConsole(Config.sourceApplicationController, "Votre choix (1-6): ");
                string serviceText = Console.ReadLine() ?? string.Empty;
                bool prasedInput = int.TryParse(serviceText, out int serviceNumber);

                if (prasedInput) ErrorMessage = RunService(serviceNumber);
                else ErrorMessage = "Erreur, Votre choix doit être entre 1 et 6, Veuillez réssayer s'il vous plaît...";
            }
        }
        private static string RunService(int service)
        {
            Console.WriteLine("\n");
            switch (service)
            {
                case 1:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

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
                                printPrompt: Methodes.PrintConsole(Config.sourceApplicationController, "Enter le total des dernières absence que vous voulez voir (1-30):");
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
                        ShowContinuePrompt();
                        break;
                    }
                case 2:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        PaidLeave paidLeave = new(contractorId, unixdate);
                        if (paidLeave.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucun congé payés enregistré" + ((unixdate > 0) ? (" ce jour là.") : (".")) + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: Methodes.PrintConsole(Config.sourceApplicationController, "Entrer le total des congés payés que vous voulez voir (1-30):");
                                string response = Console.ReadLine() ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int paidLeaveCount);
                                if (paidLeaveCount > 30 || paidLeaveCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {paidLeaveCount} congés payés pour {fullname}:");

                                int maxToShow = Math.Min(paidLeaveCount, Math.Min(30, paidLeave.ListPaidLeave.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = paidLeave.ListPaidLeave;
                                    string reason = (!string.IsNullOrWhiteSpace(row[i]["reason"]) ? row[i]["reason"] : "Non spécifiée");
                                    string sformattedDate = (!string.IsNullOrWhiteSpace(row[i]["startDate"]) ? row[i]["startDate"] : "-1");
                                    string eformattedDate = (!string.IsNullOrWhiteSpace(row[i]["endDate"]) ? row[i]["endDate"] : "-1");

                                    bool sparsedDate = long.TryParse(sformattedDate, out long sunixTimestamp);
                                    bool eparsedDate = long.TryParse(eformattedDate, out long eunixTimestamp);
                                    if (sparsedDate && eparsedDate)
                                    {
                                        DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(sunixTimestamp).DateTime.Date;
                                        DateTime edate = DateTimeOffset.FromUnixTimeSeconds(eunixTimestamp).DateTime.Date;
                                        sformattedDate = sdate.ToString("dd/MM/yyyy");
                                        eformattedDate = edate.ToString("dd/MM/yyyy");
                                    }
                                    else
                                    {
                                        eformattedDate = "Date malformé";
                                        sformattedDate = "Date malformé";
                                    }

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date de début: {sformattedDate} Date de fin: {eformattedDate} Raison : {reason}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " bénéfice d'un congé payé le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de début: " + ((!string.IsNullOrWhiteSpace(paidLeave.StartDate) ? paidLeave.StartDate : "Non spécifiée")) + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de fin: " + ((!string.IsNullOrWhiteSpace(paidLeave.EndDate) ? paidLeave.EndDate : "Non spécifiée")) + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Raison: " + ((!string.IsNullOrWhiteSpace(paidLeave.Reason) ? paidLeave.Reason : "Non spécifiée")) + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 3:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        Training training = new(contractorId, unixdate);
                        if (training.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune formation enregistré" + ((unixdate > 0) ? (" ce jour là.") : (".")) + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: Methodes.PrintConsole(Config.sourceApplicationController, "Enter le total des dernières formations que vous voulez voir (1-30):");
                                string response = Console.ReadLine() ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int trainingCount);
                                if (trainingCount > 30 || trainingCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {trainingCount} formations pour {fullname}:");

                                int maxToShow = Math.Min(trainingCount, Math.Min(30, training.ListTraining.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = training.ListTraining;
                                    string type = (!string.IsNullOrWhiteSpace(row[i]["type"]) ? row[i]["type"] : "Non spécifiée");
                                    string formateur = (!string.IsNullOrWhiteSpace(row[i]["formateur"]) ? row[i]["formateur"] : "Non spécifiée");
                                    string formattedDate = (!string.IsNullOrWhiteSpace(row[i]["date"]) ? row[i]["date"] : "-1");

                                    bool parsedDate = long.TryParse(formattedDate, out long unixTimestamp);
                                    if (parsedDate)
                                    {
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Type: {type} Formateur: {formateur}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " bénéfice d'une formation le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Type: " + ((!string.IsNullOrWhiteSpace(training.Type) ? training.Type : "Non spécifiée")));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Formateur: " + ((!string.IsNullOrWhiteSpace(training.Trainer) ? training.Trainer : "Non spécifiée")));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Adresse: " + training.Address + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 4:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        Mission mission = new(contractorId, unixdate);
                        if (mission.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune mission enregistré" + ((unixdate > 0) ? (" ce jour là.") : (".")) + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: Methodes.PrintConsole(Config.sourceApplicationController, "Enter le total des dernières missions que vous voulez voir (1-30):");
                                string response = Console.ReadLine() ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int missionCount);
                                if (missionCount > 30 || missionCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {missionCount} missions pour {fullname}:");

                                int maxToShow = Math.Min(missionCount, Math.Min(30, mission.ListMission.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = mission.ListMission;
                                    string type = (!string.IsNullOrWhiteSpace(row[i]["type"]) ? row[i]["type"] : "Non spécifiée");
                                    string address = (!string.IsNullOrWhiteSpace(row[i]["address"]) ? row[i]["address"] : "Non spécifiée");
                                    string description = (!string.IsNullOrWhiteSpace(row[i]["description"]) ? row[i]["description"] : "Non spécifiée");
                                    string formattedDate = (!string.IsNullOrWhiteSpace(row[i]["date"]) ? row[i]["date"] : "-1");

                                    bool parsedDate = long.TryParse(formattedDate, out long unixTimestamp);
                                    if (parsedDate)
                                    {
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Type: {type} Adresse: {address}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " était en mission le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Type: " + ((!string.IsNullOrWhiteSpace(mission.Type) ? mission.Type : "Non spécifiée")));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Adresse: " + ((!string.IsNullOrWhiteSpace(mission.Address) ? mission.Address : "Non spécifiée")));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Description: " + mission.Description + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 5:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        WorkTravel workTravel = new(contractorId, unixdate);
                        if (workTravel.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucun déplacement enregistré" + ((unixdate > 0) ? (" ce jour là.") : (".")) + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                            retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                            printPrompt: Methodes.PrintConsole(Config.sourceApplicationController, "Entrer le total des déplacements que vous voulez voir (1-30):");
                                string response = Console.ReadLine() ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int workTravelCount);
                                if (workTravelCount > 30 || workTravelCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {workTravelCount} déplacements pour {fullname}:");

                                int maxToShow = Math.Min(workTravelCount, Math.Min(30, workTravel.ListWorkTravel.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = workTravel.ListWorkTravel;
                                    string address = (!string.IsNullOrWhiteSpace(row[i]["address"]) ? row[i]["address"] : "Non spécifiée");
                                    string sformattedDate = (!string.IsNullOrWhiteSpace(row[i]["startDate"]) ? row[i]["startDate"] : "-1");
                                    string eformattedDate = (!string.IsNullOrWhiteSpace(row[i]["endDate"]) ? row[i]["endDate"] : "-1");

                                    bool sparsedDate = long.TryParse(sformattedDate, out long sunixTimestamp);
                                    bool eparsedDate = long.TryParse(eformattedDate, out long eunixTimestamp);
                                    if (sparsedDate && eparsedDate)
                                    {
                                        DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(sunixTimestamp).DateTime.Date;
                                        DateTime edate = DateTimeOffset.FromUnixTimeSeconds(eunixTimestamp).DateTime.Date;
                                        sformattedDate = sdate.ToString("dd/MM/yyyy");
                                        eformattedDate = edate.ToString("dd/MM/yyyy");
                                    }
                                    else
                                    {
                                        eformattedDate = "Date malformé";
                                        sformattedDate = "Date malformé";
                                    }

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date de début: {sformattedDate} Date de fin: {eformattedDate} Address : {address}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " était en déplacement le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de début: " + ((!string.IsNullOrWhiteSpace(workTravel.StartDate) ? workTravel.StartDate : "Non spécifiée")) + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de fin: " + ((!string.IsNullOrWhiteSpace(workTravel.EndDate) ? workTravel.EndDate : "Non spécifiée")) + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Adresse: " + ((!string.IsNullOrWhiteSpace(workTravel.Address) ? workTravel.Address : "Non spécifiée")) + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Description: " + ((!string.IsNullOrWhiteSpace(workTravel.Description) ? workTravel.Description : "Non spécifiée")) + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 6:
                    {
                        int codePINAttempts = 0;
                        retryCodePIN:Methodes.PrintConsole(Config.sourceApplicationController, "Enter le code PIN requis pour accéder à cette option: ");
                        string adminPanelPIN = Console.ReadLine() ?? string.Empty;
                        codePINAttempts += 1;
                        _ = int.TryParse(adminPanelPIN, out int ParsedPIN);
                        if (Config.adminSettingsPIN == ParsedPIN) {
                            AdministrationPanel();
                            break;
                        }
                        if (codePINAttempts > 2) {
                            ShowContinuePrompt();
                            break;
                        }
                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, le code pin est incorrect, Veuillez réssayer s'il vous plaît...");
                        goto retryCodePIN;
                        break;
                    }
                default:
                    {
                        return "Error, Votre choix doit être entre 1 et 6, Veuillez réssayer.";
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
        private static void AdministrationPanel()
        {
            string ErrorMessage = "";
            while (true)
            {
                //Console.Clear();
                Methodes.PrintConsole(Config.sourceApplicationController, "Bienvenue  à l'administration génerale!");
                Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:");
                Methodes.PrintConsole(Config.sourceApplicationController, "1. Gérer un contrat");
                Methodes.PrintConsole(Config.sourceApplicationController, "2. Gérer les fonctions et leur barèmes");
                Methodes.PrintConsole(Config.sourceApplicationController, "3. Ajouter/Supprimer des utilisateurs");
                Methodes.PrintConsole(Config.sourceApplicationController, "4. Revenir à la menu principal\n");
                if (ErrorMessage != "") Methodes.PrintConsole(Config.sourceApplicationController, ErrorMessage); ErrorMessage = "";
                Methodes.PrintConsole(Config.sourceApplicationController, "Votre choix (1-4): ");
                string serviceText = Console.ReadLine() ?? string.Empty;
                bool prasedInput = int.TryParse(serviceText, out int serviceNumber);

                if (prasedInput) {
                    if (serviceNumber == 4)
                        break;
                    ErrorMessage = RunAdminService(serviceNumber);
                    }
                else ErrorMessage = "Erreur, Votre choix doit être entre 1 et 4, Veuillez réssayer s'il vous plaît...";
            }
        }
        private static string RunAdminService(int service)
        {
            Console.WriteLine("\n");
            switch (service)
            {
                case 1:
                    {
                        break;
                    }
                case 2:
                    {
                        break;
                    }
                case 3:
                    {
                        break;
                    }
                default:
                    {
                        return "Erreur, Votre choix doit être entre 1 et 4, Veuillez réssayer s'il vous plaît...";
                    }
            }
            return "";
        }

        private static void ShowContinuePrompt()
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