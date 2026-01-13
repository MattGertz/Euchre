using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSEuchre4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Register the Exit event to ensure cleanup happens even if windows don't close normally
            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            // Clean up resources from the main window if it exists
            if (this.MainWindow is EuchreTable mainTable)
            {
                CleanupTableResources(mainTable);
            }

            // Clean up resources from any other EuchreTable windows that might be open
            foreach (Window win in this.Windows)
            {
                if (win is EuchreTable table && win != this.MainWindow)
                {
                    CleanupTableResources(table);
                }
                else if (win is EuchreOptions optionsDialog)
                {
                    try
                    {
                        optionsDialog.DisposeVoice();
                    }
                    catch
                    {
                        // Ignore errors during shutdown cleanup
                    }
                }
            }
        }

        private void CleanupTableResources(EuchreTable table)
        {
            try
            {
                // Dispose all player voices
                if (table.gamePlayers != null)
                {
                    for (int i = 0; i < table.gamePlayers.Length; i++)
                    {
                        table.gamePlayers[i]?.DisposeVoice();
                    }
                }
            }
            catch
            {
                // Ignore errors during shutdown cleanup
            }
        }
    }
}
