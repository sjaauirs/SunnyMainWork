using System.ComponentModel;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    public class CommandLineParserException : Exception
    {
        public CommandLineParserException(string? message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Base Option description holder
    /// </summary>
    public class CommandLineParserOption
    {
        public string OptionName { get; private set; }
        public bool ImplicitBool { get ; private set; }
        public Type OptionType;
        public object? DefaultValue;

        public CommandLineParserOption(string optionName, object? defaultValue, bool implicitBool = false)
        {
            OptionName = optionName;
            ImplicitBool = implicitBool;
            OptionType = typeof(string);
            DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// Allows options with generic argument
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandLineParserOptionTyped<T> : CommandLineParserOption
    {
        public CommandLineParserOptionTyped(string optionName, bool implicitBool = false) :
            base(optionName, null, implicitBool)
        {
            OptionType = typeof(T);
        }

        public CommandLineParserOptionTyped(string optionName, T defaultValue, bool implicitBool = false) :
            base(optionName, defaultValue, implicitBool)
        {
            OptionType = typeof(T);
        }
    }

    /// <summary>
    /// Simple Command Line Parser
    /// </summary>
    public class CommandLineParser
    {
        public IDictionary<string, object?> NamedArguments { get; private set; }
        public IList<string> UnnamedArguments { get; private set; }

        private readonly string[] _args;
        private readonly Dictionary<string, CommandLineParserOption> _mapOptions = new();

        public CommandLineParser(CommandLineParserOption[] options, string[] args)
        {
            NamedArguments = new Dictionary<string, object?>();
            UnnamedArguments = new List<string>();

            _args = args;

            foreach (var option in options)
            {
                if (!option.OptionName.StartsWith("--"))
                {
                    throw new CommandLineParserException($"CommandLineOption must begin with '--' : incorrect option: {option.OptionName}");
                }

                _mapOptions[option.OptionName] = option;
            }

            Parse();
        }

        private void Parse()
        {
            // parse cmdline args
            for (int i = 0; i < _args.Length; i++)
            {
                var arg = _args[i];

                if (arg.StartsWith("--") && _mapOptions.ContainsKey(arg))
                {
                    if (_mapOptions[arg].ImplicitBool)
                    {
                        NamedArguments[arg] = true;
                    }
                    else if (i < _args.Length - 1)
                    {
                        string argVal = _args[i + 1];
                        var typeDesc = TypeDescriptor.GetConverter(_mapOptions[arg].OptionType);
                        var val = typeDesc.ConvertFromInvariantString(argVal);
                        NamedArguments[arg] = val;

                        i++;
                    }
                    else
                    {
                        throw new CommandLineParserException($"Unable to parse explicit value option: {arg}, reached end of arguments");
                    }
                }
                else
                {
                    UnnamedArguments.Add(arg);
                }
            }

            // set values for implicit bool options that are not specified in the cmdline
            foreach (var kv in _mapOptions)
            {
                if (kv.Value.ImplicitBool && !NamedArguments.ContainsKey(kv.Key))
                {
                    NamedArguments[kv.Key] = false;
                }
            }
        }
    }
}
