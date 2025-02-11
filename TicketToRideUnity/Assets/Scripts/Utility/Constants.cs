using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using System.Security.AccessControl;

namespace Assets.Scripts.Utility
{

    /**
     * Diese Klasse stellt einige statische Methoden zur Verf�gung, die f�r die KIs n�tig sind.
     */
    public class Constants
    {
        /**
         * Die (fixe) Adresse des Hosts, auf den verbunden werden soll.
         */
        public const string HOST_ADDRESS = "127.0.0.1";
        /**
         * Der (fixe) Port, auf dem verbunden werden soll.
         */
        public const int PORT = 5555;
        /**
         * Name des Haupt-Portals.
         */
        public const string MAIN_PORTAL_NAME = "MainPortal";
        /**
         * Name des Sub-Portals.
         */
        public const string PORTAL_NAME = "Portal";
        /**
         * Name bzw. Pfad der JAR Datei des CBR-Systems.
         */
        public const string SERVER_PATH = "CBRSystem.jar";
        /**
         * Der Name des Kommunikationsagenten.
         */
        public const string COMMUNICATION_AGENT_NAME = "CommunicationAgent";
        /**
         * Der Name des Planungsagenten.
         */
        public const string PLANNING_AGENT_NAME = "PlanningAgent";

        /**
         * Der Prozess, der f�r den Start und die Terminierung des CBR-Systems (als JAR) ben�tigt wird.
         */
        public static Process proc;

        /**
         * Konstanter Pfad zur Log-Datei.
         */
        private const string TEMP_PATH = @"log.txt";
        /**
         * Der aktuelle Pfad als String.
         */
        public static string PATH = Directory.GetCurrentDirectory();
        /**
         * Pfad zum 'Saves'-Ordner, der f�r die Speicherung der Statistiken ben�tigt wird.
         */
        public static string SAVES_PATH = PATH + PATH_DELIMITER + "Saves";
        /**
         * File Ending als String.
         */
        private const string FILE_ENDING = @".csv";
        /**
         * Trennzeichen f�r die csv-Datei als char.
         */
        private const char DELIMITER = ';';
        /**
         * Unterstrich als char.
         */
        private const char UNDERSCORE = '_';
        /**
         * Trennzeichen f�r einen Pfad als String.
         */
        private const string PATH_DELIMITER = @"\";
        ///**
        // * Der Pfad zum Ordner des CBR-Spielers - erstmal None, wird sp�ter korrekt bef�llt.
        // */
        //private static string CBR_PLAYER_PATH = "NONE";
        ///**
        // * Der Pfad zum Ordner des Non-CBR-Spielers - erstmal None, wird sp�ter korrekt bef�llt.
        // */
        //private static string NON_CBR_PLAYER_PATH = "NONE";
        ///**
        // * Der Name des CBR-Spielers, wird f�r den Ordnernamen ben�tigt.
        // */
        //private static string cbrPlayerName = null;


        /**
         * Diese Methode erstellt einen Ordner am gegebenen Pfad, insofern dort kein Ordner mit dem Namen existiert.
         */
        public static void CreateFolderIfDoesNotExistYet(string path)
        {
            Directory.CreateDirectory(path);
        }

        /**
         * Methode zum Starten des Java-Servers, der f�r die Steuerung der KIs zust�ndig ist
         */
        public static void StartServer(bool window)
        {
            proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = "java";
            proc.StartInfo.Arguments = "-jar " + SERVER_PATH + " " + PORT;
            if (!window)
            {
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
            }
            proc.Start();
            UnityEngine.Debug.Log(proc.StartInfo.Arguments);
        }
    }

}
