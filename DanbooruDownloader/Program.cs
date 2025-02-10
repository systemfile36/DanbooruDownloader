using DanbooruDownloader.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;

namespace DanbooruDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineApplication application = new CommandLineApplication(true)
            {
                FullName = "Danbooru Downloader",
            };

            application.HelpOption("-?|-h|--help");

            CommandOption versionOption = application.VersionOption("-v|--version", PlatformServices.Default.Application.ApplicationVersion);

            application.Command("dump", command =>
            {
                command.Description = "Download entire images on the server of specified source.";
                command.HelpOption("-h|--help");

                var outputPathArgument = command.Argument("path", "Output path.", false);
                var startIdOption = command.Option("-s|--start-id <id>", "Starting Id. Default is 1.", CommandOptionType.SingleValue);
                var endIdOption = command.Option("-e|--end-id <id>", "Ending Id. Default is 0 (unlimited).).", CommandOptionType.SingleValue);

                //use Paging Option
                var usePageOption = command.Option("-p|--use-paging", "Use Paging value. -s and -e option will be ignored", CommandOptionType.NoValue);

                //paging options
                var startPageOption = command.Option("-sp|--start-page <page>", "Starting Page. Default is 1.", CommandOptionType.SingleValue);
                var endPageOption = command.Option("-ep|--end-page <page>", "Ending Page. Default is 0 (unlimited).", CommandOptionType.SingleValue);
                
                //Set custom query string. see https://danbooru.donmai.us/wiki_pages/help%3Acheatsheet
                var queryOption = command.Option("--query", "Set tag query string (e.g., 'score:>=100', 'blonde_hair', etc.). Default is null. --use-paging option should be enabled", CommandOptionType.SingleValue);

                //Set extensions (fileType)
                var extOption = command.Option("--ext", "Set extensions of file to download. extensions should be comma-separated list. (e.g., 'png,jpg,gif'). Default is 'png,jpg'", CommandOptionType.SingleValue);

                var ignoreHashCheckOption = command.Option("-i|--ignore-hash-check", "Ignore hash check.", CommandOptionType.NoValue);
                var includeDeletedOption = command.Option("-d|--deleted", "Include deleted posts.", CommandOptionType.NoValue);
                var usernameOption = command.Option("--username", "Username of Danbooru account.", CommandOptionType.SingleValue);
                var apikeyOption = command.Option("--api-key", "API key of Danbooru account.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    string path = outputPathArgument.Value;

                    long startId = 1;
                    long endId = 0;

                    long startPage = 1;
                    long endPage = 0;

                    bool usePage = usePageOption.HasValue();
                    bool ignoreHashCheck = ignoreHashCheckOption.HasValue();
                    bool includeDeleted = includeDeletedOption.HasValue();

                    //Default is null
                    string query = null;

                    //Default is 'png,jpg'
                    List<string> exts = new List<string> { "png", "jpg" };

                    if (startIdOption.HasValue() && !long.TryParse(startIdOption.Value(), out startId))
                    {
                        Console.WriteLine("Invalid start id.");
                        return -2;
                    }

                    if (endIdOption.HasValue() && !long.TryParse(endIdOption.Value(), out endId))
                    {
                        Console.WriteLine("Invalid end id.");
                        return -2;
                    }

                    //If paging options are set without setting --use-page option, program will be exited.
                    if(!usePage && (startPageOption.HasValue() || endPageOption.HasValue()
                    || queryOption.HasValue()))
                    {
                        Console.WriteLine("Set --use-page if you want use paging options (-sp | -ep | --query)");
                        return -2;
                    }

                    if (startPageOption.HasValue() && !long.TryParse(startPageOption.Value(), out startPage))
                    {
                        Console.WriteLine("Invalid start page.");
                        return -2;
                    }

                    if(endPageOption.HasValue() && !long.TryParse(endPageOption.Value(),out endPage))
                    {
                        Console.WriteLine("Invalid end page.");
                        return -2;
                    }

                    if (!usernameOption.HasValue() || !apikeyOption.HasValue())
                    {
                        Console.WriteLine("You must specify username and api key.");
                        return -2;
                    }

                    if(queryOption.HasValue())
                    {
                        query = queryOption.Value();
                    }

                    if(extOption.HasValue())
                    {
                        exts = new List<string>(extOption.Value().Split(","));
                    }

                    var username = usernameOption.Value();
                    var apikey = apikeyOption.Value();

                    DumpCommand.Run(path, startId, endId, startPage, endPage, query, exts, ignoreHashCheck, includeDeleted, username, apikey).Wait();

                    return 0;
                });
            });

            application.OnExecute(() =>
            {
                application.ShowHint();

                return 0;
            });

            try
            {
                int exitCode = application.Execute(args);

                if (exitCode == -2)
                {
                    application.ShowHint();
                }

                Environment.ExitCode = exitCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.ExitCode = -1;
            }
        }
    }
}
