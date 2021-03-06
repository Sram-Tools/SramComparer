﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

// ReSharper disable PossibleMultipleEnumeration

namespace SRAM.Comparison
{
	/// <summary>Starts a UI-less cmd queue.</summary>
	public class CommandQueue
	{
		private static CommandQueue? _instance;
		public static CommandQueue Instance => _instance ??= new();

		public void Start(IOptions options)
		{
			var commandHandler = ComparisonServices.CommandHandler;
			var consolePrinter = ComparisonServices.ConsolePrinter;

			commandHandler.ThrowIfNull(nameof(commandHandler));
			consolePrinter.ThrowIfNull(nameof(consolePrinter));
			options.BatchCommands.ThrowIfNull(nameof(options.BatchCommands));

			if (options.CurrentFilePath.IsNullOrEmpty())
			{
				consolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
				Console.ReadKey();
				return;
			}

			if (options.UILanguage is not null)
				CultureHelper.TrySetCulture(options.UILanguage, consolePrinter);

			var commands = options.BatchCommands?.Split('-') ?? Enumerable.Empty<string>();
			var queuedCommands = new Queue<string>(commands);

			consolePrinter.PrintLine(@$"{Resources.QueuedCommands}: {queuedCommands.Count} ({string.Join(", ", commands)})");

			while (true)
			{
				try
				{
					string? command = null;
					if (queuedCommands.Count > 0)
						queuedCommands.TryDequeue(out command);

					if (commandHandler.RunCommand(command!, options) == false)
						break;

#if !DEBUG
					if (queuedCommands.Count == 0)
						break;
#endif
				}
				catch (IOException ex)
				{
					consolePrinter.PrintError(ex.Message);
					consolePrinter.PrintSectionHeader();
				}
				catch (Exception ex)
				{
					consolePrinter.PrintError(ex);
					consolePrinter.PrintSectionHeader();
				}
			}
		}
	}
}
