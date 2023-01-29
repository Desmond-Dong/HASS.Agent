﻿using System.Collections.Generic;
using System.Linq;
using HASS.Agent.Shared.Functions;
using HASS.Agent.Shared.Models.HomeAssistant;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HASS.Agent.Shared.HomeAssistant.Sensors.GeneralSensors.SingleValue
{
    /// <summary>
    /// Sensor indicating whether the webcam is in use
    /// </summary>
    public class WebcamProcessSensor : AbstractSingleValueSensor
    {
        public WebcamProcessSensor(int? updateInterval = null, string name = "webcamprocess", string id = default, bool useAttributes = true) : base(name ?? "webcamprocess", updateInterval ?? 10, id, useAttributes)
        {
            //
        }

        private Dictionary<string, string> processes = new Dictionary<string, string>();

        private string _attributes = string.Empty;

        public override string GetState() => WebcamProcess();
        public void SetAttributes(string value) => _attributes = string.IsNullOrWhiteSpace(value) ? "{}" : value;
        public override string GetAttributes() => _attributes;

        public override DiscoveryConfigModel GetAutoDiscoveryConfig()
        {
            if (Variables.MqttManager == null) return null;

            var deviceConfig = Variables.MqttManager.GetDeviceConfigModel();
            if (deviceConfig == null) return null;

            var model = new SensorDiscoveryConfigModel()
            {
                Name = Name,
                Unique_id = Id,
                Device = deviceConfig,
                State_topic = $"{Variables.MqttManager.MqttDiscoveryPrefix()}/{Domain}/{deviceConfig.Name}/{ObjectId}/state",
                Availability_topic = $"{Variables.MqttManager.MqttDiscoveryPrefix()}/sensor/{deviceConfig.Name}/availability",
                Icon = "mdi:webcam"
            };

            if (UseAttributes)
            {
                model.Json_attributes_topic = $"{Variables.MqttManager.MqttDiscoveryPrefix()}/{Domain}/{deviceConfig.Name}/{ObjectId}/attributes";
            }

            return AutoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(model);
        }

        private string WebcamProcess()
        {
            const string regKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam";

            this.processes.Clear();

            // first local machine
            using (var key = Registry.LocalMachine.OpenSubKey(regKey))
            {
                CheckRegForWebcamInUse(key);
            }

            // then current user
            using (var key = Registry.CurrentUser.OpenSubKey(regKey))
            {
                CheckRegForWebcamInUse(key);
            }

            if (this.processes.Count > 0)
            {
                _attributes = JsonConvert.SerializeObject(this.processes, Formatting.Indented);
            }

            // nope
            return this.processes.Count.ToString();
        }

        private void CheckRegForWebcamInUse(RegistryKey key)
        {
            if (key == null) return;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                // NonPackaged has multiple subkeys
                if (subKeyName == "NonPackaged")
                {
                    using var nonpackagedkey = key.OpenSubKey(subKeyName);
                    if (nonpackagedkey == null) continue;

                    foreach (var nonpackagedSubKeyName in nonpackagedkey.GetSubKeyNames())
                    {
                        using var subKey = nonpackagedkey.OpenSubKey(nonpackagedSubKeyName);
                        if (subKey == null || !subKey.GetValueNames().Contains("LastUsedTimeStop")) continue;

                        var endTime = subKey.GetValue("LastUsedTimeStop") is long
                            ? (long)(subKey.GetValue("LastUsedTimeStop") ?? -1)
                            : -1;

                        if (endTime <= 0)
                        {
                            this.processes.Add(SharedHelperFunctions.ParseRegWebcamMicApplicationName(subKey.Name), "on");
                        }
                    }
                }
                else
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null || !subKey.GetValueNames().Contains("LastUsedTimeStop")) continue;

                    var endTime = subKey.GetValue("LastUsedTimeStop") is long ? (long)(subKey.GetValue("LastUsedTimeStop") ?? -1) : -1;
                    if (endTime <= 0)
                    {
                        this.processes.Add(SharedHelperFunctions.ParseRegWebcamMicApplicationName(subKey.Name), "on");
                    }
                }
            }
        }
    }
}
