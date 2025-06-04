using Microsoft.VisualBasic;

namespace Gestionnaire
{
    class Program
    {
        public static MySQLController Controller { get; private set; } = null!;

        static void Main(string[] args)
        {
            Methodes.PrintConsole(Config.sourceProgram, "Démarrage de l'application, veuillez patienter...");

            Controller = new MySQLController();

            Methodes.PrintConsole(Config.sourceProgram, "Connexion à votre compte personnel..");
            Methodes.UserLogin();

            ApplicationController Program = new();
        }
    }
}