using Microsoft.VisualBasic;

namespace Gestionnaire
{
    public class Program
    {
        public static MySQLController Controller { get; private set; } = null!;
        private static int _testProgression = 0;

        /// <summary>
        /// Obtient la progression actuelle du test.
        /// </summary>
        /// <remarks>
        /// Cette propriété est en lecture seule à l'extérieur de la classe.
        /// La progression ne peut être modifiée que par des méthodes internes.
        /// </remarks>
        public static int TestProgression
        {
            get => _testProgression;
            set => _testProgression = value;
        }


        /// <summary>
        /// Point d'entrée principal de l'application.
        /// Initialise les composants et gère la connexion utilisateur.
        /// </summary>
        /// <param name="args">Arguments de la ligne de commande.</param>
        static void Main(string[] args)
        {
            Methodes.PrintConsole(Config.sourceProgram, "Démarrage de l'application, veuillez patienter...");

            Controller = new MySQLController();

            Methodes.PrintConsole(Config.sourceProgram, "Connexion à votre compte personnel..");
            Methodes.UserLogin();
            if (!Config.productionRun)
            {
                Methodes.PrintConsole(Config.sourceApplicationController, "⚠️  Mode test automatique actif, saisie utilisateur momentanément désactivée\n");
            }

            ApplicationController Program = new();
        }
    }
}