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
                        GetData(out string fullname, out string date);
                        break;
                    }
                case 2:
                    {
                        GetData(out string fullname, out string date);
                        break;
                    }
                case 3:
                    {
                        GetData(out string fullname, out string date);
                        break;
                    }
                case 4:
                    {
                        GetData(out string fullname, out string date);
                        break;
                    }
                case 5:
                    {
                        GetData(out string fullname, out string date);
                        break;
                    }
                case 6:
                    {
                        GetData(out string fullname, out string date);
                        break;
                    }
                case 7:
                    {
                        break;
                    }
                default:
                    {
                        return "Error, Votre choix doit être entre 1 et 7, Veuillez réssayer.";
                    }
            }
            return "";
        }
        private static void GetData(out string fullname, out string date)
        {
            Methodes.PrintConsole(Config.sourceApplicationController, "Enter le nom et prénom du membre ciblé(e): ");
            fullname = Console.ReadLine() ?? string.Empty;
            Methodes.PrintConsole(Config.sourceApplicationController, "Enter une date précise (facultative) (dd/mm/aaaa): ");
            date = Console.ReadLine() ?? string.Empty;
        }
    }
}