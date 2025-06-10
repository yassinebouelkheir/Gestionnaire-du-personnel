using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Gestionnaire;

public class MethodesTests
{
    [Theory]
    [InlineData("123", true)]
    [InlineData("0", false)]
    [InlineData("-5", false)]
    [InlineData("abc", false)]
    public void IsNumeric_Test(string input, bool expected)
    {
        bool actual = Methodes.IsNumeric(input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void IsEmail_Test(string email, bool expected)
    {
        bool actual = Methodes.IsEmail(email);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PrintDateTime_ReturnsEmpty_WhenConsoleDateTimeIsConstZero()
    {
        // Config.consoleDateTime est const, supposons 0 => ""
        string result = Methodes.PrintDateTime(0);
        Assert.Equal(string.Empty, result);
    }


    [Fact]
    public void PrintConsole_DoesNotThrow()
    {
        // Pas de modif possible sur Config.productionRun const, on teste juste que ça ne plante pas
        Methodes.PrintConsole("TestSource", "Message test");
        Methodes.PrintConsole("TestSource", "Message test avec exit", true);
    }

    [Fact]
    public void ReadUserInput_DoesNotThrow()
    {
        // Impossible de simuler la saisie console sans refactor, on teste que ça ne plante pas
        Thread t1 = new(() =>
        {
            string result = Methodes.ReadUserInput("Entrez texte : ", false);
            Assert.NotNull(result);
        });
        t1.Start();
        t1.Join(100);

        Thread t2 = new Thread(() =>
        {
            string result = Methodes.ReadUserInput("Entrez mot de passe : ", true);
            Assert.NotNull(result);
        });
        t2.Start();
        t2.Join(100);
    }

    [Fact]
    public void UserLogin_DoesNotThrow()
    {
        // Test que UserLogin s’exécute sans exception (CheckCredential est private)
        Thread t = new Thread(() =>
        {
            Methodes.UserLogin();
        });
        t.Start();
        t.Join(100);
    }
}
