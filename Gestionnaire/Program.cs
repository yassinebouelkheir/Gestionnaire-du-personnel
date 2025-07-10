using Microsoft.VisualBasic;

namespace Gestionnaire
{
    class Program
    {
        public static MySQLController Controller { get; private set; } = null!;
        public static int TestProgression = 0;

        static void Main(string[] args)
        {
            Methodes.PrintConsole(Config.sourceProgram, "Démarrage de l'application, veuillez patienter...");

            Controller = new MySQLController();

            Methodes.PrintConsole(Config.sourceProgram, "Connexion à votre compte personnel..");
            Methodes.UserLogin();
            if (!Config.productionRun) Methodes.PrintConsole(Config.sourceApplicationController, "⚠️  Mode test automatique actif, saisie utilisateur momentanément désactivée ⚠️\n");
            ApplicationController Program = new();
        }
    }
}