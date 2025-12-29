using HASS.Agent.Forms;
using HASS.Agent.Shared.Enums;
using Serilog;
using Syncfusion.Windows.Forms;

namespace HASS.Agent.Managers
{
    /// <summary>
    /// Manages the status display of various components in the main UI
    /// </summary>
    public class ComponentStatusManager
    {
        private readonly Main _mainForm;

        public ComponentStatusManager(Main mainForm)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
        }

        /// <summary>
        /// Sets the status of a component and updates the UI accordingly
        /// </summary>
        public void SetComponentStatus(string componentName, ComponentStatus status, string statusText = null)
        {
            try
            {
                _mainForm.Invoke(new MethodInvoker(async () =>
                {
                    await SetComponentStatusAsync(componentName, status, statusText);
                }));
            }
            catch (Exception ex)
            {
                Log.Error("[COMPONENTSTATUS] Error setting status for {Component}: {Err}", componentName, ex.Message);
            }
        }

        private async Task SetComponentStatusAsync(string componentName, ComponentStatus status, string statusText = null)
        {
            try
            {
                var statusCtrl = _mainForm.Controls.Find($"LvStatus{componentName}", true).FirstOrDefault() as ListViewAdv;
                if (statusCtrl == null)
                {
                    Log.Warning("[COMPONENTSTATUS] Status control not found for: {Component}", componentName);
                    return;
                }

                statusCtrl.Items.Clear();

                var item = new ListViewItem(statusText ?? GetComponentStatusText(status))
                {
                    ImageKey = GetComponentImageKey(status)
                };

                statusCtrl.Items.Add(item);

                Log.Debug("[COMPONENTSTATUS] {Component} status set to: {Status}", componentName, status);
            }
            catch (Exception ex)
            {
                Log.Error("[COMPONENTSTATUS] Error updating UI for {Component}: {Err}", componentName, ex.Message);
            }
        }

        private string GetComponentStatusText(ComponentStatus status)
        {
            return status switch
            {
                ComponentStatus.Loading => Languages.Status_Loading,
                ComponentStatus.Ready => Languages.Status_Ready,
                ComponentStatus.Error => Languages.Status_Error,
                ComponentStatus.Disabled => Languages.Status_Disabled,
                _ => Languages.Status_Unknown
            };
        }

        private string GetComponentImageKey(ComponentStatus status)
        {
            return status switch
            {
                ComponentStatus.Loading => "radar_48.png",
                ComponentStatus.Ready => "service_32.png",
                ComponentStatus.Error => "exit_32.png",
                ComponentStatus.Disabled => "hide_32.png",
                _ => "question_32.png"
            };
        }

        /// <summary>
        /// Sets all components to loading status
        /// </summary>
        public void SetAllLoading()
        {
            SetComponentStatus("LocalApi", ComponentStatus.Loading);
            SetComponentStatus("HassApi", ComponentStatus.Loading);
            SetComponentStatus("Mqtt", ComponentStatus.Loading);
            SetComponentStatus("Sensors", ComponentStatus.Loading);
            SetComponentStatus("Commands", ComponentStatus.Loading);
            SetComponentStatus("QuickActions", ComponentStatus.Loading);
            SetComponentStatus("Service", ComponentStatus.Loading);
        }
    }
}
