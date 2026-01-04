using HASS.Agent.Shared.Enums;
using HASS.Agent.Shared.Functions;

namespace HASS.Agent.Shared.Extensions
{
    /// <summary>
    /// Extensions for HASS.Agent command objects
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Returns the name of the commandtype
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static string GetCommandName(this CommandType commandType)
        {
            var (_, name) = commandType.GetLocalizedDescriptionAndKey();
            return name.ToLower();
        }
    }
}
