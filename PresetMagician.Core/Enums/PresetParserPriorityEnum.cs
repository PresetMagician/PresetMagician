namespace PresetMagician.Core.Enums
{
    public enum PresetParserPriorityEnum
    {
        /// <summary>
        /// The default priority for a preset parser
        /// </summary>
        DEFAULT_PRIORITY = 0,
        
        /// <summary>
        /// The priority for a generic VST preset parser
        /// </summary>
        GENERIC_VST_PRIORITY = -98,
        
        /// <summary>
        /// The priority for a preset parser which knows that the plugin doesn't have load/save capabilities
        /// </summary>
        VOID_PRIORITY = -99,
        
        /// <summary>
        /// The priority for the default (null) preset parser 
        /// </summary>
        NULL_PRIORITY = -100
    }
}