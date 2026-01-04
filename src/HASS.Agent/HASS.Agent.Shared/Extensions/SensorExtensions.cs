using HASS.Agent.Shared.Enums;
using HASS.Agent.Shared.Functions;

namespace HASS.Agent.Shared.Extensions
{
    /// <summary>
    /// Extensions for HASS.Agent sensor objects
    /// </summary>
    public static class SensorExtensions
    {
        /// <summary>
        /// Returns the name of the sensortype
        /// </summary>
        /// <param name="sensorType"></param>
        /// <returns></returns>
        public static string GetSensorName(this SensorType sensorType)
        {
            var (_, name) = sensorType.GetLocalizedDescriptionAndKey();
            return name.ToLower();
        }
    }
}
