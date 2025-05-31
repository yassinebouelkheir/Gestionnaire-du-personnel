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

            Methodes.PrintConsole(Config.sourceProgram, "L'application est maintenant prête à être utilisée.");

            bool isCredentialsValid = false;
            int attemptCount = 0;

            while (!isCredentialsValid && attemptCount < Config.maxLoginAttempts)
            {
                isCredentialsValid = Methodes.UserLogin();
                attemptCount++;

                if (!isCredentialsValid && attemptCount < Config.maxLoginAttempts)
                {
                    Methodes.PrintConsole(Config.sourceProgram, $"Tentative {attemptCount}/{Config.maxLoginAttempts} échouée. Veuillez réessayer.\n");
                }
            }

            if (!isCredentialsValid)
            {
                Methodes.PrintConsole(Config.sourceProgram, "Nombre maximum de tentatives atteint. Fermeture de l'application.");
                return;
            }
            Methodes.PrintConsole(Config.sourceProgram, "Connexion réussie, Veuillez Patientez...");
        }
    }
}