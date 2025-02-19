using DanbooruDownloader.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
                var queryOption = command.Option("--query", "Set tag query string (e.g., 'score:>=100', 'blonde_hair', etc.). Default is empty string. --use-paging option should be enabled", CommandOptionType.SingleValue);

                //Set extensions (fileType)
                var extOption = command.Option("--ext", "Set extensions of file to download. extensions should be comma-separated list. (e.g., 'png,jpg,gif'). Default is 'png,jpg'", CommandOptionType.SingleValue);

                var ignoreHashCheckOption = command.Option("-i|--ignore-hash-check", "Ignore hash check.", CommandOptionType.NoValue);
                var includeDeletedOption = command.Option("-d|--deleted", "Include deleted posts.", CommandOptionType.NoValue);
                var usernameOption = command.Option("--username", "Username of Danbooru account.", CommandOptionType.SingleValue);
                var apikeyOption = command.Option("--api-key", "API key of Danbooru account.", CommandOptionType.SingleValue);

                //Set command that execute on exit
                var onExitCommandOption = command.Option("-ec|--exit-command", "Command that execute when program exit. Default is none", CommandOptionType.SingleValue);

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

                    //Default is ""
                    string query = "";

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

                    if(onExitCommandOption.HasValue())
                    {
                        
                        string command = onExitCommandOption.Value();

                        //closure
                        //Add EventListener to ProcessExit event
                        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
                        {
                            if (!string.IsNullOrWhiteSpace(command))
                            {
                                Console.WriteLine($"Exit...{command} will execute...");
                                ExecuteCommand(command);
                            }
                        };
                    }


                    var username = usernameOption.Value();
                    var apikey = apikeyOption.Value();

                    DumpCommand.Run(path, startId, endId, startPage, endPage, query, exts, ignoreHashCheck, includeDeleted, username, apikey).Wait();

                    return 0;
                });
            });

            //Add clean command
            application.Command("clean", command =>
            {
                command.Description = "Deletes records and files that match the given SQL condition.";
                command.HelpOption("-h|--help");

                var pathArgument = command.Argument("path", "Path of dataset directory that created by dump command");
                var conditionArgument = command.Argument("condition", "SQL WHERE condition.", false);

                //Option to set file name of metadata database manually
                var metaDatabaseNameOption 
                    = command.Option("--db-name", $"File name of metadata database. File name should include extendsion(.sqlite). Default is {Utilities.PathUtility.META_DB_NAME}", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    string path = pathArgument.Value;
                    string condition = conditionArgument.Value;

                    //Default is META_DB_NAME in PathUtility
                    string metaDatabaseName = Utilities.PathUtility.META_DB_NAME;

                    if (string.IsNullOrWhiteSpace(condition))
                    {
                        Console.WriteLine("Invalid condition.");
                        return -2;
                    }

                    if(string.IsNullOrEmpty(path))
                    {
                        Console.WriteLine("Invalid path.");
                        return -2;
                    }

                    if (metaDatabaseNameOption.HasValue())
                    {
                        metaDatabaseName = metaDatabaseNameOption.Value();
                    }

                    CleanCommand.Run(path, condition, metaDatabaseName);

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

        //Execute shell command
        static void ExecuteCommand(string command)
        {
            string shell, args;

            //Set shell and arguments by OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                shell = "cmd.exe"; 
                args = $"/c {command}";
            } else
            {
                shell = "/bin/bash";
                args = $"-c \"{command}\"";
            }


            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            //Start Process by Process start info
            using(Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine(output);
            }
        }
    }
}
