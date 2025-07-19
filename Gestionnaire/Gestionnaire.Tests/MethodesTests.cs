using Xunit;
using Gestionnaire;
using System;
using System.IO;

namespace Gestionnaire.Gestionnaire.Tests
{
    public class MethodesTests
    {
        [Fact]
        public void PrintConsole_UnitTest()
        {
            string source = "TestSource";
            string text = "Message de test en développement";

            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);

            try
            {
                var exception = Record.Exception(() =>
                    Methodes.PrintConsole(source, text, exitMessage: false)
                );

                Assert.Null(exception);

                string output = sw.ToString();

                Assert.Contains($"Gestionnaire::{source}", output);
                Assert.Contains(text, output);

                string timestamp = Methodes.PrintDateTime();
                if (!string.IsNullOrEmpty(timestamp))
                {
                    Assert.Contains(timestamp, output);
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void ReadUserInput_UnitTest()
        {
            Program.TestProgression = 139;
            string inputString = "Ma saisie utilisateur";

            using var input = new StringReader(inputString + "\n");
            Console.SetIn(input);

            using var output = new StringWriter();
            Console.SetOut(output);

            string result = Methodes.ReadUserInput("Entrez votre texte", ispassword: false);
            Assert.Equal(inputString, result);

            string consoleOutput = output.ToString();
            Assert.Contains("[Gestionnaire", consoleOutput);
            Assert.Contains("Entrez votre texte", consoleOutput);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(999)]
        public void PrintDateTime_ReturnsExpectedFormat(int formatDateTime)
        {
            string result = Methodes.PrintDateTime(formatDateTime);

            switch (formatDateTime == -1 ? Config.consoleDateTime : formatDateTime)
            {
                case 1:
                    Assert.Matches(@"\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}", result);
                    break;
                case 2:
                    Assert.Matches(@"\d{2}:\d{2}:\d{2}", result);
                    break;
                case 3:
                    Assert.Matches(@"\d{4}/\d{2}/\d{2}", result);
                    break;
                default:
                    Assert.Equal(string.Empty, result);
                    break;
            }
        }

        [Theory]
        [InlineData("123", true)]
        [InlineData("0", false)]
        [InlineData("-5", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        public void IsNumeric_UnitTest(string input, bool expected)
        {
            bool result = Methodes.IsNumeric(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user.name+tag+sorting@example.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("", false)]
        [InlineData("user@.com", false)]
        [InlineData("user@site", false)]
        public void IsEmail_UnitTest(string email, bool expected)
        {
            bool result = Methodes.IsEmail(email);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void UserLogin_ValidCredentials_SucceedsAfterFailures()
        {
            int callCount = 0;

            bool mockCheckCredential()
            {
                callCount++;
                return callCount == 3;
            }

            var output = new StringWriter();
            Console.SetOut(output);

            Methodes.UserLogin(mockCheckCredential);

            string consoleOutput = output.ToString();

            Assert.Equal(3, callCount);
            Assert.Contains("Tentative 1/3 échouée", consoleOutput);
            Assert.Contains("Tentative 2/3 échouée", consoleOutput);
            Assert.Contains("Connexion réussie", consoleOutput);
            Assert.Contains("Bienvenue au Gestionnaire du personnel v1.0", consoleOutput);
        }

        [Fact]
        public void UserLogin_FailsMaxAttempts_ShowsFailureMessage()
        {
            int callCount = 0;

            bool mockCheckCredential()
            {
                callCount++;
                return false;
            }

            var output = new StringWriter();
            Console.SetOut(output);

            Methodes.UserLogin(mockCheckCredential);

            string consoleOutput = output.ToString();

            Assert.Equal(3, callCount);
            Assert.Contains("Tentative 1/3 échouée", consoleOutput);
            Assert.Contains("Tentative 2/3 échouée", consoleOutput);
            Assert.Contains("Nombre maximum de tentatives atteint", consoleOutput);
        }

        [Theory]
        [InlineData(0, "admin")]
        [InlineData(1, "admin")]
        [InlineData(7, "Marie Dupont")]
        [InlineData(8, "02/01/2025")]
        [InlineData(132, "926538147")]
        [InlineData(999, "")]
        public void TestsAgent_RunTest_ReturnsExpectedResult(int testId, string expected)
        {
            var agent = new TestsAgent();

            string result = agent.RunTest(testId);

            Assert.Equal(expected, result);
        }
    }
}
