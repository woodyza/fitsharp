// Copyright � 2009 Syterra Software Inc. Includes work by Object Mentor, Inc., � 2002 Cunningham & Cunningham, Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System;
using fit.Runner;
using fit.Service;
using fitSharp.Fit.Model;
using fitSharp.IO;
using fitSharp.Machine.Application;

namespace fitnesse.fitserver
{
	public class FitServer: Runnable
	{
		private FitSocket clientSocket;
		private bool verbose;
		private string host;
		private int port;
		private string socketToken;
	    private Configuration configuration;
	    private ProgressReporter reporter;
	    private readonly TestCounts totalCounts = new TestCounts();

		private const int ASSEMBLYLIST = 0;
		private const int HOST = 1;
		private const int PORT = 2;
		private const int SOCKET_TOKEN = 3;
		private const int DONE = 4;

	    public int Run(string[] commandLineArguments, Configuration configuration, ProgressReporter reporter) {
	        this.configuration = configuration;
	        Run(commandLineArguments);
	        return totalCounts.FailCount;
	    }

	    public void Run(string[] CommandLineArguments)
		{
			ParseCommandLineArguments(CommandLineArguments);

	        reporter = MakeReporter();

			clientSocket = new FitSocket(new SocketModelImpl(host, port), reporter);
			EstablishConnection();

		    var server = new SocketServer(clientSocket, new Service(configuration), reporter, true);
			server.ProcessTestDocuments(WriteResults);

		    clientSocket.Close();
		    Exit();
		}

		private void ParseCommandLineArguments(string[] args)
		{
			int argumentPosition = 0;

			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("-"))
				{
					if ("-v".Equals(args[i]))
						verbose = true;
                    else
						PrintUsageAndExit();
				}
				else
				{
					switch (argumentPosition)
					{
						case ASSEMBLYLIST:
					        new PathParser(args[i]).AddAssemblies(configuration);
					        break;
						case HOST:
							host = args[i];
							break;
						case PORT:
							port = Int32.Parse(args[i]);
							break;
						case SOCKET_TOKEN:
							socketToken = args[i];
							break;
					}
					argumentPosition++;
				}
			}
			if (argumentPosition != DONE)
				PrintUsageAndExit();
		}

		private static void PrintUsageAndExit()
		{
			Console.Error.WriteLine("Usage: FitServer [-v] [-c config] <assemblies> <host> <port> <socket-token>");
			Console.Error.WriteLine("\t-v\tverbose: print log messages to stdout");
			Console.Error.WriteLine("\tassemblies:\t';' separated list of assembly filenames");
			Environment.Exit(1);
		}


	    private ProgressReporter MakeReporter() {
            if (verbose) return new ConsoleReporter();
            return new NullReporter();
        }

	    private void Exit()
		{
	        reporter.WriteLine("exiting...");
	        reporter.WriteLine("End results: " + totalCounts.Description);
	    }

		private void EstablishConnection()
		{
		    reporter.WriteLine("Host:Port:\t" + host + ":" + port);
		    clientSocket.EstablishConnection(Protocol.FormatRequest(socketToken));
		}

	    private void WriteResults(string tables, TestCounts counts)
	    {
	        reporter.WriteLine("\tTransmitting tables of length " + tables.Length);
	        clientSocket.SendDocument(tables);

	        reporter.WriteLine("\tTest Document finished");
	        clientSocket.SendCounts(counts);

	        totalCounts.TallyCounts(counts);
	        reporter.WriteLine("\tresults: " + counts.Description);
	    }
	}
}
