namespace Drachenkatze.PresetMagician.VSTHost.VST
{

        public interface IBankFile
        {
            string Path { get; set; }
            string BankName { get; set; }
            string ProgramRange { get; set; }
        }
    
}