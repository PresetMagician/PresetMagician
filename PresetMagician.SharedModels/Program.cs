using System;
using System.Data.Entity;
using PresetMagician.Models;


namespace SharedModels
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var context = new ApplicationDatabaseContext("Default");
            context.Plugins.Load();
        }
    }
}