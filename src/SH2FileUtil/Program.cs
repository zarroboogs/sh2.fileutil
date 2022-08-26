using System.CommandLine;

namespace SH2FileUtil;

internal class Program
{
    private static void ParseArgs(string[] args)
    {
        var inputArgument = new Argument<string>(name: "input", description: "Input path")
        {
            Arity = ArgumentArity.ExactlyOne,
        };

        var outputArgument = new Argument<string>(name: "output", description: "Output path")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };

        var saveCommand = new Command(name: "save", description: "(De)compress game saves")
        {
            inputArgument,
            outputArgument,
        };

        saveCommand.SetHandler((input, output) =>
        {
            SaveCommand.Process(input, output);
        }, inputArgument, outputArgument);

        var modeOption = new Option<CryptCommand.CryptMode>(aliases: new string[] { "-m", "--mode" },
            getDefaultValue: () => CryptCommand.CryptMode.AssetDec, description: "Crypt mode");

        var keyOption = new Option<string>(aliases: new string[] { "-k", "--key" },
            description: "Crypt key");

        var assetCommand = new Command(name: "crypt", description: "(En/De)crypt game assets")
        {
            modeOption,
            keyOption,
            inputArgument,
            outputArgument,
        };

        assetCommand.SetHandler((input, output, mode, key) =>
        {
            CryptCommand.Process(input, output, mode, key);
        }, inputArgument, outputArgument, modeOption, keyOption);

        var rootCommand = new RootCommand("SH2 File Utility")
        {
            saveCommand,
            assetCommand,
        };

        rootCommand.Invoke(args);
    }

    private static void Main(string[] args)
    {
        ParseArgs(args);
    }
}