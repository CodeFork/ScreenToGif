﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ScreenToGif.FileWriters;
using ScreenToGif.Properties;
using ScreenToGif.Util;
using ScreenToGif.Windows;
using ScreenToGif.Windows.Other;
using ExceptionViewer = ScreenToGif.Windows.Other.ExceptionViewer;

namespace ScreenToGif
{
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region Unhandled Exceptions

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            #endregion

            #region Arguments

            try
            {
                if (e.Args.Length > 0)
                {
                    Argument.Prepare(e.Args);
                }
            }
            catch (Exception ex)
            {
                var errorViewer = new ExceptionViewer(ex);
                errorViewer.ShowDialog();

                LogWriter.Log(ex, "Generic Exception - Arguments");
            }

            #endregion

            #region Upgrade Application Settings

            //See http://stackoverflow.com/questions/534261/how-do-you-keep-user-config-settings-across-different-assembly-versions-in-net
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            #endregion

            #region Language

            try
            {
                LocalizationHelper.SelectCulture(Settings.Default.Language);
            }
            catch (Exception ex)
            {
                var errorViewer = new ExceptionViewer(ex);
                errorViewer.ShowDialog();
                LogWriter.Log(ex, "Language Settings Exception.");
            }

            #endregion

            //var select = new SelectFolderDialog();
            //select.ShowDialog();

            //var select = new TestField();
            //select.ShowDialog();

            //return;

            try
            {
                #region Startup

                if (Settings.Default.StartUp == 0)
                {
                    var startup = new Startup();
                    Current.MainWindow = startup;
                    startup.ShowDialog();
                }
                else if (Settings.Default.StartUp == 4 || Argument.FileNames.Any())
                {
                    var edit = new Editor();
                    Current.MainWindow = edit;
                    edit.ShowDialog();
                }
                else
                {
                    var editor = new Editor();
                    List<FrameInfo> frames = null;
                    var exitArg = ExitAction.Exit;
                    bool? result = null;

                    #region Recorder, Webcam or Border

                    switch (Settings.Default.StartUp)
                    {
                        case 1:
                            var rec = new Recorder(true);
                            Current.MainWindow = rec;

                            result = rec.ShowDialog();
                            exitArg = rec.ExitArg;
                            frames = rec.ListFrames;
                            break;
                        case 2:
                            var web = new Windows.Webcam(true);
                            Current.MainWindow = web;

                            result = web.ShowDialog();
                            exitArg = web.ExitArg;
                            frames = web.ListFrames;
                            break;
                        case 3:
                            var board = new Board();
                            Current.MainWindow = board;

                            result = board.ShowDialog();
                            exitArg = board.ExitArg;
                            frames = board.ListFrames;
                            break;
                    }

                    #endregion

                    if (result.HasValue && result.Value)
                    {
                        #region If Close

                        Environment.Exit(0);

                        #endregion
                    }
                    else if (result.HasValue)
                    {
                        #region If Backbutton or Stop Clicked

                        if (exitArg == ExitAction.Recorded)
                        {
                            editor.ListFrames = frames;
                            Current.MainWindow = editor;
                            editor.ShowDialog();
                        }

                        #endregion
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                var errorViewer = new ExceptionViewer(ex);
                errorViewer.ShowDialog();
                LogWriter.Log(ex, "Generic Exception - Root");
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            //TODO: Save all settings, stop all encoding.
        }

        #region Exception Handling

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogWriter.Log(e.Exception, "On Dispacher Unhandled Exception - Unknow");

            try
            {
                var errorViewer = new ExceptionViewer(e.Exception);
                errorViewer.ShowDialog();
            }
            catch (Exception)
            {}
            
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null) return;

            LogWriter.Log(exception, "Current Domain Unhandled Exception - Unknow");

            try
            {
                var errorViewer = new ExceptionViewer(exception);
                errorViewer.ShowDialog();
            }
            catch (Exception)
            {}
        }

        #endregion
    }
}
